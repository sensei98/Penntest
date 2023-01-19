using VRefSolutions.Domain.Entities;
using VRefSolutions.Repository.Interfaces;
using VRefSolutions.Service.Interfaces;

namespace VRefSolutions.Service
{
    public class OrganizationService : IOrganizationService
    {
        private IOrganizationRepository OrganizationRepository;
        public OrganizationService(IOrganizationRepository organizationRepository)
        {
            OrganizationRepository = organizationRepository;
        }

        public bool CheckIfOrganizationNameExists(string name)
        {
            Organization organization = OrganizationRepository.CheckIfOrganizationNameExists(name);
            if(object.ReferenceEquals(null, organization))
            {
                return false;
            }
            return true;
        }

        public Organization CreateOrganization(Organization organization)
        {
            // more logic (if name exists, etc.)
            return  OrganizationRepository.Add(organization);
        }

        public void delete(Organization organization)
        {
            OrganizationRepository.Delete(organization);
        }

        public List<Organization> GetAllOrganizations()
        {
            return OrganizationRepository.GetAllOrganizations();
        }

        public Organization GetOrganizationById(int id)
        {            
            return OrganizationRepository.GetSingle(id);
        }

        public Organization updateOrganization(Organization organization)
        {
            return OrganizationRepository.Update(organization);
        }
    }

}