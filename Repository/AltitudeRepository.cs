using VRefSolutions.Domain.Entities;
using VRefSolutions.DAL;
using VRefSolutions.Repository.Interfaces;
using System.Linq.Expressions;

namespace VRefSolutions.Repository
{
    public class AltitudeRepository : IBaseRepository<Altitude>, IAltitudeRepository
    {
        private VRefSolutionsContext Context;

        public AltitudeRepository(VRefSolutionsContext context)
        {
            Context = context;
        }

        public Altitude Add(Altitude entity)
        {
            Context.Altitudes.Add(entity);
            Commit();
            return entity;
        }

        public IEnumerable<Altitude> AllIncluding(params Expression<Func<Altitude, object>>[] includeProperties)
        {
            throw new NotImplementedException();
        }

        public void Commit()
        {
            Context.SaveChanges();
        }

        public int Count()
        {
            return Context.Altitudes.Count();
        }

        public void Delete(Altitude entity)
        {
            Context.Altitudes.Remove(entity);
            Commit();
        }

        public void DeleteWhere(Expression<Func<Altitude, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Altitude> FindBy(Func<Altitude, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Altitude> GetAll()
        {
            return Context.Altitudes;
        }

        public Altitude GetSingle(int id)
        {
            return Context.Altitudes
                .Where(o => o.Id ==  id)
                .FirstOrDefault();
        }

        public Altitude GetSingle(Expression<Func<Altitude, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Altitude GetSingle(Expression<Func<Altitude, bool>> predicate, params Expression<Func<Altitude, object>>[] includeProperties)
        {
            throw new NotImplementedException();
        }

        public List<Altitude> GetAltitudesByTrainingId(int trainingId)
        {
            return Context.Altitudes
                .Where(o => o.Training.Id.Equals(trainingId))
                .ToList();
        }

        public Altitude Update(Altitude updatedAltitude)
        {
            Context.Altitudes.Update(updatedAltitude);
            Commit();
            return updatedAltitude;
        }
    }
}