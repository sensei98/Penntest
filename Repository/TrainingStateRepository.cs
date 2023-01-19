using VRefSolutions.Domain.Entities;
using VRefSolutions.DAL;
using VRefSolutions.Repository.Interfaces;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace VRefSolutions.Repository
{
    public class TrainingStateRepository : IBaseRepository<TrainingState>, ITrainingStateRepository
    {
        private VRefSolutionsContext Context;

        public TrainingStateRepository(VRefSolutionsContext context)
        {
            Context = context;
        }

        public TrainingState Add(TrainingState entity)
        {
            Context.TrainingStates.Add(entity);
            Commit();
            return entity;
        }

        public IEnumerable<TrainingState> AllIncluding(params Expression<Func<TrainingState, object>>[] includeProperties)
        {
            throw new NotImplementedException();
        }

        public void Commit()
        {
            Context.SaveChanges();
        }

        public int Count()
        {
            return Context.TrainingStates.Count();
        }

        public void Delete(TrainingState entity)
        {
            Context.TrainingStates.Remove(entity);
            Commit();
        }

        public void DeleteWhere(Expression<Func<TrainingState, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TrainingState> FindBy(Func<TrainingState, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TrainingState> GetAll()
        {
            return Context.TrainingStates;
        }

        public TrainingState? GetByTrainingId(int trainingId)
        {
            return Context.TrainingStates.Where(t => t.Training.Id == trainingId)?.First();
        }

        public TrainingState GetSingle(int id)
        {
            return Context.TrainingStates
                .Where(o => o.Id ==  id)
                .FirstOrDefault();
        }

        public TrainingState GetSingle(Expression<Func<TrainingState, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public TrainingState GetSingle(Expression<Func<TrainingState, bool>> predicate, params Expression<Func<TrainingState, object>>[] includeProperties)
        {
            throw new NotImplementedException();
        }

        public TrainingState Update(TrainingState updatedTrainingState)
        {
            Context.TrainingStates.Update(updatedTrainingState);
            Commit();
            return updatedTrainingState;
        }
    }
}