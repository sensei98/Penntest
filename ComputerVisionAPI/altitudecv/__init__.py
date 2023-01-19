import logging
import io
import os
import tempfile
from PIL import Image
from zipfile import ZipFile
import azure.core.exceptions
import azure.storage.fileshare
from altitudecv.ocr import predict_altitude

import azure.functions as func

DEPENDENCIES_SHARE_NAME = os.environ["DependenciesShareName"]
SHARE_TESSERACT_PATH = 'tesseract/tesseract.zip'
TESSERACT_DIR = f'{tempfile.gettempdir()}/tesseract'
OUTPUT_ZIP_NAME = f'{tempfile.gettempdir()}/tesseract.zip'
IMG_PATH = f'{tempfile.gettempdir()}/input.png'
CONN_STRING = os.environ["AzureWebJobsStorage"]


def main(req: func.HttpRequest) -> func.HttpResponse:
    body = req.get_body() 

    try:
        img = Image.open(io.BytesIO(body))
    except:
        return func.HttpResponse("Body should be a valid image", status_code=400)

    img.save(IMG_PATH)
    
    zip = fetch_tesseract_zip()
    zip.extractall(TESSERACT_DIR + '/')

    try:
        prediction = predict_altitude(IMG_PATH, TESSERACT_DIR)
    except:
        return func.HttpResponse("Image could not be analysed", status_code=400)

    return func.HttpResponse(str(prediction), status_code=200)


def fetch_tesseract_zip():
    file_client = azure.storage.fileshare.ShareFileClient.from_connection_string(CONN_STRING, DEPENDENCIES_SHARE_NAME, SHARE_TESSERACT_PATH)
    download_tesseract(file_client, OUTPUT_ZIP_NAME)
    return ZipFile(OUTPUT_ZIP_NAME, 'r')


def download_tesseract(file_client, output_name):
    try:
        with open(output_name, "wb") as data:
            stream = file_client.download_file()
            data.write(stream.readall())
    except azure.core.exceptions.ResourceNotFoundError as ex:
        logging.error("ResourceNotFoundError:", ex.message)