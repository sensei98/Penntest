using TestSupport.EfHelpers;
using VRefSolutions.DAL;
using VRefSolutions.Domain.Entities;
using VRefSolutions.Domain.Enums;
using VRefSolutions.Domain.Models;
using VRefSolutions.Repository;
using VRefSolutions.Service;
using VRefSolutions.Service.Interfaces;

namespace TestUnit.Services;

public class EventServiceTests
{
    private VRefSolutionsContext Context;
    private IEventService EventService;
    private List<User> _mockUsers;
    private List<Training> _mockTrainings;
    private List<EventType> _mockEventTypes;
    private List<Event> _mockEvents;

    public EventServiceTests()
    {
        Setup();
    }

    [Fact]
    public void CreateEvent_Should_Increase_Event_Count()
    {
        bool didIncreaseEventCount;
        int initialCount = Context.Events.Count();
        Event eventObj = new()
        {
            Id = 99,
            Training = _mockTrainings[0],
            EventType = _mockEventTypes[0],
            OverwriteMessage = "",
            OverwriteSymbol = "",
            OverwriteName = "",
            TimeStamp = new TimeStamp(0, 0, 30, 0)
        };

        EventService.CreateEvent(eventObj);
        didIncreaseEventCount = Context.Events.Count() > initialCount;

        Assert.True(didIncreaseEventCount);
    }

    [Fact]
    public void GetEventById_Should_Return_Event_With_Id()
    {
        bool didFindCorrectEvent;
        int id = 1;

        Event eventObj = EventService.GetEventById(id);
        didFindCorrectEvent = eventObj.Id == id;

        Assert.True(didFindCorrectEvent);
    }

    [Fact]
    public void GetEventByTrainingId_With_TrainingId_2_Should_Return_2_Events_With_TrainingId()
    {
        bool didReturn2Events;
        int trainingId = 2;
        int expectedCount = 2;

        List<Event> events = EventService.GetEventsByTrainingId(trainingId);

        didReturn2Events = events.Count == expectedCount;

        Assert.True(didReturn2Events);
    }

    [Fact]
    public void GetEventsInRangeByTrainingId_Should_Return_Less_Events()
    {
        bool didReturnLessEvents;
        int eventsCount = Context.Events.Count();
        TimeStamp time = new(0, 0, 30, 0);
        int range = 10;

        List<Event> filteredEvents = EventService.GetEventsInRangeByTrainingId(_mockTrainings[0].Id, time, range);
        didReturnLessEvents = filteredEvents.Count() < eventsCount;

        Assert.True(didReturnLessEvents);
    }

    [Fact]
    public void GetEventsInRangeByTrainingId_With_Time_30s_And_Range_10s_Should_Return_3_Events_Within_Range()
    {
        bool didReturn3Events;
        TimeStamp time = new(0, 0, 30, 0);
        int range = 10;

        List<Event> filteredEvents = EventService.GetEventsInRangeByTrainingId(_mockTrainings[0].Id, time, range);
        didReturn3Events = filteredEvents.Count() == 3;

        Assert.True(didReturn3Events);
    }

    [Fact]
    public void GetEventsInRangeByTrainingId_With_Time_30s_And_Range_20s_Should_Return_4_Events_Within_Range()
    {
        bool didReturn4Events;
        TimeStamp time = new(0, 0, 30, 0);
        int range = 20;

        List<Event> filteredEvents = EventService.GetEventsInRangeByTrainingId(_mockTrainings[0].Id, time, range);
        didReturn4Events = filteredEvents.Count() == 4;

        Assert.True(didReturn4Events);
    }

    private void Setup()
    {
        var options = SqliteInMemory.CreateOptions<VRefSolutionsContext>();
        Context = new VRefSolutionsContext(options);
        Context.Database.EnsureCreated();

        EventService = new EventService(new EventRepository(Context));

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
                OverwriteSymbol = "",
                OverwriteName = "",
                TimeStamp = new TimeStamp(0, 0, 30, 0)
            },
            new Event
            {
                Id = 2,
                Training = _mockTrainings[0],
                EventType = _mockEventTypes[1],
                OverwriteMessage = "Custom message",
                OverwriteSymbol = "",
                OverwriteName = "",
                TimeStamp = new TimeStamp(0, 0, 39, 0)
            },
            new Event
            {
                Id = 3,
                Training = _mockTrainings[0],
                EventType = _mockEventTypes[0],
                OverwriteMessage = "",
                OverwriteSymbol = "",
                OverwriteName = "",
                TimeStamp = new TimeStamp(0, 0, 25, 0)
            },
            new Event
            {
                Id = 4,
                Training = _mockTrainings[0],
                EventType = _mockEventTypes[0],
                OverwriteMessage = "",
                OverwriteSymbol = "",
                OverwriteName = "",
                TimeStamp = new TimeStamp(0, 0, 10, 0)
            },
            new Event
            {
                Id = 5,
                Training = _mockTrainings[1],
                EventType = _mockEventTypes[0],
                OverwriteMessage = "",
                OverwriteSymbol = "",
                OverwriteName = "",
                TimeStamp = new TimeStamp(0, 0, 29, 0)
            },
            new Event
            {
                Id = 6,
                Training = _mockTrainings[1],
                EventType = _mockEventTypes[0],
                OverwriteMessage = "",
                OverwriteSymbol = "",
                OverwriteName = "",
                TimeStamp = new TimeStamp(0, 1, 50, 0)
            },
        });
        Context.Events.AddRange(_mockEvents);

        Context.SaveChanges();
    }
}