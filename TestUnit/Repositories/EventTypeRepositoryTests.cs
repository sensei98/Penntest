using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Newtonsoft.Json;
using TestSupport.EfHelpers;
using VRefSolutions.DAL;
using VRefSolutions.Domain.Entities;
using VRefSolutions.Domain.Enums;
using VRefSolutions.Domain.Models;
using VRefSolutions.Repository;
using VRefSolutions.Repository.Interfaces;
using Xunit.Abstractions;

namespace TestUnit.Repositories;

public class EventTypeRepositoryTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private VRefSolutionsContext Context;
    private IEventTypeRepository EventTypeRepository;
    private List<User> _mockUsers;
    private List<Training> _mockTrainings;
    private List<EventType> _mockEventTypes;
    private List<Event> _mockEvents;

    public EventTypeRepositoryTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        Setup();
    }

    [Fact]
    public void GetAll_Should_Return_List_EventType()
    {
        List<EventType> eventTypes;
        bool didReturnListEventType;

        eventTypes = EventTypeRepository.GetAll().ToList();
        didReturnListEventType = eventTypes.Count > 1;

        Assert.True(didReturnListEventType);
    }

    [Fact]
    public void Add_Should_Add_EventType()
    {
        int eventTypesCount = EventTypeRepository.GetAll().ToList().Count;
        EventType eventType = new()
        {
            Id = 99,
            Name = "Name1",
            Message = "Message1",
            Symbol = "Symbol1"
        };
        bool didAddEventType;

        EventTypeRepository.Add(eventType);
        didAddEventType = Context.EventTypes.Count() > eventTypesCount;

        Assert.True(didAddEventType);
    }

    [Fact]
    public void Delete_Should_Delete_Event()
    {
        bool didDeleteEventType;
        List<EventType> eventTypes = EventTypeRepository.GetAll().ToList();
        EventType eventType = eventTypes[0];
        int eventTypesCount = eventTypes.Count;

        EventTypeRepository.Delete(eventType);
        didDeleteEventType = Context.EventTypes.Count() < eventTypesCount;

        Assert.True(didDeleteEventType);
    }

    [Fact]
    public void Update_Should_Update_Event()
    {
        bool didUpdateEventType;
        EventType eventType = EventTypeRepository.GetAll().ToList()[0];
        string originalMessage = eventType.Message;
        eventType.Message = "Test message";

        EventType updatedEventType = EventTypeRepository.Update(eventType);
        didUpdateEventType = !updatedEventType.Message.Equals(originalMessage);

        Assert.True(didUpdateEventType);
    }

    private void Setup()
    {
        var options = SqliteInMemory.CreateOptions<VRefSolutionsContext>();
        Context = new VRefSolutionsContext(options);
        Context.Database.EnsureCreated();

        EventTypeRepository = new EventTypeRepository(Context);

        _mockEventTypes = new();
        _mockEventTypes.AddRange(new List<EventType>(){
            new EventType
            {
                Id = 1,
                Name = "EventType1",
                Message = "Message1",
                Symbol = "img.svg"
            },
            new EventType
            {
                Id = 2,
                Name = "EventType2",
                Message = "Message2",
                Symbol = "img.svg"
            }
        });
        Context.EventTypes.AddRange(_mockEventTypes);

        Context.SaveChanges();
    }
}