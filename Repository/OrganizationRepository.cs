using VRefSolutions.Domain.Entities;
using VRefSolutions.DAL;
using VRefSolutions.Repository.Interfaces;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace VRefSolutions.Repository
{
    public class OrganizationRepository : IBaseRepository<Organization> ,IOrganizationRepository
    {
        private VRefSolutionsContext Context;
        public OrganizationRepository(VRefSolutionsContext context)
        {
            Context = context;
        }

        public Organization Add(Organization entity)
        {
            Context.Organizations.Add(entity);
            Commit();
            return entity;
        }

        public IEnumerable<Organization> AllIncluding(params Expression<Func<Organization, object>>[] includeProperties)
        {
            throw new NotImplementedException();
        }

        public void Commit()
        {
            Context.SaveChanges();
        }

        public int Count()
        {
            return Context.Organizations.Count();
        }

        public void Delete(Organization entity)
        {
            Context.Organizations.Remove(entity);
            Commit();
        }

        public void DeleteWhere(Expression<Func<Organization, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Organization> FindBy(Func<Organization, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public List<Organization> GetAllOrganizations()
        {
            List<Organization> organizations = new List<Organization>();
            organizations = Context.Set<Organization>().ToList();
            return organizations;
        }

        public Organization GetSingle(int id)
        {
            return Context.Organizations.Where(o => o.Id ==  id).Include(o => o.Users).FirstOrDefault();
        }

        public Organization CheckIfOrganizationNameExists(string name)
        {
            return Context.Organizations.Where(o => o.Name.Equals(name)).FirstOrDefault();
        }

        public Organization GetSingle(Expression<Func<Organization, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Organization GetSingle(Expression<Func<Organization, bool>> predicate, params Expression<Func<Organization, object>>[] includeProperties)
        {
            throw new NotImplementedException();
        }

        public Organization Update(Organization organization)
        {
            Context.Organizations.Update(organization);
            Commit();
            return organization;
        }

        public IEnumerable<Organization> GetAll()
        {
            throw new NotImplementedException();
        }

        public List<Organization> GetAllOrganizationsById(int id)
        {
            List<Organization> organizations = new List<Organization>();
            organizations = Context.Set<Organization>().Include(o => o.Users).ToList();
            var result = organizations.Where(o => o.Id == id);
            return result.ToList();
        }
    }
}