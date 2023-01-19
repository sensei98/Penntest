using VRefSolutions.Domain.DTO;
using VRefSolutions.Domain.Entities;
namespace VRefSolutions.Repository.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>{
        User GetListByActivationCode(string activationCode);
        List<User> getUsersBySearch(string searchField);
        bool GetUserExistsByEmail(string email);
        User GetUserByEmail(string email);
        User GetUserByCredentials(string email, string password);
    }
}