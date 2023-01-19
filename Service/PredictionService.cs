using VRefSolutions.Service.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using AzureCVPrediction = Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using AzureCVPredictionModels = Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using AzureCVTraining = Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using AzureCV = Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using BoundingBox = Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models.BoundingBox;
using Azure.Storage.Queues;
using Newtonsoft.Json;
using VRefSolutions.Domain.Models;
using Azure.Storage.Blobs;
using SixLabors.ImageSharp.Processing;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Web;
using VRefSolutions.Repository.Interfaces;
using VRefSolutions.Domain.Entities;
using Google.Protobuf.WellKnownTypes;
using VRefSolutions.Repository;
using VRefSolutions.Service.Util;
using Microsoft.Extensions.Logging;

namespace VRefSolutions.Service
{
    public class PredictionService : IPredictionService
    {
        readonly string CUSTOMVISION_PREDICTION_KEY = Environment.GetEnvironmentVariable("CustomVisionPredictionKey", EnvironmentVariableTarget.Process);
        readonly string CUSTOMVISION_PREDICTION_ENDPOINT = Environment.GetEnvironmentVariable("CustomVisionPredictionEndpoint", EnvironmentVariableTarget.Process);
        readonly string CUSTOMVISION_TRAINING_KEY = Environment.GetEnvironmentVariable("CustomVisionTrainingKey", EnvironmentVariableTarget.Process);
        readonly string CUSTOMVISION_TRAINING_ENDPOINT = Environment.GetEnvironmentVariable("CustomVisionTrainingEndpoint", EnvironmentVariableTarget.Process);
        readonly string COMPUTERVISION_KEY = Environment.GetEnvironmentVariable("ComputerVisionKey", EnvironmentVariableTarget.Process);
        readonly string COMPUTERVISION_ENDPOINT = Environment.GetEnvironmentVariable("ComputerVisionEndpoint", EnvironmentVariableTarget.Process);
        readonly string COMPUTERVISION_AZURE_ENDPOINT = Environment.GetEnvironmentVariable("ComputerVisionAzureEndpoint", EnvironmentVariableTarget.Process);
        readonly string CONNECTION_STRING = Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);
        readonly string CV_FUNCTIONAPP_ENDPOINT = Environment.GetEnvironmentVariable("CvFunctionAppEndpoint", EnvironmentVariableTarget.Process);
        readonly string CV_FUNCTIONAPP_PREDICTION_ROUTE = Environment.GetEnvironmentVariable("CvFunctionAppPredictionRoute", EnvironmentVariableTarget.Process);
        readonly string PROJECT_NAME = Environment.GetEnvironmentVariable("CustomVisionProjectName", EnvironmentVariableTarget.Process);
        readonly string MODEL_NAME = Environment.GetEnvironmentVariable("CustomVisionModelName", EnvironmentVariableTarget.Process);
        readonly double MIN_PROBABILITY = 0.1;
        readonly double MAX_ALT_DIFFERENCE = 20;
        const string ALT_TAG = "alt";
        const string ECAM_TAG = "ecam";
        const string ECAM_EWD_TAG = "ecam_ewd";
        const string ALTITUDE_POSITION_QUEUE = "altitude-position";
        const string ECAM_POSITION_QUEUE = "ecam-position";
        const string TRAINING_SNAPSHOT_BLOB_CONTAINER = "training-snapshot";

        private ILogger<PredictionService> Logger { get; }
        private CustomVisionPredictionClient PredictionApi { get; set; }
        private CustomVisionTrainingClient TrainingApi { get; set; }
        private ComputerVisionClient ComputerVisionClient { get; set; }
        private HttpClient HttpClient { get; set; }
        private IEcamMessageRepository EcamMessageRepository { get; set; }
        private IEventTypeService EventTypeService { get; set; }
        private IEventService EventService { get; set; }
        private IAltitudeService AltitudeService { get; set; }
        private ITrainingService TrainingService { get; set; }
        private ITrainingStateRepository TrainingStateRepository { get; set; }

        public PredictionService(
            ILogger<PredictionService> logger,
            IEcamMessageRepository ecamMessageRepository,
            IEventTypeService eventTypeService,
            IEventService eventService,
            IAltitudeService altitudeService,
            ITrainingService trainingService,
            ITrainingStateRepository trainingStateRepository)
        {
            Logger = logger;
            EcamMessageRepository = ecamMessageRepository;
            EventTypeService = eventTypeService;
            EventService = eventService;
            AltitudeService = altitudeService;
            TrainingService = trainingService;
            TrainingStateRepository = trainingStateRepository;
            TrainingApi = AuthenticateTraining(CUSTOMVISION_TRAINING_ENDPOINT, CUSTOMVISION_TRAINING_KEY);
            PredictionApi = AuthenticatePrediction(CUSTOMVISION_PREDICTION_ENDPOINT, CUSTOMVISION_PREDICTION_KEY);
            ComputerVisionClient = AuthenticateComputerVision(COMPUTERVISION_KEY, COMPUTERVISION_ENDPOINT);
            HttpClient = new HttpClient();
        }

        public async Task PredictInstruments(int trainingId, TimeStamp timestamp, byte[] image)
        {
            if (GetProject(out Project project) is null)
                throw new Exception("Custom Vision training could not be found");

            string snapshotBlobName = await StoreTrainingSnapshotBlob(trainingId, timestamp, image);

            // May fail if the Custom Vision model is not published under the right name and the Prediction key is not using the model's key
            AzureCVPredictionModels.ImagePrediction imagePredictionResults = PredictionApi.DetectImage(project.Id, MODEL_NAME, new MemoryStream(image));

            IDictionary<string, PredictionModel> detectedInstruments = GetHighestObjectPredictions(imagePredictionResults);

            HandleAltitudePosition(detectedInstruments, trainingId, timestamp, snapshotBlobName);
            HandleEcamPosition(detectedInstruments, trainingId, timestamp, snapshotBlobName);
        }

        public async Task PredictEcamEvents(PositionMessage positionMessage)
        {
            BlobContainerClient snapshotBlobContainerClient = new(CONNECTION_STRING, TRAINING_SNAPSHOT_BLOB_CONTAINER);
            BlobClient snapshotBlob = snapshotBlobContainerClient.GetBlobClient(positionMessage.SnapshotBlobName);

            if (!snapshotBlob.Exists())
                throw new Exception("Snapshot blob does not exist"); 

            Image<Rgba32> img = await BlobToImage(snapshotBlob);
            
            byte[] ecamCrop = Crop(img, positionMessage.Position);

            List<string> detectedMessages = await ReadEcam(ecamCrop);

            CreateEventsFromEcamMessages(detectedMessages, positionMessage.TrainingId, positionMessage.Timestamp);
        }

        public async Task PredictAltitude(PositionMessage positionMessage)
        {
            BlobContainerClient snapshotBlobContainerClient = new(CONNECTION_STRING, TRAINING_SNAPSHOT_BLOB_CONTAINER);
            BlobClient snapshotBlob = snapshotBlobContainerClient.GetBlobClient(positionMessage.SnapshotBlobName);

            // Skip the process if the blob failed uploading or has been deleted already
            if (!snapshotBlob.Exists())
                throw new Exception("Snapshot blob does not exist");

            Image<Rgba32> img = await BlobToImage(snapshotBlob);

            byte[] altitudeCrop = Crop(img, positionMessage.Position);

            StreamContent content = new(new MemoryStream(altitudeCrop));
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            HttpResponseMessage response = await HttpClient.PostAsync($"{CV_FUNCTIONAPP_ENDPOINT}/{CV_FUNCTIONAPP_PREDICTION_ROUTE}", content);
            response.EnsureSuccessStatusCode();

            string amslString = await response.Content.ReadAsStringAsync();

            if (!int.TryParse(amslString, out int amsl))
                throw new Exception("OCR output could not be parsed to an integer");

            CreateAltitude(amsl, positionMessage.TrainingId, positionMessage.Timestamp);

            await DeleteSnapshotBlob(positionMessage.SnapshotBlobName);
        }

        private void CreateEventsFromEcamMessages(List<string> ecamMessages, int trainingId, TimeStamp timestamp)
        {
            TrainingState? trainingState = TrainingStateRepository.GetByTrainingId(trainingId);
            Training training = TrainingService.GetTrainingById(trainingId);

            if (trainingState is null)
                throw new Exception("Training does not have a training state");

            for (int i = 0; i < ecamMessages.Count; i++)
            {
                // Skip message if it already exists in the state
                if (trainingState.EcamMessages.Contains(ecamMessages[i]))
                    continue;

                // New message, so create new event
                EventType? matchingEventType = EventTypeService.GetByName(ecamMessages[i]);

                if (matchingEventType is null) { continue; }

                EventService.CreateEvent(new()
                {
                    Training = training,
                    EventType = matchingEventType,
                    OverwriteMessage = "",
                    OverwriteName = "",
                    OverwriteSymbol = "",
                    TimeStamp = timestamp
                });
            }

            trainingState.EcamMessages = ecamMessages.ToArray();
            TrainingStateRepository.Update(trainingState);
        }

        private void CreateAltitude(int amsl, int trainingId, TimeStamp timestamp)
        {
            TrainingState? trainingState = TrainingStateRepository.GetByTrainingId(trainingId);
            Training training = TrainingService.GetTrainingById(trainingId);

            if (trainingState is null)
                throw new Exception("Training does not have a training state.");

            bool didAltitudeExceedAllowedDifference = (trainingState.Altitude < amsl - MAX_ALT_DIFFERENCE) || (trainingState.Altitude > amsl + MAX_ALT_DIFFERENCE);
            if (didAltitudeExceedAllowedDifference)
                return;

            HandleAltitudeEvents(amsl, trainingState.Altitude, trainingId, timestamp);

            trainingState.Altitude = amsl;
            TrainingStateRepository.Update(trainingState);

            AltitudeService.CreateAltitude(new()
            {
                Training = training,
                Amsl = amsl,
                TimeStamp = timestamp
            });
        }

        private void HandleAltitudeEvents(int newAltitude, int prevAltitude, int trainingId, TimeStamp timestamp)
        {
            if (prevAltitude == 0 && newAltitude > 0)
            {
                CreateEventFromEventTypeName("Takeoff", trainingId, timestamp);
                return;
            }
            if (newAltitude == 0 && prevAltitude > 0)
            {
                CreateEventFromEventTypeName("Landing", trainingId, timestamp);
                return;
            }
        }

        private void HandleAltitudePosition(IDictionary<string, PredictionModel> detectedInstruments, int trainingId, TimeStamp timestamp, string snapshotBlobName)
        {
            if (detectedInstruments.TryGetValue(ALT_TAG, out PredictionModel? detectedAlt))
            {
                SendAltitudePositionQueueMessage(detectedAlt.BoundingBox, trainingId, timestamp, snapshotBlobName);
            }
        }

        private void HandleEcamPosition(IDictionary<string, PredictionModel> detectedInstruments, int trainingId, TimeStamp timestamp, string snapshotBlobName)
        {
            detectedInstruments.TryGetValue(ECAM_EWD_TAG, out PredictionModel? detectedEcamEwd);
            detectedInstruments.TryGetValue(ECAM_TAG, out PredictionModel? detectedEcam);

            if (detectedEcamEwd is not null || detectedEcam is not null)
            {
                BoundingBox? bestEcamBoundingBox = FindBestEcamBoundingBox(detectedEcamEwd, detectedEcam);

                if (bestEcamBoundingBox is not null)
                    SendEcamPositionQueueMessage(bestEcamBoundingBox, trainingId, timestamp, snapshotBlobName);
            }
        }

        private IDictionary<string, PredictionModel> GetHighestObjectPredictions(AzureCVPredictionModels.ImagePrediction predictions)
        {
            IDictionary<string, PredictionModel> detectedInstruments = new Dictionary<string, PredictionModel>();

            for (int i = 0; i < predictions.Predictions.Count; i++)
            {
                PredictionModel prediction = predictions.Predictions[i];

                if (prediction.Probability < MIN_PROBABILITY) continue;

                // Since the prediction results are ordered by probability, only the first occurence of a tag is needed
                switch (prediction.TagName)
                {
                    case ALT_TAG:
                        if (!detectedInstruments.TryGetValue(prediction.TagName, out _))
                        {
                            detectedInstruments.Add(prediction.TagName, prediction);
                            break;
                        }
                        break;
                    case ECAM_TAG:
                        if (!detectedInstruments.TryGetValue(prediction.TagName, out _))
                        {
                            detectedInstruments.Add(prediction.TagName, prediction);
                            break;
                        }
                        break;
                    case ECAM_EWD_TAG:
                        if (!detectedInstruments.TryGetValue(prediction.TagName, out _))
                        {
                            detectedInstruments.Add(prediction.TagName, prediction);
                            break;
                        }
                        break;
                }
            }

            return detectedInstruments;
        }

        private async Task<List<string>> ReadEcam(byte[] image)
        {
            List<string> words = await ReadImageText(image);

            List<EcamMessage> ecamMessages = EcamMessageRepository.GetAll().ToList();

            List<string> detectedMessages = LevenshteinDistance.CorrectMessages(words, ecamMessages);

            List<string> filteredMessages = new();

            for (int i = 0; i < detectedMessages.Count; i++)
            {
                if (!IsEcamMessageAccepted(detectedMessages[i], ecamMessages))
                {
                    continue;
                }

                filteredMessages.Add(detectedMessages[i]);
            }

            return filteredMessages;
        }

        private bool IsEcamMessageAccepted(string message, List<EcamMessage> ecamMessageList)
        {
            EcamMessage ecamMessage = ecamMessageList.FirstOrDefault(listItem => (listItem.Name).ToLower().Equals(message.ToLower()));

            return (ecamMessage is not null && ecamMessage.IsAccepted);
        }

        private async Task<List<string>> ReadImageText(byte[] imageBytes)
        {
            StreamContent content = new(new MemoryStream(imageBytes));
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");

            HttpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", COMPUTERVISION_KEY);

            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["language"] = "en";
            queryString["overload"] = "stream";
            queryString["readingOrder"] = "natural";
            queryString["modelVersion"] = "latest";
            var response = await HttpClient.PostAsync(COMPUTERVISION_AZURE_ENDPOINT + "/read/analyze?" + queryString, content);
            response.EnsureSuccessStatusCode();

            string operationLocation = response.Headers.GetValues("operation-location").First();

            // Retrieve relevant ID from the URI
            const int numberOfCharsInOperationId = 36;
            string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

            Thread.Sleep(200);
            ReadOperationResult results;
            do
            {
                HttpResponseMessage readResult = await HttpClient.GetAsync(COMPUTERVISION_AZURE_ENDPOINT + "/read/analyzeResults/" + operationId);

                results = JsonConvert.DeserializeObject<ReadOperationResult>(
                    await readResult.Content.ReadAsStringAsync()
                );

                Thread.Sleep(200);
            }
            while (results.Status == OperationStatusCodes.Running || results.Status == OperationStatusCodes.NotStarted);

            IList<ReadResult> textReadResults = results.AnalyzeResult.ReadResults;
            List<string> words = new();

            foreach (ReadResult page in textReadResults)
            {
                foreach (Line line in page.Lines)
                {
                    words.Add(line.Text);
                }
            }

            return words;
        }

        private BoundingBox? FindBestEcamBoundingBox(PredictionModel? ewdPrediction, PredictionModel? ecamPrediction)
        {

            // Both ECAM and ECAM E/WD were detected, check if E/WD exists within or near ECAM's bounding box
            if (ecamPrediction is not null && ewdPrediction is not null)
            {
                return IsEwdWithinEcamBoundingBox(ewdPrediction.BoundingBox, ecamPrediction.BoundingBox)
                    ? ewdPrediction.BoundingBox
                    : ecamPrediction.BoundingBox;
            }

            return ewdPrediction is not null
                ? ewdPrediction.BoundingBox
                : ecamPrediction?.BoundingBox;
        }

        private bool IsEwdWithinEcamBoundingBox(BoundingBox ewd, BoundingBox ecam)
        {
            double maxAllowedDistancePercentage = 0.1;

            bool isEwdInEcam = (ewd.Left >= ecam.Left) && (ewd.Left + ewd.Width <= ecam.Left + ecam.Width);

            bool isEwdLeftNearEcamLeft = (ewd.Left <= ecam.Left + maxAllowedDistancePercentage)
                && (ewd.Left >= ecam.Left - maxAllowedDistancePercentage);
            bool isEwdRightNearEcamRight = (ewd.Left + ewd.Width) <= (ecam.Left + ecam.Width + maxAllowedDistancePercentage)
                && (ewd.Left + ewd.Width) >= (ecam.Left + ecam.Width - maxAllowedDistancePercentage);
            bool isEwdNearEcam = isEwdLeftNearEcamLeft && isEwdRightNearEcamRight;

            return isEwdInEcam || isEwdNearEcam;
        }

        private async Task<Image<Rgba32>> BlobToImage(BlobClient blob)
        {
            Stream stream = await blob.OpenReadAsync();
            return SixLabors.ImageSharp.Image.Load<Rgba32>(StreamToBytes(stream));
        }

        private static byte[] StreamToBytes(Stream input)
        {
            using MemoryStream ms = new();
            input.CopyTo(ms);
            input.Position = 0;
            return ms.ToArray();
        }

        private byte[] Crop(Image<Rgba32> image, BoundingBox boundingBox)
        {
            using MemoryStream outStream = new();

            int cropWidth = (int)(image.Width * boundingBox.Width);
            int cropHeight = (int)(image.Height * boundingBox.Height);
            int x = (int)(image.Width * boundingBox.Left);
            int y = (int)(image.Height * boundingBox.Top);

            SixLabors.ImageSharp.Image cropped = image.Clone(
                i => i.Resize(image.Width, image.Height)
                      .Crop(new Rectangle(x, y, cropWidth, cropHeight)));

            cropped.SaveAsPng(outStream);

            return outStream.ToArray();
        }

        private CustomVisionPredictionClient AuthenticatePrediction(string endpoint, string predictionKey)
        {
            return new(new AzureCVPrediction.ApiKeyServiceClientCredentials(predictionKey)) { Endpoint = endpoint };
        }

        private CustomVisionTrainingClient AuthenticateTraining(string endpoint, string trainingKey)
        {
            return new(new AzureCVTraining.ApiKeyServiceClientCredentials(trainingKey)) { Endpoint = endpoint };
        }

        private ComputerVisionClient AuthenticateComputerVision(string endpoint, string cvKey)
        {
            return new(new AzureCV.ApiKeyServiceClientCredentials(cvKey)) { Endpoint = endpoint };
        }

        private void CreateEventFromEventTypeName(string eventTypeName, int trainingId, TimeStamp timestamp)
        {
            EventType? eventType = EventTypeService.GetByName(eventTypeName);

            if (eventType is null)
                throw new Exception($"Event type \"{eventTypeName}\" could not be found.");

            EventService.CreateEvent(new()
            {
                Training = TrainingService.GetTrainingById(trainingId),
                EventType = eventType,
                OverwriteMessage = "",
                TimeStamp = timestamp
            });
        }

        public void SendAltitudePositionQueueMessage(BoundingBox position, int trainingId, TimeStamp timestamp, string snapshotBlobName)
        {
            SendQueueMessage(ALTITUDE_POSITION_QUEUE, new PositionMessage(position, trainingId, timestamp, snapshotBlobName));
        }

        public void SendEcamPositionQueueMessage(BoundingBox position, int trainingId, TimeStamp timestamp, string snapshotBlobName)
        {
            SendQueueMessage(ECAM_POSITION_QUEUE, new PositionMessage(position, trainingId, timestamp, snapshotBlobName));
        }

        private async void SendQueueMessage(string queueName, object message)
        {
            await new QueueClient(CONNECTION_STRING, queueName, new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 })
                .SendMessageAsync(JsonConvert.SerializeObject(message));
        }

        private async Task<string> StoreTrainingSnapshotBlob(int trainingId, TimeStamp timestamp, byte[] snapshot)
        {
            string blobName = $"{trainingId}_{timestamp.ToString('-')}_{Guid.NewGuid().ToString()}.png";

            BlobClient blob = new BlobContainerClient(CONNECTION_STRING, TRAINING_SNAPSHOT_BLOB_CONTAINER)
                .GetBlobClient(blobName);
            await blob.UploadAsync(new MemoryStream(snapshot));

            return blobName;
        }

        private async Task DeleteSnapshotBlob(string blobName)
        {
            BlobClient blob = new BlobContainerClient(CONNECTION_STRING, TRAINING_SNAPSHOT_BLOB_CONTAINER)
                .GetBlobClient(blobName);
            await blob.DeleteIfExistsAsync();
        }

        private Project GetProject(out Project project)
        {
            project = TrainingApi.GetProjects()
                .First(proj => proj.Name == PROJECT_NAME);

            return project;
        }
    }
}