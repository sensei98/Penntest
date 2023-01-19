using TestSupport.EfHelpers;
using VRefSolutions.DAL;
using VRefSolutions.Domain.Entities;
using VRefSolutions.Domain.Enums;
using VRefSolutions.Repository;
using VRefSolutions.Repository.Interfaces;
using Xunit.Abstractions;

namespace TestUnit.Repositories;

public class TrainingStateRepositoryTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private VRefSolutionsContext Context;
    private ITrainingStateRepository TrainingStateRepository;
    private List<TrainingState> _mockTrainingStates;
    private List<Training> _mockTrainings;

    public TrainingStateRepositoryTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        Setup();
    }

    [Fact]
    public void GetAll_Should_Return_List_TrainingState()
    {
        List<TrainingState> trainingStates;
        bool didReturnListTrainingState;

        trainingStates = TrainingStateRepository.GetAll().ToList();
        didReturnListTrainingState = trainingStates.Count > 1;

        Assert.True(didReturnListTrainingState);
    }

    [Fact]
    public void Add_Should_Add_TrainingState()
    {
        int trainingStatesCount = TrainingStateRepository.GetAll().ToList().Count;
        TrainingState trainingState = new()
        {
            Id = 99,
            Training = _mockTrainings[2],
            Altitude = 0,
            EcamMessages = new string[] { "TEST" }
        };
        bool didAddTrainingState;

        TrainingStateRepository.Add(trainingState);
        didAddTrainingState = TrainingStateRepository.GetAll().ToList().Count > trainingStatesCount;

        Assert.True(didAddTrainingState);
    }

    [Fact]
    public void Delete_Should_Delete_TrainingState()
    {
        bool didDeleteTrainingState;
        List<TrainingState> trainingStates = TrainingStateRepository.GetAll().ToList();
        TrainingState trainingState = trainingStates[0];
        int trainingStatesCount = trainingStates.Count;

        TrainingStateRepository.Delete(trainingState);
        didDeleteTrainingState = Context.TrainingStates.Count() < trainingStatesCount;

        Assert.True(didDeleteTrainingState);
    }

    [Fact]
    public void Update_Should_Update_TrainingState()
    {
        bool didUpdateTrainingState;
        TrainingState trainingState = TrainingStateRepository.GetAll().ToList()[0];
        int originalAltitude = trainingState.Altitude;
        trainingState.Altitude = 10;

        TrainingState updatedTrainingState = TrainingStateRepository.Update(trainingState);
        didUpdateTrainingState = updatedTrainingState.Altitude != originalAltitude;

        Assert.True(didUpdateTrainingState);
    }

    private void Setup()
    {
        var options = SqliteInMemory.CreateOptions<VRefSolutionsContext>();
        Context = new VRefSolutionsContext(options);
        Context.Database.EnsureCreated();

        TrainingStateRepository = new TrainingStateRepository(Context);

        _mockTrainings = new();
        _mockTrainings.AddRange(
            new List<Training>()
            {
                new Training(null),
                new Training(null),
                new Training(null)
            });
        Context.Trainings.AddRange(_mockTrainings);

        _mockTrainingStates = new List<TrainingState>();
        _mockTrainingStates.AddRange(new List<TrainingState>(){
            new TrainingState
            {
                Id = 1,
                Training = _mockTrainings[0],
                Altitude = 0,
                EcamMessages = new string[] {"HYD", "LAND ASAP", "APU GEN"}
            },
            new TrainingState
            {
                Id = 2,
                Training = _mockTrainings[1],
                Altitude = 0,
                EcamMessages = new string[] {"HYD", "LAND ASAP"}
            },
        });
        Context.TrainingStates.AddRange(_mockTrainingStates);

        Context.SaveChanges();
    }
}