using VRefSolutions.Domain.Entities;
namespace VRefSolutions.Service.Interfaces
{
    public interface IOrganizationService{
        Organization CreateOrganization(Organization organization);
        Organization GetOrganizationById(int id);
        bool CheckIfOrganizationNameExists(string name);
        List<Organization> GetAllOrganizations();
        public void delete(Organization organization);
        public Organization updateOrganization(Organization organization);
    }
}