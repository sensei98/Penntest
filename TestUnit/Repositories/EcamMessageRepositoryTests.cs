using TestSupport.EfHelpers;
using VRefSolutions.DAL;
using VRefSolutions.Domain.Entities;
using VRefSolutions.Domain.Enums;
using VRefSolutions.Domain.Models;
using VRefSolutions.Repository;
using VRefSolutions.Repository.Interfaces;
using Xunit.Abstractions;

namespace TestUnit.Repositories;

public class EcamMessageRepositoryTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private VRefSolutionsContext Context;
    private IEcamMessageRepository EcamMessageRepository;
    private List<EcamMessage> _mockEcamMessages;

    public EcamMessageRepositoryTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        Setup();
    }

    [Fact]
    public void GetAll_Should_Return_List_EcamMessage()
    {
        List<EcamMessage> ecamMessages;
        bool didReturnListEcamMessage;

        ecamMessages = EcamMessageRepository.GetAll().ToList();
        didReturnListEcamMessage = ecamMessages.Count > 1;

        Assert.True(didReturnListEcamMessage);
    }

    [Fact]
    public void Add_Should_Add_EcamMessage()
    {
        int ecamMessagesCount = EcamMessageRepository.GetAll().ToList().Count;
        EcamMessage ecamMessage = new EcamMessage()
        {
            Id = 99,
            Name = "FIRE",
            IsAccepted = true
        };
        bool didAddEcamMessage;

        EcamMessageRepository.Add(ecamMessage);
        didAddEcamMessage = EcamMessageRepository.GetAll().ToList().Count > ecamMessagesCount;

        Assert.True(didAddEcamMessage);
    }

    [Fact]
    public void Delete_Should_Delete_EcamMessage()
    {
        bool didDeleteEcamMessage;
        List<EcamMessage> ecamMessages = EcamMessageRepository.GetAll().ToList();
        EcamMessage ecamMessage = ecamMessages[0];
        int ecamMessagesCount = ecamMessages.Count;

        EcamMessageRepository.Delete(ecamMessage);
        didDeleteEcamMessage = Context.EcamMessages.Count() < ecamMessagesCount;

        Assert.True(didDeleteEcamMessage);
    }

    [Fact]
    public void Update_Should_Update_EcamMessage()
    {
        bool didUpdateEcamMessage;
        EcamMessage ecamMessage = EcamMessageRepository.GetAll().ToList()[0];
        string originalMessage = ecamMessage.Name;
        ecamMessage.Name = "TEST";

        EcamMessage updatedEcamMessage = EcamMessageRepository.Update(ecamMessage);
        didUpdateEcamMessage = !updatedEcamMessage.Name.Equals(originalMessage);

        Assert.True(didUpdateEcamMessage);
    }

    private void Setup()
    {
        var options = SqliteInMemory.CreateOptions<VRefSolutionsContext>();
        Context = new VRefSolutionsContext(options);
        Context.Database.EnsureCreated();

        EcamMessageRepository = new EcamMessageRepository(Context);

        _mockEcamMessages = new List<EcamMessage>();
        _mockEcamMessages.AddRange(new List<EcamMessage>(){
            new EcamMessage
            {
                Id = 1,
                Name = "HYD",
                IsAccepted = true
            },
            new EcamMessage
            {
                Id = 2,
                Name = "LAND ASAP",
                IsAccepted = true
            },
            new EcamMessage
            {
                Id = 3,
                Name = "GND SPLRS ARMED",
                IsAccepted = false
            },
            new EcamMessage
            {
                Id = 4,
                Name = "ENG 1 RELIGHT",
                IsAccepted = false
            },
        });
        Context.EcamMessages.AddRange(_mockEcamMessages);

        Context.SaveChanges();
    }
}