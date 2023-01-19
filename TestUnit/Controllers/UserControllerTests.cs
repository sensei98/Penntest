using AutoMapper;
using Company.VRefSolutions.Controller;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TestHelper;
using VRefSolutions.Domain.DTO;
using VRefSolutions.Domain.Entities;
using VRefSolutions.Domain.Enums;
using VRefSolutions.Repository.Interfaces;
using VRefSolutions.Service;
using VRefSolutions.Service.Interfaces;
using Xunit.Extensions.AssertExtensions;

namespace TestUnit
{
    public class UserControllerTests : ControllerTests
    {
        private UserController UserController;
        private UserService UserService;
        private OrganizationService OrganizationService;
        private Organization organization;
        private User testStudentUser;
        private User testStudentUser2;
        private User testAdminUser;


        public UserControllerTests()
        {
            UserController = new UserController(    host.Services.GetRequiredService<ILogger<UserController>>(),
                                                    host.Services.GetRequiredService<IUserService>(),
                                                    host.Services.GetRequiredService<IOrganizationService>(),
                                                    host.Services.GetRequiredService<ITokenService>(),
                                                    host.Services.GetRequiredService<IEmailService>(),
                                                    host.Services.GetRequiredService<IMapper>());

            OrganizationService = new OrganizationService(host.Services.GetRequiredService<IOrganizationRepository>());
            UserService = new UserService(host.Services.GetRequiredService<IUserRepository>());
            // Setup all your database entities. 
            organization = new Organization { Name = "The Testing Organization" };
            OrganizationService.CreateOrganization(organization);
            string password = UserService.HashPassword("test");
            testStudentUser = new User { Email = "Student@gmail.com", FirstName = "TestFirstName", LastName = "TestLastName", Password = password, ActivationCode = "abcdefgh", Organization = organization, Trainings = new List<Training>(), UserType = Role.Student };
            UserService.CreateUser(testStudentUser);
            testAdminUser = new User { Email = "Admin@gmail.com", FirstName = "TestAdminFirstName", LastName = "TestAdminLastName", Password = password, ActivationCode = "abcdefgh", Organization = organization, Trainings = new List<Training>(), UserType = Role.Admin };
            UserService.CreateUser(testAdminUser);
            testStudentUser2 = new User { Email = "Student2@gmail.com", FirstName = "TestFirstName2", LastName = "TestLastName2", Password = password, ActivationCode = "abcdefgh", Organization = organization, Trainings = new List<Training>(), UserType = Role.Student };
            UserService.CreateUser(testStudentUser2);

            organization.ShouldNotBeNull();
            organization.Id.ShouldEqual(1);
            
            UserController.ShouldNotBeNull();

        }

        #region Create user 

        [Fact]
        public async Task CreateUser_With_SuperAdministrator_Should_Return_OK()
        {
            var fakeContext = new FakeFunctionContext("1", "SuperAdmin", host);
            string json = JsonConvert.SerializeObject(
            new UserCreateDTO
            {
                Email = "testemail@gmail.com",FirstName = "testFirstname",LastName = "testLastName", Organization = new OrganizationDTO { Id = organization.Id }
            }
            );
            var body = new MemoryStream(Encoding.Default.GetBytes(json));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.createUser(request, fakeContext);
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CreateUser_With_Admin_Role_Should_Return_OK()
        {
            var fakeContext = new FakeFunctionContext("1", "Admin", host);
            string json = JsonConvert.SerializeObject(
            new UserCreateDTO
            {
                Email = "testemail@gmail.com",
                FirstName = "testFirstname",
                LastName = "testLastName",
                Organization = new OrganizationDTO { Id = organization.Id }
            }
            );
            var body = new MemoryStream(Encoding.Default.GetBytes(json));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.createUser(request, fakeContext);
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CreateUser_With_Instructor_Role_Should_Return_Unauthorized()
        {
            var fakeContext = new FakeFunctionContext("1", "Instructor", host);
            string json = JsonConvert.SerializeObject(
            new UserCreateDTO
            {
                Email = "testemail@gmail.com",
                FirstName = "testFirstname",
                LastName = "testLastName",
                Organization = new OrganizationDTO { Id = organization.Id }
            }
            );
            var body = new MemoryStream(Encoding.Default.GetBytes(json));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.createUser(request, fakeContext);
            response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateUser_With_Student_Role_Should_Return_Unauthorized()
        {
            var fakeContext = new FakeFunctionContext("1", "Student", host);
            string json = JsonConvert.SerializeObject(
            new UserCreateDTO
            {
                Email = "testemail@gmail.com",
                FirstName = "testFirstname",
                LastName = "testLastName",
                Organization = new OrganizationDTO { Id = organization.Id }
            }
            );
            var body = new MemoryStream(Encoding.Default.GetBytes(json));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.createUser(request, fakeContext);
            response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateUser_With_Wrong_Email_Should_Return_BadRequest()
        {
            var fakeContext = new FakeFunctionContext("1", "SuperAdmin", host);
            string json = JsonConvert.SerializeObject(
            new UserCreateDTO
            {
                Email = "testemail@gm",
                FirstName = "testFirstname",
                LastName = "testLastName",
                Organization = new OrganizationDTO { Id = organization.Id }
            }
            );
            var body = new MemoryStream(Encoding.Default.GetBytes(json));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.createUser(request, fakeContext);
            response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateUser_With_Already_Existing_Email_Should_Return_Conflict()
        {
            var fakeContext = new FakeFunctionContext("1", "SuperAdmin", host);
            string json = JsonConvert.SerializeObject(
            new UserCreateDTO
            {
                Email = testStudentUser.Email,
                FirstName = "testFirstname",
                LastName = "testLastName",
                Organization = new OrganizationDTO { Id = organization.Id }
            }
            );
            var body = new MemoryStream(Encoding.Default.GetBytes(json));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.createUser(request, fakeContext);
            response.StatusCode.ShouldEqual(HttpStatusCode.Conflict);
        }

        #endregion

        #region Activate user

        [Fact]
        public async Task ActivateUser_Correct_ActivationCode_Return_OK()
        {
            var fakeContext = new FakeFunctionContext("1", "SuperAdmin", host);
            string json = JsonConvert.SerializeObject(
            new UserActivateDTO
            {
                ActivationCode = testStudentUser.ActivationCode,
                Password = "test"
            }
            );
            var body = new MemoryStream(Encoding.Default.GetBytes(json));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.activateUser(request, fakeContext, "abcdefgh");
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ActivateUser_InCorrect_ActivationCode_Return_NotFound()
        {
            var fakeContext = new FakeFunctionContext("1", "SuperAdmin", host);
            string json = JsonConvert.SerializeObject(
            new UserActivateDTO
            {
                ActivationCode = testStudentUser.ActivationCode,
                Password = "test"
            }
            );
            var body = new MemoryStream(Encoding.Default.GetBytes(json));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.activateUser(request, fakeContext, "abcdefgi");
            response.StatusCode.ShouldEqual(HttpStatusCode.NotFound);
        }

        // check on password length with validators #tim

        #endregion

        #region Login

        [Fact]
        public async Task Login_User_With_Correct_Credentials_Return_OK()
        {
            var fakeContext = new FakeFunctionContext("1", "SuperAdmin", host);
            string json = JsonConvert.SerializeObject(
            new UserLoginRequestDTO
            {
                Email = testStudentUser.Email,
                Password = "test"
            }
            );
            var body = new MemoryStream(Encoding.Default.GetBytes(json));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.login(request);
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Login_User_With_Incorrect_Credentials_Return_BadRequest()
        {
            var fakeContext = new FakeFunctionContext("1", "SuperAdmin", host);
            string json = JsonConvert.SerializeObject(
            new UserLoginRequestDTO
            {
                Email = testStudentUser.Email,
                Password = "test1"
            }
            );
            var body = new MemoryStream(Encoding.Default.GetBytes(json));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.login(request);
            response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
        }


        #endregion

        #region Get one user

        [Fact]
        public async Task Get_Student_User_With_SuperAdmin_Role_Return_OK()
        {
            var fakeContext = new FakeFunctionContext("1", "SuperAdmin", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.getUserAsync(request, fakeContext, 1);
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_Student_User_With_Admin_Role_Return_OK()
        {
            var fakeContext = new FakeFunctionContext("1", "Admin", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.getUserAsync(request, fakeContext, 1);
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_Student_User_With_Instructor_Role_Return_Forbidden()
        {
            var fakeContext = new FakeFunctionContext("1", "Instructor", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.getUserAsync(request, fakeContext, 1);
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_Other_Student_User_With_Student_Role_Return_Forbidden()
        {
            var fakeContext = new FakeFunctionContext("1", "Student", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.getUserAsync(request, fakeContext, 3);
            response.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Get_Own_User_With_Student_Role_Return_OK()
        {
            var fakeContext = new FakeFunctionContext("1", "Student", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.getUserAsync(request, fakeContext, 1);
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }

        #endregion

        #region Get multiple users

        [Fact]
        public async Task Get_Users_With_SuperAdmin_Role_Return_OK()
        {
            var fakeContext = new FakeFunctionContext("1", "SuperAdmin", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.getUsers(request, fakeContext);
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_Users_With_Admin_Role_Return_OK()
        {
            var fakeContext = new FakeFunctionContext("1", "Admin", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.getUsers(request, fakeContext);
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_Users_With_Instructor_Role_Return_OK()
        {
            var fakeContext = new FakeFunctionContext("1", "Instructor", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.getUsers(request, fakeContext);
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_Users_With_Student_Role_Return_Unauthorized()
        {
            var fakeContext = new FakeFunctionContext("1", "Student", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.getUsers(request, fakeContext);
            response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Get_Users_With_SearchField_And_Role_SuperAdmin_Return_OK()
        {
            var fakeContext = new FakeFunctionContext("1", "SuperAdmin", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.getUsers(request, fakeContext, "Student");
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_Users_With_SearchField_And_Role_Admin_Return_OK()
        {
            var fakeContext = new FakeFunctionContext("1", "Admin", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.getUsers(request, fakeContext, "Student");
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_Users_With_SearchField_And_Role_Instructor_Return_OK()
        {
            var fakeContext = new FakeFunctionContext("1", "Instructor", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.getUsers(request, fakeContext, "Student");
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_Users_With_SearchField_And_Role_Student_Return_Unauthorized()
        {
            var fakeContext = new FakeFunctionContext("1", "Student", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.getUsers(request, fakeContext, "Student");
            response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Delete User

        [Fact]
        public async Task Delete_User_With_Role_SuperAdmin_Return_OkNoContent()
        {
            var fakeContext = new FakeFunctionContext("1", "SuperAdmin", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.deleteUser(request, fakeContext, 1);
            response.StatusCode.ShouldEqual(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Delete_User_With_Role_Admin_Return_OkNoContent()
        {
            var fakeContext = new FakeFunctionContext("1", "Admin", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.deleteUser(request, fakeContext, 1);
            response.StatusCode.ShouldEqual(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Delete_User_With_Role_Instructor_Return_Unauthorized()
        {
            var fakeContext = new FakeFunctionContext("10", "Instructor", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.deleteUser(request, fakeContext, 1);
            response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Delete_User_With_Role_Student_Return_Unauthorized()
        {
            var fakeContext = new FakeFunctionContext("10", "Student", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.deleteUser(request, fakeContext, testStudentUser.Id);
            response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Delete_Last_Admin_In_Organization_With_Role_SuperAdmin_Return_BadRequest()
        {
            
            var fakeContext = new FakeFunctionContext("1", "SuperAdmin", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.deleteUser(request, fakeContext, testAdminUser.Id);
            response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Delete_Second_Admin_In_Organization_With_Role_SuperAdmin_Return_OkNoContent()
        {
            User secondAdmin = new User { Email = "Admin@gmail.com", FirstName = "TestAdminFirstName", LastName = "TestAdminLastName", Password = "test", ActivationCode = "abcdefgh", Organization = organization, Trainings = new List<Training>(), UserType = Role.Admin };
            UserService.CreateUser(secondAdmin);
            var fakeContext = new FakeFunctionContext("1", "SuperAdmin", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await UserController.deleteUser(request, fakeContext, secondAdmin.Id);
            response.StatusCode.ShouldEqual(HttpStatusCode.NoContent);
        }

        #endregion

    }
}
