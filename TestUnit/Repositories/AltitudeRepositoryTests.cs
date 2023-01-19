using TestSupport.EfHelpers;
using VRefSolutions.DAL;
using VRefSolutions.Domain.Entities;
using VRefSolutions.Domain.Enums;
using VRefSolutions.Domain.Models;
using VRefSolutions.Repository;
using VRefSolutions.Repository.Interfaces;
using Xunit.Abstractions;

namespace TestUnit.Repositories;

public class AltitudeRepositoryTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private VRefSolutionsContext Context;
    private IAltitudeRepository AltitudeRepository;
    private List<Training> _mockTrainings;
    private List<Altitude> _mockAltitudes;

    public AltitudeRepositoryTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        Setup();
    }

    [Fact]
    public void GetAll_Should_Return_List_Altitude()
    {
        List<Altitude> altitudes;
        bool didReturnListAltitude;

        altitudes = AltitudeRepository.GetAll().ToList();
        didReturnListAltitude = altitudes.Count > 1;

        Assert.True(didReturnListAltitude);
    }

    [Fact]
    public void Add_Should_Add_Altitude()
    {
        int altitudesCount = AltitudeRepository.GetAll().ToList().Count;
        Altitude altitude = new Altitude()
        {
            Id = 99,
            Training = _mockTrainings[1],
            Amsl = 99,
            TimeStamp = new TimeStamp(0, 0, 30, 0)
        };
        bool didAddAltitude;

        AltitudeRepository.Add(altitude);
        didAddAltitude = AltitudeRepository.GetAll().ToList().Count > altitudesCount;

        Assert.True(didAddAltitude);
    }

    [Fact]
    public void GetAltitudesByTrainingId_Should_Return_2_Altitudes_From_Training_With_2_Altitudes()
    {
        List<Altitude> altitudes;
        bool didReturn2Altitudes;

        altitudes = AltitudeRepository.GetAltitudesByTrainingId(_mockAltitudes[0].Training.Id);
        didReturn2Altitudes = altitudes.Count == 2;

        Assert.True(didReturn2Altitudes);
    }

    [Fact]
    public void Delete_Should_Delete_Altitude()
    {
        bool didDeleteAltitude;
        List<Altitude> altitudes = AltitudeRepository.GetAll().ToList();
        Altitude altitude = altitudes[0];
        int altitudesCount = altitudes.Count;

        AltitudeRepository.Delete(altitude);
        didDeleteAltitude = Context.Altitudes.Count() < altitudesCount;

        Assert.True(didDeleteAltitude);
    }

    [Fact]
    public void Update_Should_Update_Altitude()
    {
        bool didUpdateAltitude;
        Altitude altitude = AltitudeRepository.GetAll().ToList()[0];
        double originalAmsl = altitude.Amsl;
        altitude.Amsl = 500;

        Altitude updatedAltitude = AltitudeRepository.Update(altitude);
        didUpdateAltitude = updatedAltitude.Amsl != originalAmsl;

        Assert.True(didUpdateAltitude);
    }

    private void Setup()
    {
        var options = SqliteInMemory.CreateOptions<VRefSolutionsContext>();
        Context = new VRefSolutionsContext(options);
        Context.Database.EnsureCreated();

        AltitudeRepository = new AltitudeRepository(Context);

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
                Amsl = 30,
                TimeStamp = new TimeStamp(0, 0, 30, 0)
            },
            new Altitude
            {
                Id = 2,
                Training = _mockTrainings[0],
                Amsl = 50,
                TimeStamp = new TimeStamp(0, 0, 39, 0)
            },
            new Altitude
            {
                Id = 3,
                Training = _mockTrainings[1],
                Amsl = 20,
                TimeStamp = new TimeStamp(0, 0, 29, 0)
            },
        });
        Context.Altitudes.AddRange(_mockAltitudes);

        Context.SaveChanges();
    }
}