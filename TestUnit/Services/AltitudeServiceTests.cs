using TestSupport.EfHelpers;
using VRefSolutions.DAL;
using VRefSolutions.Domain.Entities;
using VRefSolutions.Domain.Enums;
using VRefSolutions.Domain.Models;
using VRefSolutions.Repository;
using VRefSolutions.Service;
using VRefSolutions.Service.Interfaces;

namespace TestUnit.Services;

public class AltitudeServiceTests
{
    private VRefSolutionsContext Context;
    private IAltitudeService AltitudeService;
    private List<Training> _mockTrainings;
    private List<Altitude> _mockAltitudes;

    public AltitudeServiceTests()
    {
        Setup();
    }

    [Fact]
    public void CreateAltitude_Should_Increase_Altitude_Count()
    {
        bool didIncreaseAltitudeCount;
        int initialCount = Context.Altitudes.Count();
        Altitude altitude = new()
        {
            Id = 99,
            Training = _mockTrainings[0],
            Amsl = 99,
            TimeStamp = new TimeStamp(0, 0, 30, 0)
        };

        AltitudeService.CreateAltitude(altitude);
        didIncreaseAltitudeCount = Context.Altitudes.Count() > initialCount;

        Assert.True(didIncreaseAltitudeCount);
    }

    [Fact]
    public void GetAltitudeByTrainingId_With_TrainingId_2_Should_Return_2_Altitudes_With_TrainingId()
    {
        bool didReturn2Altitudes;
        int trainingId = 2;
        int expectedCount = 2;

        List<Altitude> altitudes = AltitudeService.GetAltitudesByTrainingId(trainingId);

        didReturn2Altitudes = altitudes.Count == expectedCount;

        Assert.True(didReturn2Altitudes);
    }

    [Fact]
    public void GetAltitudesInRangeByTrainingId_Should_Return_Less_Altitudes()
    {
        bool didReturnLessAltitudes;
        int altitudesCount = Context.Altitudes.Count();
        TimeStamp time = new(0, 0, 30, 0);
        int range = 10;

        List<Altitude> filteredAltitudes = AltitudeService.GetAltitudesInRangeByTrainingId(_mockTrainings[0].Id, time, range);
        didReturnLessAltitudes = filteredAltitudes.Count() < altitudesCount;

        Assert.True(didReturnLessAltitudes);
    }

    [Fact]
    public void GetAltitudesInRangeByTrainingId_With_Time_30s_And_Range_10s_Should_Return_3_Altitudes_Within_Range()
    {
        bool didReturn3Altitudes;
        TimeStamp time = new(0, 0, 30, 0);
        int range = 10;

        List<Altitude> filteredAltitudes = AltitudeService.GetAltitudesInRangeByTrainingId(_mockTrainings[0].Id, time, range);
        didReturn3Altitudes = filteredAltitudes.Count() == 3;

        Assert.True(didReturn3Altitudes);
    }

    [Fact]
    public void GetAltitudesInRangeByTrainingId_With_Time_30s_And_Range_20s_Should_Return_4_Altitudes_Within_Range()
    {
        bool didReturn4Altitudes;
        TimeStamp time = new(0, 0, 30, 0);
        int range = 20;

        List<Altitude> filteredAltitudes = AltitudeService.GetAltitudesInRangeByTrainingId(_mockTrainings[0].Id, time, range);
        didReturn4Altitudes = filteredAltitudes.Count() == 4;

        Assert.True(didReturn4Altitudes);
    }

    private void Setup()
    {
        var options = SqliteInMemory.CreateOptions<VRefSolutionsContext>();
        Context = new VRefSolutionsContext(options);
        Context.Database.EnsureCreated();

        AltitudeService = new AltitudeService(new AltitudeRepository(Context));

        _mockTrainings = new List<Training>();
        _mockTrainings.AddRange(
            new List<Training>()
            {
                new Training(null),
                new Training(null),
                new Training(null)
            });
        Context.Trainings.AddRange(_mockTrainings);

        _mockAltitudes = new List<Altitude>();
        _mockAltitudes.AddRange(new List<Altitude>(){
            new Altitude
            {
                Id = 1,
                Training = _mockTrainings[0],
                Amsl = 20,
                TimeStamp = new TimeStamp(0, 0, 25, 0)
            },
            new Altitude
            {
                Id = 2,
                Training = _mockTrainings[0],
                Amsl = 30,
                TimeStamp = new TimeStamp(0, 0, 30, 0)
            },
            new Altitude
            {
                Id = 3,
                Training = _mockTrainings[0],
                Amsl = 50,
                TimeStamp = new TimeStamp(0, 0, 39, 0)
            },
            new Altitude
            {
                Id = 4,
                Training = _mockTrainings[0],
                Amsl = 80,
                TimeStamp = new TimeStamp(0, 0, 45, 0)
            },
            new Altitude
            {
                Id = 5,
                Training = _mockTrainings[0],
                Amsl = 400,
                TimeStamp = new TimeStamp(0, 1, 20, 0)
            },
            new Altitude
            {
                Id = 6,
                Training = _mockTrainings[1],
                Amsl = 20,
                TimeStamp = new TimeStamp(0, 0, 29, 0)
            },
            new Altitude
            {
                Id = 7,
                Training = _mockTrainings[1],
                Amsl = 500,
                TimeStamp = new TimeStamp(1, 2, 42, 0)
            },
        });
        Context.Altitudes.AddRange(_mockAltitudes);

        Context.SaveChanges();
    }
}