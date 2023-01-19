using System.Security.Claims;
using VRefSolutions.Domain.Entities;
using VRefSolutions.Domain.Enums;

namespace VRefSolutions.Service.Interfaces
{
    public interface ITrainingService{
        Training CreateTraining(Training Training);
        Training GetTrainingById(int id);
        Training updateTraining (Training training);
        IEnumerable<Training> GetTrainingByStatus(Status processing);
        IEnumerable<string> GetVideoAccessURLs(Training training);
        int FetchOrganizationIdFromTraining(Training training);
        bool IsStudentPartOfTraining(Training targetTraining, int studentId);
        List<Training> GetTrainingsByUserId(int loggedInUserId);
        List<Training> GetTrainingsByUserOrganization(int loggedInUserId);
        List<Training> GetAll();
        void DeleteTraining(Training training);
        bool IsInstructorOfTraining(int loggedInUserId, Training training);
        User GetInstructorFromTraining(Training training);
        bool IsUserAuthorizedToViewTraining(Training training, User user);
    }
}