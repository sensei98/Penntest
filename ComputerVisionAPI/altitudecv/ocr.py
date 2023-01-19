import logging
import cv2
import numpy as np
import imutils
import pytesseract

horizontal_array = [
    [0, 0, 0, 0, 0],
    [0, 0, 0, 0, 0],
    [1, 1, 1, 1, 1],
    [0, 0, 0, 0, 0],
    [0, 0, 0, 0, 0]
]

vertical_array = [
    [0, 0, 1, 0, 0],
    [0, 0, 1, 0, 0],
    [0, 0, 1, 0, 0],
    [0, 0, 1, 0, 0],
    [0, 0, 1, 0, 0]
]

def predict_altitude(input, tesseract_dir):
    pytesseract.pytesseract.tesseract_cmd = tesseract_dir + '/tesseract.exe'

    img = cv2.imread(input, cv2.IMREAD_GRAYSCALE)

    if img is None:
        return

    img = cv2.bitwise_not(img)

    img_cropped_top_btm = crop_top_btm_noise(img, 0.2)

    img_digits = crop_altitude_box(img_cropped_top_btm)

    img_ocr_resized = imutils.resize(img_digits, height=50)
    img_ocr_resized = cv2.dilate(img_ocr_resized, (3, 3), 0)

    result = ocr(img_ocr_resized, img_digits)

    logging.info('Result:' + result)

    if not result:
        return

    return int(result)


def crop_altitude_box(src):
    upscaled = imutils.resize(src, height=1000)
    resized_height, resized_width = upscaled.shape[:2]
    cropped_top_btm_height, cropped_top_btm_width = src.shape[:2]
    edged = preprocess_edged(upscaled)

    horizontal = edged_to_horizontal(edged, horizontal_array)
    vertical = edged_to_horizontal(edged, vertical_array)

    try:
        cnt1_y, cnt2_y, cnt1_x, cnt2_x = cnts_by_houghline(horizontal, vertical)
    except:
        cnt1_y, cnt2_y, cnt1_x, cnt2_x = cnts_by_widest_cnts(horizontal)
        logging.info('Houghline approach failed. Trying widest contour approach..')

    cnt1_y = cnt1_y + (resized_height * .015)
    cnt2_y = cnt2_y - (resized_height * .015)

    preprocessed_to_cropped_multiplier = cropped_top_btm_height / resized_height
    cropped = crop_cnts(src, cnt1_y, cnt2_y, cnt1_x, cnt2_x,
                        preprocessed_to_cropped_multiplier)
    return cropped


def cnts_by_houghline(horizontal, vertical):
    height, width = horizontal.shape[:2]
    horizontal_lines, _ = houghline(horizontal)
    _, vertical_lines = houghline(vertical)
    cnt1_y = min(horizontal_lines)
    cnt2_y = max(horizontal_lines)
    cnt1_x = min(vertical_lines)
    cnt2_x = max(vertical_lines)

    min_cnt_distance = height * 0.1
    is_min_y_near_max_y = cnt2_y - cnt1_y <= min_cnt_distance
    is_min_x_near_max_x = cnt2_x - cnt1_x <= min_cnt_distance

    if is_min_y_near_max_y or is_min_x_near_max_x:
        raise Exception()

    return cnt1_y, cnt2_y, cnt1_x, cnt2_x


def cnts_by_widest_cnts(src):
    height, width = src.shape[:2]
    widest_cnt, scnd_widest_cnt = find_widest_cnts(src, height, 0.1)
    cnt1_y, cnt2_y = order_cnts(widest_cnt, scnd_widest_cnt)
    cnt1_x = 0
    cnt2_x = width
    return cnt1_y, cnt2_y, cnt1_x, cnt2_x


def houghline(src):
    horizontal_lines = []
    vertical_lines = []
    lines = cv2.HoughLines(src, 1, np.pi / 180, 200)

    for r_theta in lines:
        arr = np.array(r_theta[0], dtype=np.float64)
        r, theta = arr
        a = np.cos(theta)
        b = np.sin(theta)
        x0 = a * r
        y0 = b * r
        x1 = int(x0 + 1000 * -b)
        y1 = int(y0 + 1000 * a)

        horizontal_lines.append(y1)
        vertical_lines.append(x1)
        
    return horizontal_lines, vertical_lines


def find_widest_cnts(src, input_height, min_cnt_y_distance_pct):
    contours, _ = cv2.findContours(src, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)

    min_cnt_y_distance = input_height * min_cnt_y_distance_pct

    widest_cnt = contours[0]
    scnd_widest_cnt = contours[0]

    for cnt in contours:
        x, y, w, h = cv2.boundingRect(cnt)
        _, widest_cnt_y, widest_cnt_w, _ = cv2.boundingRect(widest_cnt)
        _, scnd_widest_cnt_y, scnd_widest_cnt_w, _ = cv2.boundingRect(scnd_widest_cnt)

        is_y_near_widest_cnt_y = widest_cnt_y - min_cnt_y_distance <= y <= widest_cnt_y + min_cnt_y_distance
        is_y_near_scnd_widest_cnt_y = scnd_widest_cnt_y - min_cnt_y_distance <= y <= scnd_widest_cnt_y + min_cnt_y_distance

        # Prevent neighbouring contours
        if is_y_near_widest_cnt_y:
            if w > widest_cnt_w:
                widest_cnt = cnt
            continue
        if is_y_near_scnd_widest_cnt_y:
            if w > scnd_widest_cnt_w:
                scnd_widest_cnt = cnt
            continue

        # Overwrite stored contours if it is wider
        if w > scnd_widest_cnt_w:
            if w > widest_cnt_w:
                scnd_widest_cnt = widest_cnt[:]
                widest_cnt = cnt
                continue
            scnd_widest_cnt = cnt

    return widest_cnt, scnd_widest_cnt

 
# Remove all edges that are not horizontal
def edged_to_horizontal(src, kernel_array):
    h_kernel_large = np.array(kernel_array, np.uint8)
    edged = cv2.morphologyEx(src, cv2.MORPH_OPEN, h_kernel_large, iterations=2)  # Detect horizontal lines
    # edged = cv2.morphologyEx(edged, cv2.MORPH_CLOSE, h_kernel_large)  # Close gaps in the lines
    return edged


def preprocess_edged(src):
    edged = cv2.GaussianBlur(src, (3, 3), 0)
    edged = cv2.Canny(edged, 50, 200, 255)
    _, edged = cv2.threshold(edged, 5, 255, cv2.THRESH_BINARY)
    return edged


# Remove upper and lower part of the image as they may influence the results
def crop_top_btm_noise(src, crop_pct):
    input_height, input_width = src.shape[:2]
    px_to_crop = int(input_height * crop_pct)
    cropped_top_btm = src[px_to_crop:input_height - px_to_crop, 0:input_width]
    return cropped_top_btm


def order_cnts(widest_cnt, scnd_widest_cnt):
    _, widest_cnt_y, _, _ = cv2.boundingRect(widest_cnt)
    _, scnd_widest_cnt_y, _, _ = cv2.boundingRect(scnd_widest_cnt)
    portions = [widest_cnt, scnd_widest_cnt] if (widest_cnt_y < scnd_widest_cnt_y) else [scnd_widest_cnt, widest_cnt]
    _, cnt1_y, _, _ = cv2.boundingRect(portions[0])
    _, cnt2_y, _, _ = cv2.boundingRect(portions[1])
    return cnt1_y, cnt2_y


# Use the detected contours to crop the altitude value
def crop_cnts(src, cnt1_y, cnt2_y, cnt1_x, cnt2_x, multiplier):
    return src[int(cnt1_y * multiplier):int((cnt2_y * multiplier)) - 0,
           int(cnt1_x * multiplier):int(cnt2_x * multiplier)]


def ocr(ocr_src, ocr_src_fallback):
    result = pytesseract.image_to_string(ocr_src, lang='eng', config='--psm 6 outputbase digits')
    logging.info('First OCR:' + result)

    result = correct_ocr_output(result)

    if not result or not result.isnumeric():
        logging.info('OCR failed. Reattempting with different settings...')
        ocr_preprocessed = cv2.dilate(ocr_src_fallback, (3, 3), iterations=1)
        ocr_preprocessed = cv2.erode(ocr_preprocessed, (3, 3), iterations=1)

        result = pytesseract.image_to_string(ocr_preprocessed, lang='eng', config='--psm 6 outputbase digits')
        result = correct_ocr_output(result)

        if not result or not result.isnumeric():
            logging.info('OCR with different settings failed. Reattempting with original size...')

            result = pytesseract.image_to_string(ocr_src_fallback, lang='eng', config='--psm 6 outputbase digits')
            result = correct_ocr_output(result)

            if not result or not result.isnumeric():
                logging.info('Final attempt failed')
                return ""
    return result


def correct_ocr_output(text):
    if not text:
        return ''

    # Remove first character if it is not numeric (eg. -49 > 49)
    if not text[0].isnumeric():
        text = text[1:]

    # Take first part of string if it contains characters (eg. 49.60 > 49)
    text = text.split('.', 1)[0].split('-', 1)[0]

    text = text.replace('\n', '').replace('-', '').replace('.', '')
    return text
