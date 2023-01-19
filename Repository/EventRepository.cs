using VRefSolutions.Domain.Entities;
using VRefSolutions.DAL;
using VRefSolutions.Repository.Interfaces;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace VRefSolutions.Repository
{
    public class EventRepository : IBaseRepository<Event>, IEventRepository
    {
        private VRefSolutionsContext Context;
        public EventRepository(VRefSolutionsContext context)
        {
            Context = context;
        }

        public Event Add(Event entity)
        {
            Context.Events.Add(entity);
            Commit();
            return entity;
        }

        public IEnumerable<Event> AllIncluding(params Expression<Func<Event, object>>[] includeProperties)
        {
            throw new NotImplementedException();
        }

        public void Commit()
        {
            Context.SaveChanges();
        }

        public int Count()
        {
            return Context.Events.Count();
        }

        public void Delete(Event entity)
        {
            Context.Events.Remove(entity);
            Commit();
        }

        public void DeleteWhere(Expression<Func<Event, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Event> FindBy(Func<Event, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Event> GetAll()
        {
            return Context.Events;
        }

        public Event GetSingle(int id)
        {
            return Context.Events.Where(o => o.Id ==  id)
                .Include(o => o.EventType)
                .FirstOrDefault();
        }

        public Event GetSingle(Expression<Func<Event, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Event GetSingle(Expression<Func<Event, bool>> predicate, params Expression<Func<Event, object>>[] includeProperties)
        {
            throw new NotImplementedException();
        }

        public List<Event> GetEventsByTrainingId(int trainingId)
        {
            return Context.Events
                .Where(o => o.Training.Id.Equals(trainingId))
                .Include(o => o.EventType)
                .ToList();
        }

        public Event Update(Event updatedEvent)
        {
            Context.Events.Update(updatedEvent);
            Commit();
            return updatedEvent;
        }
    }
}