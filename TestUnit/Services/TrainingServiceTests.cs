using TestSupport.EfHelpers;
using VRefSolutions.DAL;
using VRefSolutions.Domain.Entities;
using VRefSolutions.Domain.Enums;
using VRefSolutions.Repository;
using VRefSolutions.Service;
using VRefSolutions.Service.Interfaces;
using Xunit.Extensions.AssertExtensions;

namespace TestUnit;

public class TrainingServiceTests
{
    private ITrainingService TrainingService;
    public TrainingServiceTests()
    {
        Setup(); // in memory sqlite relational database
    }
    [Fact]
    public void GetById_Should_Return_Correct_Training()
    {
        Training training = TrainingService.GetTrainingById(1);
        training.ShouldNotBeNull();
        training.Id.ShouldEqual(1);
    }
    [Fact]
    public void GetById_With_Wrong_Id_Should_Return_Null_Training()
    {
        Training training = TrainingService.GetTrainingById(111111111);
        training.ShouldBeNull();
    }

    [Fact]
    public void Training_Of_Student_Should_Be_Same_Training_From_GetTrainingsByUserId()
    {
        Training training = TrainingService.GetTrainingById(1);
        training.ShouldNotBeNull();
        training.Participants.ForEach(u =>
        {
            Training userTraining = TrainingService.GetTrainingsByUserId(u.Id).Where(t => t.Id == training.Id).First();
            userTraining.ShouldNotBeNull();
            training.ShouldBeSameAs(userTraining);
        });
    }


    [Fact]
    public void IsInstructorOfTraining_On_Wrong_Instructor_Id_Should_Return_False()
    {
        Training training = TrainingService.GetTrainingById(1);
        training.ShouldNotBeNull();
        TrainingService.IsInstructorOfTraining(4, training).ShouldBeFalse();
    }

    [Fact]
    public void IsInstructorOfTraining_On_Wrong_Instructor_Id_Should_Return_True()
    {
        Training training = TrainingService.GetTrainingById(1);
        int InstructorId = training.Participants.Where(u => u.UserType == Role.Instructor).First().Id;
        TrainingService.IsInstructorOfTraining(InstructorId, training).ShouldBeTrue();
    }

    [Fact]
    public void Training_Should_Have_Status_Created()
    {
        Training training = TrainingService.GetTrainingById(1);
        training.ShouldNotBeNull();
        training.Status.ShouldEqual(Status.Created);
    }
    [Fact]
    public void GetTrainingByStatus_Should_Return_Status_Created()
    {
        List<Training> trainings = TrainingService.GetTrainingByStatus(Status.Created).ToList();
        trainings.ForEach(t =>
        {
            t.ShouldNotBeNull();
            t.Status.ShouldEqual(Status.Created);
            t.Participants.ShouldNotBeNull();
            t.Participants.ForEach(u =>
            {
                u.Organization.ShouldNotBeNull();
                u.Organization.Id.ShouldNotBeNull();
                u.Organization.Name.ShouldNotBeNull();
            });
        });
    }

    [Fact]
    public void Update_Training_Should_Have_Status_Recording_Paused_Processing_Finished()
    {
        Training training = TrainingService.GetTrainingById(1);
        training.Status = Status.Recording;
        training = TrainingService.updateTraining(training);
        training.Status.ShouldEqual(Status.Recording);

        training.Status = Status.Paused;
        TrainingService.updateTraining(training);
        training = TrainingService.GetTrainingById(1);
        training.Status.ShouldEqual(Status.Paused);

        training.Status = Status.Processing;
        TrainingService.updateTraining(training);
        training = TrainingService.GetTrainingById(1);
        training.Status.ShouldEqual(Status.Processing);

        training.Status = Status.Finished;
        TrainingService.updateTraining(training);
        training = TrainingService.GetTrainingById(1);
        training.Status.ShouldEqual(Status.Finished);
    }

    private void Setup()
    {
        var options = SqliteInMemory.CreateOptions<VRefSolutionsContext>();
        VRefSolutionsContext Context = new VRefSolutionsContext(options);
        Context.Database.EnsureCreated();
        TrainingService = new TrainingService(new TrainingRepository(Context), new TrainingStateRepository(Context), null);
        List<User> users = new List<User>();
        var org = new Organization { Name = "Test Org 1", Users = new List<User>() };
        Context.Organizations.Add(org);
        users.AddRange(new List<User>(){
            new User
            {
                Email = "Test@whatever.com",
                FirstName = "Jim",
                LastName = "Testson",
                UserType = Role.Student,
                Organization = org
            },
            new User
            {
                Email = "Another@who.com",
                FirstName = "Jacob",
                LastName = "Mctest",
                UserType = Role.Student,
                Organization = org
            },
            new User
            {
                Email = "TheTeach@someorg.com",
                FirstName = "Jeremy",
                LastName = "Flightson",
                UserType = Role.Instructor,
                Organization = org
            }});
        Context.Users.AddRange(users);
        Context.Trainings.Add(new Training(users));
        Context.SaveChanges();
    }
}