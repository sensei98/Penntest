using VRefSolutions.Domain.Entities;
namespace VRefSolutions.Repository.Interfaces
{
    public interface IEventRepository : IBaseRepository<Event>
    {
        List<Event> GetEventsByTrainingId(int trainingId);
    }
}