using VRefSolutions.Domain.Entities;
using VRefSolutions.DAL;
using VRefSolutions.Repository.Interfaces;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using VRefSolutions.Domain.DTO;

namespace VRefSolutions.Repository
{
    public class UserRepository : IUserRepository
    {
        private VRefSolutionsContext Context;
        public UserRepository(VRefSolutionsContext context)
        {
            Context = context;
        }

        public User Add(User entity)
        {
            Context.Users.Add(entity);
            Commit();
            return entity;
        }

        public IEnumerable<User> AllIncluding(params Expression<Func<User, object>>[] includeProperties)
        {
            throw new NotImplementedException();
        }

        public void Commit()
        {
            Context.SaveChanges();
        }

        public int Count()
        {
            return Context.Users.Count();
        }

        public void Delete(User entity)
        {
            Context.Users.Remove(entity);
            Commit();
        }

        public void DeleteWhere(Expression<Func<User, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<User> FindBy(Func<User, bool> predicate)
        {
            return Context.Set<User>().Where(predicate);
        }

        public IEnumerable<User> GetAll()
        {
            throw new NotImplementedException();
        }

        public User GetSingle(int id)
        {
            User user = Context.Users.Where(u=>u.Id == id).Include(u => u.Organization).FirstOrDefault();
            return user;
        }

        public User GetSingle(Expression<Func<User, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public User GetSingle(Expression<Func<User, bool>> predicate, params Expression<Func<User, object>>[] includeProperties)
        {
            throw new NotImplementedException();
        }

        public User Update(User user)
        {
            Context.Users.Update(user);
            Commit();
            return user;
        }

        public User GetListByActivationCode(string activationCode)
        {
            return Context.Users.Where(U => U.ActivationCode.Equals(activationCode)).FirstOrDefault();            
        }

        public bool GetUserExistsByEmail(string email)
        {
            List<User> users = new List<User>(Context.Users.Where(U => U.Email.Equals(email)));
            if (users.Count > 0)
            {
                return true;
            }
            return false;
        }

        public User GetUserByEmail(string email)
        {
            return Context.Users.Where(u => u.Email.Equals(email)).Include(u => u.Organization).FirstOrDefault();
        }

        public User GetUserByCredentials(string email, string password)
        {
            return Context.Users.Where(u => u.Email.Equals(email) && u.Password.Equals(password)).FirstOrDefault();
        }

        public List<User> getUsersBySearch(string searchField)
        {
            List<User> allUsers = new List<User>();
            allUsers = Context.Set<User>().Include(u => u.Organization).ToList();
            if (!String.IsNullOrEmpty(searchField))
            {
                var result = allUsers.Where(u => u.Email.Contains(searchField) || u.FirstName.Contains(searchField) || u.LastName.Contains(searchField));

                return result.ToList();
            }
            return allUsers;
        }
    }
}