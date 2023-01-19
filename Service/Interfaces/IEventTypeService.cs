using VRefSolutions.Domain.Entities;
namespace VRefSolutions.Service.Interfaces
{
    public interface IEventTypeService
    {
        EventType CreateEventType(EventType EventType);
        EventType GetEventTypeById(int id);
        EventType GetByName(string name);
    }
}