using System;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VRefSolutions.Domain.Entities;
using VRefSolutions.Domain.Enums;
using VRefSolutions.Service.Interfaces;

namespace VRefSoltutions
{
    public class TrainingTimerTrigger
    {
        private readonly ILogger _logger;
        private ITrainingService TrainingService;
        private IEmailService EmailService;

        public TrainingTimerTrigger(ILoggerFactory loggerFactory, ITrainingService trainingService, IEmailService emailService)
        {
            _logger = loggerFactory.CreateLogger<TrainingTimerTrigger>();
            TrainingService = trainingService;
            EmailService =emailService;
        }

        [Function("TrainingTimerTrigger")]
        public void Run([TimerTrigger("0 */1 * * * *")] MyInfo myTimer)
        {
            // Todo: implement this Timed Status Updater method

            // Steps:
            var blobContainerClient = new BlobContainerClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), Environment.GetEnvironmentVariable("StorageContainer"));

            // Get all Trainings that are in the `Processing` Status.
            var processingTrainings = TrainingService.GetTrainingByStatus(Status.Processing).ToList();

            // For each training: check if the videos are available in blob storage. 
            foreach (Training training in processingTrainings)
            {
                Organization organization = training.Participants.Where(u => u.UserType == Role.Instructor).First().Organization;
                bool VideosUploaded = true;
                // check if their videos are uploaded to blob storage. 
                // Blob storage folder paths are: /organizationId/TrainingId/
                training.Videos.ForEach(s =>_logger.LogInformation("Video: "+ s));
                foreach (string video in training.Videos)
                {
                    string path = $"/{organization.Id}/training-{training.Id}/{video}.mp4";
                    _logger.LogInformation($"Checking video on path: {path}");
                    var blobclient = blobContainerClient.GetBlockBlobClient(path);
                    if (!blobclient.Exists())
                    {
                        VideosUploaded = false;
                        break;
                    }
                }
                if (!VideosUploaded)
                    continue;
                _logger.LogInformation("Videos Exists! Updating Training Status...");
                training.Status = Status.Finished;
                List<User> students = training.Participants.Where(u => u.UserType == Role.Student).ToList();
                foreach (User student in students)
                {
                    EmailService.sendEmail(student.Email,
                    "Your Training Recording is Ready!",
                    $"Hi {student.FirstName}, A training you participated has finished processing and is viewable on the VRefSolutions IPad App.");
                    // Todo: Mail students that their recording is ready to be viewed from the app. 
                }
                TrainingService.updateTraining(training);

            }
        }
    }

    public class MyInfo
    {
        public MyScheduleStatus ScheduleStatus { get; set; }

        public bool IsPastDue { get; set; }
    }

    public class MyScheduleStatus
    {
        public DateTime Last { get; set; }

        public DateTime Next { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
