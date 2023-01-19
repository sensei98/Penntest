using VRefSolutions.Domain.Entities;
namespace VRefSolutions.Repository.Interfaces
{
    public interface IOrganizationRepository : IBaseRepository<Organization>{

        public Organization CheckIfOrganizationNameExists(string name);
        public List<Organization> GetAllOrganizations();
        public List<Organization> GetAllOrganizationsById(int id);
    }
}