using VRefSolutions.Domain.Entities;
using VRefSolutions.Repository;
using VRefSolutions.Repository.Interfaces;
using VRefSolutions.Service.Interfaces;

namespace VRefSolutions.Service
{
    public class EventTypeService : IEventTypeService
    {
        private IEventTypeRepository EventTypeRepository;

        public EventTypeService(IEventTypeRepository eventTypeRepository)
        {
            EventTypeRepository = eventTypeRepository;
        }

        public EventType CreateEventType(EventType EventType)
        {
            return EventTypeRepository.Add(EventType);
        }

        public EventType GetEventTypeById(int id)
        {
            return EventTypeRepository.GetSingle(id);
        }

        public EventType GetByName(string name)
        {
            return EventTypeRepository.GetByName(name);
        }

        public EventType GetEventTypeByName(string name)
        {
            return EventTypeRepository.GetByName(name);
        }

        public IEnumerable<EventType> GetEventTypes()
        {
            return EventTypeRepository.GetAll();
        }
    }

}