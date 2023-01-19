using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestSupport.EfHelpers;
using VRefSolutions.DAL;
using VRefSolutions.Domain.DTO;
using VRefSolutions.Domain.Entities;
using VRefSolutions.Domain.Enums;
using VRefSolutions.Repository;
using VRefSolutions.Service;

namespace TestUnit.Services
{

    public class OrganizationServiceTests
    {
        private VRefSolutionsContext Context;
        private OrganizationService organizationService;
        private UserService userService;
        public OrganizationServiceTests()
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
            User Instructor = new User { Email = "EmailInstructorTest", FirstName = "FirstNameInstructorTest", LastName = "LastNameInstructorTest", Password = "InstructorTest", UserType = Role.Instructor, Organization = testOrganization, ActivationCode = "", Trainings = new List<Training>() };
            Context.Users.Add(Instructor);
            User Admin = new User { Email = "EmailAdminTest", FirstName = "FirstNameAdminTest", LastName = "LastNameAdminTest", Password = "AdminTest", UserType = Role.Admin, Organization = testOrganization, ActivationCode = "", Trainings = new List<Training>() };
            Context.Users.Add(Admin);
            User SuperAdmin = new User { Email = "EmailSuperAdminTest", FirstName = "FirstNameSuperAdminTest", LastName = "LastNameSuperAdminTest", Password = "SuperAdminTest", UserType = Role.SuperAdmin, Organization = testOrganization, ActivationCode = "", Trainings = new List<Training>() };
            Context.Users.Add(SuperAdmin);
            Context.SaveChanges();

            userService = new UserService(new UserRepository(Context));
            organizationService = new OrganizationService(new OrganizationRepository(Context));
        }


        [Fact]
        public void CheckIfOrganizationNameExists()
        {
            Organization organization = new Organization { Name = "Test Org 1", Users = new List<User>() };
            bool exists = organizationService.CheckIfOrganizationNameExists(organization.Name);
            Assert.Equal(exists, true);
        }

        [Fact]
        public void CreateOrganization()
        {
            Organization organization = new Organization { Name = "Test Org 1", Users = new List<User>() };
            organizationService.CreateOrganization(organization);
            List<Organization> organizations = organizationService.GetAllOrganizations();
            Assert.Equal(organizations.Count, 2);
        }

        [Fact]
        public void deleteOrganization()
        {
            Organization organization = organizationService.GetOrganizationById(1);
            organizationService.delete(organization);
            List<Organization> organizations = organizationService.GetAllOrganizations();
            Assert.Equal(organizations.Count, 0);
        }

        [Fact]
        public void GetAllOrganizations()
        {
            List<Organization> organizations = organizationService.GetAllOrganizations();
            Assert.Equal(organizations.Count, 1);
        }
        [Fact]
        public void GetOrganizationById()
        {
            Organization organization = organizationService.GetOrganizationById(1);
            Assert.Equal(organization.Id, 1);
        }
        [Fact]
        public void updateOrganization()
        {
            Organization organizationBeforeUpdate = organizationService.GetOrganizationById(1);
            organizationBeforeUpdate.Name = "New Org 1 Name";
            organizationService.updateOrganization(organizationBeforeUpdate);
            Organization organizationAfterUpdate = organizationService.GetOrganizationById(1);
            Assert.Equal(organizationAfterUpdate.Name, "New Org 1 Name");
        }

    }

}
