using VRefSolutions.Domain.Entities;
using VRefSolutions.DAL;
using VRefSolutions.Repository.Interfaces;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace VRefSolutions.Repository
{
    public class TrainingRepository : ITrainingRepository
    {
        private VRefSolutionsContext Context;
        public TrainingRepository(VRefSolutionsContext context)
        {
            Context = context;
        }

        public Training Add(Training entity)
        {
            Context.Trainings.Add(entity);
            Commit();
            return entity;
        }

        public IEnumerable<Training> AllIncluding(params Expression<Func<Training, object>>[] includeProperties)
        {
            throw new NotImplementedException();
        }

        public void Commit()
        {
            Context.SaveChanges();
        }

        public int Count()
        {
            return Context.Trainings.Count();
        }

        public void Delete(Training entity)
        {
            Context.Trainings.Remove(entity);
            Commit();
        }

        public void DeleteWhere(Expression<Func<Training, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Training> FindBy(Func<Training, bool> predicate)
        {
            return Context.Trainings
                  .Include(t => t.Participants)
                  .ThenInclude(u => u.Organization)
                  .Where(predicate);
        }

        public IEnumerable<Training> GetAll()
        {
            return Context.Trainings
            .Include(t => t.Participants)
            .ThenInclude(u => u.Organization);
        }

        public IEnumerable<Training> GetByUserId(int loggedInUserId)
        {
            return Context.Trainings.IgnoreAutoIncludes().Where(t => t.Participants.Any(u => u.Id == loggedInUserId))
                .Include(t => t.Participants)
                .ThenInclude(u => u.Organization);
        }

        public IEnumerable<Training> GetByOrganizationId(int organizationId)
        {
            return Context.Trainings.IgnoreAutoIncludes().Where(t => t.Participants.Any(u => u.Organization.Id == organizationId))
                .Include(t => t.Participants)
                .ThenInclude(u => u.Organization);
        }

        public Training GetSingle(int id)
        {
            return Context.Trainings.Where(t => t.Id == id)
                .Include(t => t.Events)
                .Include(t => t.Altitudes)
                .Include(t => t.Participants)
                .ThenInclude(u => u.Organization)
                .FirstOrDefault();
        }

        public Training GetSingle(Expression<Func<Training, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Training GetSingle(Expression<Func<Training, bool>> predicate, params Expression<Func<Training, object>>[] includeProperties)
        {
            throw new NotImplementedException();
        }

        public Training Update(Training entity)
        {
            Context.Trainings.Update(entity);
            Commit();
            return entity;
        }
    }
}