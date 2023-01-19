using Azure.Storage.Blobs;
using VRefSolutions.Domain.Entities;
using VRefSolutions.Domain.Models;
using VRefSolutions.Repository;
using VRefSolutions.Repository.Interfaces;
using VRefSolutions.Service.Interfaces;

namespace VRefSolutions.Service
{
    public class EventService : IEventService
    {
        private IEventRepository EventRepository;
        public EventService(IEventRepository eventRepository)
        {
            EventRepository = eventRepository;
        }

        public Event CreateEvent(Event newEvent)
        {
            return EventRepository.Add(newEvent);
        }

        public Event GetEventById(int id)
        {
            return EventRepository.GetSingle(id);
        }

        public void DeleteEvent(Event eventToDelete)
        {
            EventRepository.Delete(eventToDelete);
        }

        public List<Event> GetEventsByTrainingId(int trainingId)
        {
            return EventRepository.GetEventsByTrainingId(trainingId);
        }

        public List<Event> GetEventsInRangeByTrainingId(int trainingId, TimeStamp time, int range)
        {
            return EventRepository.GetEventsByTrainingId(trainingId).Where(e => e.TimeStamp.IsWithinRange(time, range)).ToList();
        }

        public Event UpdateEvent(Event updatedEvent)
        {
            return EventRepository.Update(updatedEvent);
        }
    }

}
