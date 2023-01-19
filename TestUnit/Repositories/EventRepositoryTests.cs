using TestSupport.EfHelpers;
using VRefSolutions.DAL;
using VRefSolutions.Domain.Entities;
using VRefSolutions.Domain.Enums;
using VRefSolutions.Domain.Models;
using VRefSolutions.Repository;
using VRefSolutions.Repository.Interfaces;
using Xunit.Abstractions;

namespace TestUnit.Repositories;

public class EventRepositoryTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private VRefSolutionsContext Context;
    private IEventRepository EventRepository;
    private List<User> _mockUsers;
    private List<Training> _mockTrainings;
    private List<EventType> _mockEventTypes;
    private List<Event> _mockEvents;

    public EventRepositoryTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        Setup();
    }

    [Fact]
    public void GetAll_Should_Return_List_Event()
    {
        List<Event> events;
        bool didReturnListEvent;

        events = EventRepository.GetAll().ToList();
        didReturnListEvent = events.Count > 1;

        Assert.True(didReturnListEvent);
    }

    [Fact]
    public void Add_Should_Add_Event()
    {
        int eventsCount = EventRepository.GetAll().ToList().Count;
        Event eventObj = new Event()
        {
            Id = 99,
            Training = _mockTrainings[1],
            EventType = _mockEventTypes[1],
            OverwriteMessage = "",
            OverwriteName = "",
            OverwriteSymbol = "",
            TimeStamp = new TimeStamp(0, 0, 30, 0)
        };
        bool didAddEvent;

        EventRepository.Add(eventObj);
        didAddEvent = EventRepository.GetAll().ToList().Count > eventsCount;

        Assert.True(didAddEvent);
    }

    [Fact]
    public void GetEventsByTrainingId_Should_Return_2_Events_From_Training_With_2_Events()
    {
        List<Event> events;
        bool didReturn2Events;

        events = EventRepository.GetEventsByTrainingId(_mockEvents[0].Training.Id);
        didReturn2Events = events.Count == 2;

        Assert.True(didReturn2Events);
    }

    [Fact]
    public void Delete_Should_Delete_Event()
    {
        bool didDeleteEvent;
        List<Event> events = EventRepository.GetAll().ToList();
        Event eventObj = events[0];
        int eventsCount = events.Count;

        EventRepository.Delete(eventObj);
        didDeleteEvent = Context.Events.Count() < eventsCount;

        Assert.True(didDeleteEvent);
    }

    [Fact]
    public void Update_Should_Update_Event()
    {
        bool didUpdateEvent;
        Event eventObj = EventRepository.GetAll().ToList()[0];
        string originalMessage = eventObj.OverwriteMessage;
        eventObj.OverwriteMessage = "Test message";

        Event updatedEvent = EventRepository.Update(eventObj);
        didUpdateEvent = !updatedEvent.OverwriteMessage.Equals(originalMessage);

        Assert.True(didUpdateEvent);
    }

    private void Setup()
    {
        var options = SqliteInMemory.CreateOptions<VRefSolutionsContext>();
        Context = new VRefSolutionsContext(options);
        Context.Database.EnsureCreated();

        EventRepository = new EventRepository(Context);

        _mockUsers = new();
        _mockTrainings = new();
        var org = new Organization { Name = "Test Org 1", Users = new List<User>() };
        Context.Organizations.Add(org);
        _mockUsers.AddRange(new List<User>(){
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
        _mockTrainings.AddRange(
            new List<Training>()
            {
                new Training(_mockUsers),
                new Training(_mockUsers),
                new Training(_mockUsers)
            });
        Context.Users.AddRange(_mockUsers);
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

        _mockEvents = new List<Event>();
        _mockEvents.AddRange(new List<Event>(){
            new Event
            {
                Id = 1,
                Training = _mockTrainings[0],
                EventType = _mockEventTypes[0],
                OverwriteMessage = "",
                OverwriteName = "",
                OverwriteSymbol = "",
                TimeStamp = new TimeStamp(0, 0, 30, 0)
            },
            new Event
            {
                Id = 2,
                Training = _mockTrainings[0],
                EventType = _mockEventTypes[1],
                OverwriteMessage = "Custom message",
                OverwriteName = "",
                OverwriteSymbol = "",
                TimeStamp = new TimeStamp(0, 0, 39, 0)
            },
                        new Event
            {
                Id = 3,
                Training = _mockTrainings[1],
                EventType = _mockEventTypes[0],
                OverwriteMessage = "",
                OverwriteName = "",
                OverwriteSymbol = "",
                TimeStamp = new TimeStamp(0, 0, 29, 0)
            },
        });
        Context.Events.AddRange(_mockEvents);

        Context.SaveChanges();
    }
}