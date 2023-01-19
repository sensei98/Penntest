using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using VRefSolutions.Domain.DTO;
using VRefSolutions.Domain.Entities;
using VRefSolutions.Domain.Enums;
using VRefSolutions.Repository.Interfaces;
using VRefSolutions.Service.Interfaces;

namespace VRefSolutions.Service
{
    public class UserService : IUserService
    {
        private IUserRepository UserRepository;

        private const int _saltSize = 16; // 128 bits
        private const int _keySize = 32; // 256 bits
        private const int _iterations = 100000;
        private static readonly HashAlgorithmName _algorithm = HashAlgorithmName.SHA256;

        private const char segmentDelimiter = ':';
        public UserService(IUserRepository userRepository)
        {
            UserRepository = userRepository;
        }
        public User CreateUser(User user)
        {
            return UserRepository.Add(user);
        }

        public User GetUserByActivationCode(string activationCode)
        {
            return UserRepository.GetListByActivationCode(activationCode);
        }

        public User GetUserByCredentials(string email, string password)
        {
            return UserRepository.GetUserByCredentials(email, password);
        }

        public User GetUserByEmail(string email)
        {
            return UserRepository.GetUserByEmail(email);
        }

        public User GetUserById(int id)
        {
            return UserRepository.GetSingle(id);
        }

        public List<User> getUsersBySearch(string searchField)
        {
            return UserRepository.getUsersBySearch(searchField);
        }

        public bool userExistsByEmail(string email)
        {
            return UserRepository.GetUserExistsByEmail(email);
        }

        public User UpdateUser(User user)
        {
            return UserRepository.Update(user);
        }

        public string HashPassword(string secret)
        {
            var salt = RandomNumberGenerator.GetBytes(_saltSize);
            var key = Rfc2898DeriveBytes.Pbkdf2(
                secret,
                salt,
                _iterations,
                _algorithm,
                _keySize
            );
            return string.Join(
                segmentDelimiter,
                Convert.ToHexString(key),
                Convert.ToHexString(salt),
                _iterations,
                _algorithm
            );
        }

        public bool verifyPasswordHash(string secret, string hash)
        {
            var segments = hash.Split(segmentDelimiter);
            var key = Convert.FromHexString(segments[0]);
            var salt = Convert.FromHexString(segments[1]);
            var iterations = int.Parse(segments[2]);
            var algorithm = new HashAlgorithmName(segments[3]);
            var inputSecretKey = Rfc2898DeriveBytes.Pbkdf2(
                secret,
                salt,
                iterations,
                algorithm,
                key.Length
            );
            return key.SequenceEqual(inputSecretKey);
        }

        public bool checkIfUserIsSameForRequestedId(int requestingUser, int id)
        {
            User userFromDB = UserRepository.GetSingle(id);
            if (object.ReferenceEquals(null, userFromDB))
            {
                return false;
            }            
            if (requestingUser == userFromDB.Id)
            {
                return true;
            }
            return false;            
        }

        public bool IsObjectOfSuperAdmin(int id)
        {
            User userFromDB = UserRepository.GetSingle(id);
            if (userFromDB.UserType.ToString() == "SuperAdmin")
            {
                return true;
            }
            return false;
        }

        public bool IsObjectOfAdmin(int id)
        {
            User userFromDB = UserRepository.GetSingle(id);
            if (userFromDB.UserType.ToString() == "Admin")
            {
                return true;
            }
            return false;
        }

        public List<string> GetChangedProperties<T>(UserUpdateDTO A, UserUpdateDTO B)
        {
            List<string> changes = new List<string>();
            if (A != null && B != null)
            {
                var type = typeof(T);
                var allProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var allSimpleProperties = allProperties.Where(pi => pi.PropertyType.IsSimpleType());
                var unequalProperties =
                       from pi in allSimpleProperties
                       let AValue = type.GetProperty(pi.Name).GetValue(A, null)
                       let BValue = type.GetProperty(pi.Name).GetValue(B, null)
                       where AValue != BValue && (AValue == null || !AValue.Equals(BValue))select pi.Name;
                changes = unequalProperties.ToList();
                if (A.Organization.Id != B.Organization.Id)
                {
                    changes.Add("OrganizationId");
                }
                return changes;
            }
            else
            {
                throw new ArgumentNullException("You need to provide 2 non-null objects");
            }
        }

        public void delete(User user)
        {
            UserRepository.Delete(user);
        }

        public string Truncate(string value, int maxLength)
        {
            return value?.Length > maxLength
                ? value.Substring(0, maxLength)
                : value;
        }
    }

}