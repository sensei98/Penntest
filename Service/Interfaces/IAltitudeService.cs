using VRefSolutions.Domain.Entities;
using VRefSolutions.Domain.Models;

namespace VRefSolutions.Service.Interfaces
{
    public interface IEventService{
        Event CreateEvent(Event newEvent);
        Event GetEventById(int id);
        void DeleteEvent(Event eventToDelete);
        List<Event> GetEventsByTrainingId(int trainingId);
        List<Event> GetEventsInRangeByTrainingId(int trainingId, TimeStamp time, int range);
        Event UpdateEvent(Event updatedEvent);
    }
}