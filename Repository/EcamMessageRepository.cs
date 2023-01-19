using VRefSolutions.Domain.Entities;
using VRefSolutions.DAL;
using VRefSolutions.Repository.Interfaces;
using System.Linq.Expressions;

namespace VRefSolutions.Repository
{
    public class EcamMessageRepository : IBaseRepository<EcamMessage>, IEcamMessageRepository
    {
        private VRefSolutionsContext Context;

        public EcamMessageRepository(VRefSolutionsContext context)
        {
            Context = context;
        }

        public EcamMessage Add(EcamMessage entity)
        {
            Context.EcamMessages.Add(entity);
            Commit();
            return entity;
        }

        public IEnumerable<EcamMessage> AllIncluding(params Expression<Func<EcamMessage, object>>[] includeProperties)
        {
            throw new NotImplementedException();
        }

        public void Commit()
        {
            Context.SaveChanges();
        }

        public int Count()
        {
            return Context.EcamMessages.Count();
        }

        public void Delete(EcamMessage entity)
        {
            Context.EcamMessages.Remove(entity);
            Commit();
        }

        public void DeleteWhere(Expression<Func<EcamMessage, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<EcamMessage> FindBy(Func<EcamMessage, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<EcamMessage> GetAll()
        {
            return Context.EcamMessages;
        }

        public EcamMessage GetSingle(int id)
        {
            return Context.EcamMessages.Where(o => o.Id == id).FirstOrDefault();
        }

        public EcamMessage GetSingle(Expression<Func<EcamMessage, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public EcamMessage GetSingle(Expression<Func<EcamMessage, bool>> predicate, params Expression<Func<EcamMessage, object>>[] includeProperties)
        {
            throw new NotImplementedException();
        }

        public EcamMessage Update(EcamMessage entity)
        {
            Context.EcamMessages.Update(entity);
            Commit();
            return entity;
        }
    }
}