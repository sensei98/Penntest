using VRefSolutions.DAL;
using VRefSolutions.Domain.Entities;
using Xunit.Abstractions;
using VRefSolutions.Service.Util;

namespace TestUnit.Util;

public class LevenshteinDistanceTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private List<EcamMessage> _ecamMessages;

    public LevenshteinDistanceTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        Setup();
    }

    [Fact]
    public void CorrectMessages_With_Input_EOAM9_Should_Return_ECAM9()
    {
        List<string> input = new() { "EOAM9" };
        _ecamMessages.Add(new EcamMessage()
        {
            Id = 9,
            Name = "ECAM9",
            IsAccepted = false
        });
        bool didCorrectInput;

        List<string> corrected = LevenshteinDistance.CorrectMessages(input, _ecamMessages);
        didCorrectInput = _ecamMessages.Any(o => o.Name.Equals(corrected[0]));

        Assert.True(didCorrectInput);
    }

    [Fact]
    public void CorrectMessages_With_3_Valid_Inputs_Should_Correct_3()
    {
        List<string> input = new() { "EOAM9", "AOAM9", "Tosa"};
        _ecamMessages.Add(new()
        {
            Id = 9,
            Name = "ECAM9",
            IsAccepted = false
        });
        _ecamMessages.Add(new()
        {
            Id = 10,
            Name = "Test",
            IsAccepted = false
        });
        bool didCorrect3;

        List<string> corrected = LevenshteinDistance.CorrectMessages(input, _ecamMessages);
        didCorrect3 = corrected.Count == 3;

        Assert.True(didCorrect3);
    }

    [Fact]
    public void CorrectMessages_With_Long_Unmapped_Input_Should_Return_Empty()
    {
        List<string> input = new() { "Long text that is not an ECAM message" };
        bool didNotCorrectInput;

        List<string> corrected = LevenshteinDistance.CorrectMessages(input, _ecamMessages);
        didNotCorrectInput = corrected.Count == 0;

        Assert.True(didNotCorrectInput);
    }

    [Fact]
    public void CorrectMessages_With_Empty_Input_Should_Return_Empty()
    {
        List<string> input = new() { };
        bool didNotCorrectInput;

        List<string> corrected = LevenshteinDistance.CorrectMessages(input, _ecamMessages);
        didNotCorrectInput = corrected.Count == 0;

        Assert.True(didNotCorrectInput);
    }

    [Fact]
    public void CorrectMessages_With_Empty_EcamMessages_Should_Return_Empty()
    {
        List<string> input = new() { "ACAM1" };
        bool didNotCorrectInput;

        List<string> corrected = LevenshteinDistance.CorrectMessages(input, new() { });
        didNotCorrectInput = corrected.Count == 0;

        Assert.True(didNotCorrectInput);
    }

    private void Setup()
    {
        _ecamMessages = new()
        {
            new EcamMessage()
            {
                Id = 1,
                Name = "ECAM1",
                IsAccepted = true,
            },
            new EcamMessage()
            {
                Id = 2,
                Name = "ECAM2",
                IsAccepted = false,
            },
            new EcamMessage()
            {
                Id = 3,
                Name = "ECAM3",
                IsAccepted = true,
            },
            new EcamMessage()
            {
                Id = 4,
                Name = "ECAM4",
                IsAccepted = false
            },
        };
    }
}