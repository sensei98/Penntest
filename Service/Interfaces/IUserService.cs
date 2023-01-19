using Microsoft.Azure.Functions.Worker.Http;
using System.Security.Claims;
using VRefSolutions.Domain.DTO;
using VRefSolutions.Domain.Entities;
using VRefSolutions.Domain.Enums;

namespace VRefSolutions.Service.Interfaces
{
    public interface IUserService{
        User CreateUser(User user);
        User GetUserById(int id);
        User UpdateUser(User user);       
        User GetUserByEmail(string email);
        List<User> getUsersBySearch(string searchField);
        User GetUserByCredentials(string email, string password);
        User GetUserByActivationCode(string activationCode);
        bool userExistsByEmail(string email);
        string HashPassword(string secret);
        bool verifyPasswordHash(string secret, string hash);
        public bool checkIfUserIsSameForRequestedId(int requestingUser, int TargetUser);
        public bool IsObjectOfSuperAdmin(int id);
        public bool IsObjectOfAdmin(int id);
        public List<string> GetChangedProperties<T>(UserUpdateDTO A, UserUpdateDTO B);
        public void delete(User user);
        public string Truncate(string value, int maxLength);
    }
}