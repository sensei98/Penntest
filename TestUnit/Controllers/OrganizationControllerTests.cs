using AutoMapper;
using Company.VRefSolutions.Controller;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRefSolutions.Domain.Entities;
using VRefSolutions.Domain.Enums;
using VRefSolutions.Repository.Interfaces;
using VRefSolutions.Service.Interfaces;
using VRefSolutions.Service;
using Xunit.Extensions.AssertExtensions;
using Newtonsoft.Json;
using System.Net;
using TestHelper;
using VRefSolutions.Domain.DTO;

namespace TestUnit
{
    public class OrganizationControllerTests : ControllerTests
    {
        private OrganizationController OrganizationController;
        private UserService UserService;
        private OrganizationService OrganizationService;
        private Organization organization;
        private User testStudentUser;
        private User testAdminUser;

        public OrganizationControllerTests()
        {
            OrganizationController = new OrganizationController(host.Services.GetRequiredService<ILogger<OrganizationController>>(),
                                                    host.Services.GetRequiredService<IOrganizationService>(),
                                                    host.Services.GetRequiredService<IMapper>(),
                                                    host.Services.GetRequiredService<IUserService>());

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

            organization.ShouldNotBeNull();
            organization.Id.ShouldEqual(1);

            OrganizationController.ShouldNotBeNull();

        }

        #region Create Organization
        [Fact]
        public async Task CreateOrganization_With_SuperAdmin_Should_Return_OK()
        {
            var fakeContext = new FakeFunctionContext("1", "SuperAdmin", host);
            string json = JsonConvert.SerializeObject(
            new OrganizationCreateDTO
            {
                Name = "test Organization 1"
            }
            );
            var body = new MemoryStream(Encoding.Default.GetBytes(json));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await OrganizationController.createOrganization(request, fakeContext);
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CreateOrganization_With_Same_Name_As_SuperAdmin_Should_Return_BadRequest()
        {
            var fakeContext = new FakeFunctionContext("1", "SuperAdmin", host);
            string json = JsonConvert.SerializeObject(
            new OrganizationCreateDTO
            {
                Name = "The Testing Organization"
            }
            );
            var body = new MemoryStream(Encoding.Default.GetBytes(json));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await OrganizationController.createOrganization(request, fakeContext);
            response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateOrganization_Admin_Should_Return_Unauthorized()
        {
            var fakeContext = new FakeFunctionContext("1", "Admin", host);
            string json = JsonConvert.SerializeObject(
            new OrganizationCreateDTO
            {
                Name = "The Testing Organization"
            }
            );
            var body = new MemoryStream(Encoding.Default.GetBytes(json));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await OrganizationController.createOrganization(request, fakeContext);
            response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateOrganization_Instructor_Should_Return_Unauthorized()
        {
            var fakeContext = new FakeFunctionContext("1", "Instructor", host);
            string json = JsonConvert.SerializeObject(
            new OrganizationCreateDTO
            {
                Name = "The Testing Organization"
            }
            );
            var body = new MemoryStream(Encoding.Default.GetBytes(json));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await OrganizationController.createOrganization(request, fakeContext);
            response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateOrganization_Student_Should_Return_Unauthorized()
        {
            var fakeContext = new FakeFunctionContext("1", "Student", host);
            string json = JsonConvert.SerializeObject(
            new OrganizationCreateDTO
            {
                Name = "The Testing Organization"
            }
            );
            var body = new MemoryStream(Encoding.Default.GetBytes(json));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await OrganizationController.createOrganization(request, fakeContext);
            response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Edit Organization

        [Fact]
        public async Task EditOrganization_As_SuperAdmin_Should_Return_Ok()
        {
            var fakeContext = new FakeFunctionContext("1", "SuperAdmin", host);
            string json = JsonConvert.SerializeObject(
            new OrganizationCreateDTO
            {
                Name = "The Testing Organization12"
            }
            );
            var body = new MemoryStream(Encoding.Default.GetBytes(json));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await OrganizationController.editOrganization(request, fakeContext, 1);
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }

        [Fact]
        public async Task EditOrganization_To_Already_Existing_Name_As_SuperAdmin_Should_Return_BadRequest()
        {
            var fakeContext = new FakeFunctionContext("1", "SuperAdmin", host);
            string json = JsonConvert.SerializeObject(
            new OrganizationCreateDTO
            {
                Name = "The Testing Organization"
            }
            );
            var body = new MemoryStream(Encoding.Default.GetBytes(json));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await OrganizationController.editOrganization(request, fakeContext, 1);
            response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task EditOrganization_As_Admin_Should_Return_OK()
        {
            var fakeContext = new FakeFunctionContext("1", "Admin", host);
            string json = JsonConvert.SerializeObject(
            new OrganizationCreateDTO
            {
                Name = "The Testing Organization12"
            }
            );
            var body = new MemoryStream(Encoding.Default.GetBytes(json));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await OrganizationController.editOrganization(request, fakeContext, 1);
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }

        [Fact]
        public async Task EditOrganization_As_Instructor_Should_Return_Unauthorized()
        {
            var fakeContext = new FakeFunctionContext("1", "Instructor", host);
            string json = JsonConvert.SerializeObject(
            new OrganizationCreateDTO
            {
                Name = "The Testing Organization12"
            }
            );
            var body = new MemoryStream(Encoding.Default.GetBytes(json));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await OrganizationController.editOrganization(request, fakeContext, 1);
            response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task EditOrganization_As_Student_Should_Return_Unauthorized()
        {
            var fakeContext = new FakeFunctionContext("1", "Student", host);
            string json = JsonConvert.SerializeObject(
            new OrganizationCreateDTO
            {
                Name = "The Testing Organization12"
            }
            );
            var body = new MemoryStream(Encoding.Default.GetBytes(json));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await OrganizationController.editOrganization(request, fakeContext, 1);
            response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Delete Organization

        [Fact]
        public async Task DeleteOrganization_As_SuperAdmin_Should_Return_OKNoContent()
        {
            var fakeContext = new FakeFunctionContext("1", "SuperAdmin", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await OrganizationController.deleteOrganization(request, fakeContext, 1);
            response.StatusCode.ShouldEqual(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeleteOrganization_Which_Doesnt_Exist_As_SuperAdmin_Should_Return_BadRequest()
        {
            var fakeContext = new FakeFunctionContext("1", "SuperAdmin", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await OrganizationController.deleteOrganization(request, fakeContext, 2);
            response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task DeleteOrganization_As_Admin_Should_Return_Unauthorized()
        {
            var fakeContext = new FakeFunctionContext("1", "Admin", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await OrganizationController.deleteOrganization(request, fakeContext, 1);
            response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
        }


        [Fact]
        public async Task DeleteOrganization_As_Instructor_Should_Return_Unauthorized()
        {
            var fakeContext = new FakeFunctionContext("1", "Instructor", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await OrganizationController.deleteOrganization(request, fakeContext, 1);
            response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteOrganization_As_Student_Should_Return_Unauthorized()
        {
            var fakeContext = new FakeFunctionContext("1", "Student", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await OrganizationController.deleteOrganization(request, fakeContext, 1);
            response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Get One Organization

        [Fact]
        public async Task GetOrganization_With_SuperAdmin_Should_Return_OK()
        {
            var fakeContext = new FakeFunctionContext("1", "SuperAdmin", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await OrganizationController.getOrganization(request, fakeContext, 1);
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetOrganization_With_Admin_Should_Return_OK()
        {
            var fakeContext = new FakeFunctionContext("1", "Admin", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await OrganizationController.getOrganization(request, fakeContext, 1);
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetOrganization_With_Instructor_Should_Return_Unauthorized()
        {
            var fakeContext = new FakeFunctionContext("1", "Instructor", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await OrganizationController.getOrganization(request, fakeContext, 1);
            response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetOrganization_With_Student_Should_Return_Unauthorized()
        {
            var fakeContext = new FakeFunctionContext("1", "Student", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await OrganizationController.getOrganization(request, fakeContext, 1);
            response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Get Multiple Organizations

        [Fact]
        public async Task GetAllOrganizations_With_SuperAdmin_Should_Return_OK()
        {
            var fakeContext = new FakeFunctionContext("1", "SuperAdmin", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await OrganizationController.getAllOrganizations(request, fakeContext);
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAllOrganizations_With_Admin_Should_Return_Unauthorized()
        {
            var fakeContext = new FakeFunctionContext("1", "Admin", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await OrganizationController.getAllOrganizations(request, fakeContext);
            response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAllOrganizations_With_Instructor_Should_Return_Unauthorized()
        {
            var fakeContext = new FakeFunctionContext("1", "Instructor", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await OrganizationController.getAllOrganizations(request, fakeContext);
            response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
        }


        [Fact]
        public async Task GetAllOrganizations_With_Student_Should_Return_Unauthorized()
        {
            var fakeContext = new FakeFunctionContext("1", "Student", host);
            var body = new MemoryStream(Encoding.Default.GetBytes(""));
            var request = new FakeHttpRequestData(fakeContext,
                                                  new Uri("https://dummy-url.com/api/user/"),
                                                  body);
            var response = await OrganizationController.getAllOrganizations(request, fakeContext);
            response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
        }


        #endregion

    }
}
