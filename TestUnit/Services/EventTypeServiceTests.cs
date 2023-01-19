using TestSupport.EfHelpers;
using VRefSolutions.DAL;
using VRefSolutions.Domain.Entities;
using VRefSolutions.Domain.Enums;
using VRefSolutions.Domain.Models;
using VRefSolutions.Repository;
using VRefSolutions.Service;
using VRefSolutions.Service.Interfaces;

namespace TestUnit.Services;

public class EventTypeServiceTests
{
    private VRefSolutionsContext Context;
    private IEventTypeService EventTypeService;
    private List<Training> _mockTrainings;
    private List<EventType> _mockEventTypes;

    public EventTypeServiceTests()
    {
        Setup();
    }

    [Fact]
    public void GetEventTypeById_Should_Return_EventType_With_Id()
    {
        bool didFindCorrectEventType;
        int id = 1;

        EventType eventType = EventTypeService.GetEventTypeById(id);
        didFindCorrectEventType = eventType.Id == id;

        Assert.True(didFindCorrectEventType);
    }

    [Fact]
    public void CreateEventType_Should_Increase_EventType_Count()
    {
        bool didIncreaseEventTypeCount;
        int initialCount = Context.EventTypes.Count();
        EventType eventType = new()
        {
            Id = 99,
            Name = "Event type",
            Message = "Message",
            Symbol = "symbol.svg"
        };

        EventTypeService.CreateEventType(eventType);
        didIncreaseEventTypeCount = Context.EventTypes.Count() > initialCount;

        Assert.True(didIncreaseEventTypeCount);
    }

    [Fact]
    public void CreateEventType_Should_Add_Input_EventType()
    {
        bool didAddInputEventType;
        EventType eventType = new()
        {
            Id = 99,
            Name = "Event type",
            Message = "Message",
            Symbol = "symbol.svg"
        };

        EventType createdEventType = EventTypeService.CreateEventType(eventType);
        didAddInputEventType = createdEventType.Name.Equals(eventType.Name);

        Assert.True(didAddInputEventType);
    }

    [Fact]
    public void GetByName_Should_Find_EventType()
    {
        string name = "EventType2";
        bool didFindEventType;

        EventType foundEventType = EventTypeService.GetByName("EventType2");
        didFindEventType = foundEventType.Name.Equals(name);

        Assert.True(didFindEventType);
    }

    private void Setup()
    {
        var options = SqliteInMemory.CreateOptions<VRefSolutionsContext>();
        Context = new VRefSolutionsContext(options);
        Context.Database.EnsureCreated();

        EventTypeService = new EventTypeService(new EventTypeRepository(Context));

        _mockTrainings = new();
        _mockTrainings.AddRange(
            new List<Training>()
            {
                new Training(null),
                new Training(null),
                new Training(null)
            });
        Context.Trainings.AddRange(_mockTrainings);

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