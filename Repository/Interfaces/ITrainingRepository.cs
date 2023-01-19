using VRefSolutions.Domain.Entities;
namespace VRefSolutions.Repository.Interfaces
{
    public interface ITrainingRepository : IBaseRepository<Training>
    {
        IEnumerable<Training> GetByUserId(int loggedInUserId);
        IEnumerable<Training> GetByOrganizationId(int organizationId);
    }
}