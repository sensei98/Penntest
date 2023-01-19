using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using VRefSolutions.Domain.Entities;
using VRefSolutions.Domain.Enums;
using VRefSolutions.Repository.Interfaces;
using VRefSolutions.Service.Interfaces;

namespace VRefSolutions.Service
{
    public class TrainingService : ITrainingService
    {
        private ITrainingRepository TrainingRepository;
        private ITrainingStateRepository TrainingStateRepository;
        private IUserRepository UserRepository;

        public TrainingService(ITrainingRepository trainingRepository, ITrainingStateRepository trainingStateRepository, IUserRepository userRepository)
        {
            TrainingRepository = trainingRepository;
            TrainingStateRepository = trainingStateRepository;
            UserRepository = userRepository;
        }
        public Training CreateTraining(Training Training)
        {
            Training training = TrainingRepository.Add(Training);
            TrainingStateRepository.Add(new()
            {
                Training = Training,
                EcamMessages = Array.Empty<string>(),
                Altitude = 0
            });

            return training;
        }

        public Training GetTrainingById(int id)
        {
            return TrainingRepository.GetSingle(id);
        }

        public IEnumerable<Training> GetTrainingByStatus(Status status)
        {
            return TrainingRepository.FindBy(t => t.Status == status);
        }

        public IEnumerable<string> GetVideoAccessURLs(Training training)
        {
            // given the videos and organization of Instructor, find the videos in blob storage and create SAS tokens
            List<string> videoAccessURLs = new List<string>();
            int organizationId = FetchOrganizationIdFromTraining(training);
            BlobSasBuilder builder = new BlobSasBuilder
            {
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(24)
            };
            builder.SetPermissions(BlobSasPermissions.Read);
            BlobContainerClient blobContainerClient;
            try
            {   // In case Blob Storage is not available.
                blobContainerClient = new BlobContainerClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), Environment.GetEnvironmentVariable("StorageContainer"));
            }
            catch
            {
                return videoAccessURLs;
            }
            foreach (string video in training.Videos)
            {
                var blobBlockClient = blobContainerClient.GetBlockBlobClient($"/{organizationId}/training-{training.Id}/{video}.mp4");
                if (blobBlockClient.Exists())
                {
                    // builder.BlobName = $"/{organizationId}/training-{training.Id}/{video}.mp4";
                    videoAccessURLs.Add(blobBlockClient.GenerateSasUri(builder).AbsoluteUri);
                }

            }
            return videoAccessURLs;
        }

        public Training updateTraining(Training training)
        {
            return TrainingRepository.Update(training);
        }
        public int FetchOrganizationIdFromTraining(Training training)
        {
            return training.Participants.Where(u => u.UserType == Role.Instructor).First().Organization.Id;
        }

        public bool IsStudentPartOfTraining(Training targetTraining, int studentId)
        {
            return targetTraining.Participants.Where(u => u.Id == studentId && u.UserType == Role.Student).Count() > 0;
        }
        public bool IsUserAuthorizedToViewTraining(Training training, User user){
            switch(user.UserType){
                case Role.SuperAdmin:
                    return true;
                case Role.Admin:
                    return GetInstructorFromTraining(training).Organization.Id == user.Organization.Id;
                case Role.Instructor:
                    return GetInstructorFromTraining(training).Id == user.Id;
                case Role.Student:
                    return IsStudentPartOfTraining(training, user.Id);
                default:
                    return false;
            }
        }

        public List<Training> GetTrainingsByUserId(int loggedInUserId)
        {
            // return TrainingRepository.GetAll().Where(t => t.Id ==1 ).ToList();
            return TrainingRepository.GetByUserId(loggedInUserId).ToList();
        }

        public List<Training> GetTrainingsByUserOrganization(int loggedInUserId)
        {
            int organizationId = UserRepository.GetSingle(loggedInUserId).Organization.Id;

            return TrainingRepository.GetByOrganizationId(organizationId).ToList();
        }

        public List<Training> GetAll()
        {
            return TrainingRepository.GetAll().ToList();
        }

        public void DeleteTraining(Training training)
        {
            try
            {
                var blobContainerClient = new BlobContainerClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), Environment.GetEnvironmentVariable("StorageContainer"));
                int organizationId = FetchOrganizationIdFromTraining(training);
                foreach (string video in training.Videos)
                {
                    var blobBlockClient = blobContainerClient.GetBlockBlobClient($"/{organizationId}/training-{training.Id}/{video}.mp4");
                    blobBlockClient.DeleteIfExistsAsync();
                }
            }
            catch{}
            finally
            {
                TrainingRepository.Delete(training);
            }
        }

        public bool IsInstructorOfTraining(int loggedInUserId, Training training)
        {
            return training.Participants.Where(u => u.Id == loggedInUserId && u.UserType == Role.Instructor).Count() > 0;
        }
        public User GetInstructorFromTraining(Training training){
            return training.Participants.Where(u => u.UserType == Role.Instructor).First();
        }
    }

}