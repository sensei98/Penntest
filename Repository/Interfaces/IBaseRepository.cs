using System.Linq.Expressions;
using VRefSolutions.Domain.Interfaces;

namespace VRefSolutions.Repository.Interfaces
{
    public interface IBaseRepository<T> where T : class, IBaseEntity, new()
    {
        IEnumerable<T> AllIncluding(params Expression<Func<T, object>>[] includeProperties);
        IEnumerable<T> GetAll();
        int Count();
        T GetSingle(int id);
        T GetSingle(Expression<Func<T, bool>> predicate);
        T GetSingle(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties);
        IEnumerable<T> FindBy(Func<T, bool> predicate);
        T Add(T entity);
        T Update(T entity);
        void Delete(T entity);
        void DeleteWhere(Expression<Func<T, bool>> predicate);
        void Commit();
    }
}