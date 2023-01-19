using VRefSolutions.Domain.Entities;
namespace VRefSolutions.Repository.Interfaces
{
    public interface IEventTypeRepository : IBaseRepository<EventType>
    {
        public EventType? GetByName(string name);
    }
}