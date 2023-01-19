using VRefSolutions.Domain.Entities;
using VRefSolutions.DAL;
using VRefSolutions.Repository.Interfaces;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace VRefSolutions.Repository
{
    public class EventTypeRepository : IBaseRepository<EventType>, IEventTypeRepository
    {
        private VRefSolutionsContext Context;
        public EventTypeRepository(VRefSolutionsContext context)
        {
            Context = context;
        }

        public EventType Add(EventType entity)
        {
            Context.EventTypes.Add(entity);
            Commit();
            return entity;
        }

        public IEnumerable<EventType> AllIncluding(params Expression<Func<EventType, object>>[] includeProperties)
        {
            throw new NotImplementedException();
        }

        public void Commit()
        {
            Context.SaveChanges();
        }

        public int Count()
        {
            return Context.EventTypes.Count();
        }

        public void Delete(EventType entity)
        {
            Context.EventTypes.Remove(entity);
            Commit();
        }

        public void DeleteWhere(Expression<Func<EventType, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public EventType GetByName(string name)
        {
            return Context.EventTypes.IgnoreAutoIncludes().Where(t => t.Name == name)?.First();
        }

        public IEnumerable<EventType> FindBy(Func<EventType, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<EventType> GetAll()
        {
            return Context.EventTypes;
        }

        public EventType GetSingle(int id)
        {
            return Context.EventTypes.Where(o => o.Id == id).FirstOrDefault();
        }

        public EventType GetSingle(Expression<Func<EventType, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public EventType GetSingle(Expression<Func<EventType, bool>> predicate, params Expression<Func<EventType, object>>[] includeProperties)
        {
            throw new NotImplementedException();
        }

        public EventType Update(EventType entity)
        {
            Context.EventTypes.Update(entity);
            Commit();
            return entity;
        }
    }
}