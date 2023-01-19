using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestSupport.EfHelpers;
using VRefSolutions.DAL;
using VRefSolutions.Domain.Entities;
using VRefSolutions.Domain.Enums;
using VRefSolutions.Repository;
using VRefSolutions.Service;

namespace TestUnit.Services
{
    public class JwtMiddleWareTests
    {

        private VRefSolutionsContext Context;
        private UserService userService;
        private TokenService tokenService;

        public JwtMiddleWareTests()
        {
            var options = SqliteInMemory.CreateOptions<VRefSolutionsContext>();
            Context = new VRefSolutionsContext(options);
            Context.Database.EnsureCreated();

            Organization testOrganization = new Organization();
            testOrganization.Name = "Test Org 1";
            testOrganization.Users = new List<User>();
            Context.Organizations.Add(testOrganization);

            User Student = new User { Email = "EmailStudentTest", FirstName = "FirstNameStudentTest", LastName = "LastNameStudentTest", Password = "StudentTest", UserType = Role.Student, Organization = testOrganization, ActivationCode = "ABCDEFGH", Trainings = new List<Training>() };
            Context.Users.Add(Student);
            Context.SaveChanges();



            var logfactory = new LoggerFactory();
            ILogger<TokenService> logger = logfactory.CreateLogger<TokenService>();
            var inMemorySettings = new Dictionary<string, string> { };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            userService = new UserService(new UserRepository(Context));
            tokenService = new TokenService(configuration, logger);
        }


        [Fact]
        public void CreateTokenForLoggedInUser()
        {
            User user = userService.GetUserById(1);
            var userLoginResponseDTO = tokenService.CreateToken(user);
            Assert.Equal(userLoginResponseDTO.Result.TokenType, "Bearer");
        }

        [Fact]
        public void GetValueForMiddleWare()
        {
            User userToLogin = userService.GetUserById(1);
            var userLoginResponseDTO = tokenService.CreateToken(userToLogin);

            var getLoggedInUserEmail = tokenService.GetByValue(userLoginResponseDTO.Result.AccessToken);

            Assert.Equal(userToLogin.Email, getLoggedInUserEmail.Result.Identity.Name);
        }


    }

}