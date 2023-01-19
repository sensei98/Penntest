using TestSupport.EfHelpers;
using VRefSolutions.DAL;
using VRefSolutions.Domain.Entities;
using VRefSolutions.Domain.Enums;
using VRefSolutions.Repository;
using VRefSolutions.Repository.Interfaces;
using Xunit.Extensions.AssertExtensions;

namespace TestUnit;

public class TrainingRepositoryTests
{
    private VRefSolutionsContext Context;
    private ITrainingRepository TrainingRepository;

    public TrainingRepositoryTests()
    {
        Setup();
    }

    [Fact]
    public void testCRUD(){
        var trainings = GetAll();
        trainings.Count.ShouldEqual(3);

        var training = Create();
        training.ShouldNotBeNull();
        training.Events.ShouldBeEmpty();
        training.Altitudes.ShouldBeEmpty();
        training.Participants.ShouldNotBeEmpty();
        training.Id.ShouldEqual(4);

        var sameTraining = GetByID(4);
        sameTraining.ShouldNotBeNull();
        sameTraining.ShouldBeSameAs(training);


        trainings = GetAll();
        trainings.Count.ShouldEqual(4);
        training.Status = Status.Finished;
        var trainingUpdated = Update(training);
        trainingUpdated.Status.ShouldEqual(Status.Finished);
        trainingUpdated.ShouldBeSameAs(training);
        Delete(trainingUpdated);

        var deletedTraining = GetByID(4);
        deletedTraining.ShouldBeNull();

        trainings = GetAll();
        trainings.Count.ShouldEqual(3);
        
      
    }
    private Training Create(){
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
        Training training = new Training(users);
        TrainingRepository.Add(training);
        training.Id.ShouldEqual(4);
        return training;
    }

    private Training GetByID(int id){
        Training training = TrainingRepository.GetSingle(id);
        return training;
    }
    private List<Training> GetAll(){
        return TrainingRepository.GetAll().ToList();
    }
    private Training Update(Training training){
        return TrainingRepository.Update(training);
    }
    private void Delete(Training training){
        TrainingRepository.Delete(training);
    }
    public void Setup()
    {

        var options = SqliteInMemory.CreateOptions<VRefSolutionsContext>();
        Context = new VRefSolutionsContext(options);
        Context.Database.EnsureCreated();
        TrainingRepository = new TrainingRepository(Context);
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
        Context.Trainings.Add(new Training(users));
        Context.Trainings.Add(new Training(users));
        Context.SaveChanges();
    }
}