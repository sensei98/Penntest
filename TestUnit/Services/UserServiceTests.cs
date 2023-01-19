using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestSupport.EfHelpers;
using VRefSolutions.DAL;
using VRefSolutions.Domain.Entities;
using VRefSolutions.Repository;
using VRefSolutions.Service;
using Xunit.Extensions.AssertExtensions;
using VRefSolutions.Domain.Enums;
using VRefSolutions.Domain.DTO;

namespace TestUnit.Services
{
    public class UserServiceTests
    {
        private VRefSolutionsContext Context;
        private UserService userService;
        private OrganizationService organizationService;
        public UserServiceTests()
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
        public void Created_Users_Are_Of_Same_Organization()
        {
            Organization organization = organizationService.GetOrganizationById(1);
            var users = userService.getUsersBySearch("");
            Assert.Equal(organization, users[0].Organization);
        }

        [Fact]
        public void Activate_Student()
        {
            User userBeforeActivating = userService.GetUserByActivationCode("ABCDEFGH");
            userBeforeActivating.ActivationCode = "";
            userService.UpdateUser(userBeforeActivating);
            User userAfterUpdate = userService.GetUserById(1);
            Assert.Equal(userBeforeActivating.ActivationCode, userAfterUpdate.ActivationCode);
        }

        [Fact]
        public void Get_User_By_Credentials()
        {
            User userStudent = userService.GetUserByCredentials("EmailStudentTest", "StudentTest");
            Assert.Equal(userStudent.FirstName, "FirstNameStudentTest");
        }

        [Fact]
        public void Get_User_By_Email()
        {
            User userStudent = userService.GetUserByEmail("EmailStudentTest");
            Assert.Equal(userStudent.FirstName, "FirstNameStudentTest");
        }
        [Fact]
        public void Get_User_By_Id()
        {
            User userStudent = userService.GetUserById(1);
            Assert.Equal(userStudent.FirstName, "FirstNameStudentTest");
        }
        [Fact]
        public void get_Users_By_Search()
        {
            List<User> users = userService.getUsersBySearch("EmailStudentTest");
            Assert.Equal(users[0].FirstName, "FirstNameStudentTest");
        }

        [Fact]
        public void user_Exists_By_Email()
        {
            bool exists = userService.userExistsByEmail("EmailStudentTest");
            Assert.Equal(exists, true);
        }

        [Fact]
        public void Update_User()
        {
            User userStudent = userService.GetUserById(1);
            userStudent.FirstName = "newStudentFirstName";
            userService.UpdateUser(userStudent);
            User studentRetrievedAfterUpdate = userService.GetUserById(1);
            Assert.Equal(studentRetrievedAfterUpdate.FirstName, "newStudentFirstName");
        }

        [Fact]
        public void Hash_Password_And_Verify_Password_Hash()
        {
            User userStudent = userService.GetUserById(1);
            string passwordInputField = userStudent.Password;
            userStudent.Password = userService.HashPassword(userStudent.Password);
            userService.UpdateUser(userStudent);
            User studentRetrievedAfterUpdate = userService.GetUserById(1);
            bool checkIfPasswordIsCorrect = userService.verifyPasswordHash(passwordInputField, studentRetrievedAfterUpdate.Password);
            Assert.Equal(checkIfPasswordIsCorrect, true);
        }

        [Fact]
        public void Is_Object_Of_SuperAdmin()
        {
            bool superAdmin = userService.IsObjectOfSuperAdmin(4);
            Assert.Equal(superAdmin, true);
        }

        [Fact]
        public void Is_Object_OfAdmin()
        {
            bool admin = userService.IsObjectOfAdmin(3);
            Assert.Equal(admin, true);
        }

        [Fact]
        public void Get_Changed_Properties()
        {
            OrganizationDTO organization = new OrganizationDTO(1);
            UserUpdateDTO A = new UserUpdateDTO { Email = "A", FirstName = "A", LastName = "A", Organization = organization, UserType = Role.Student };
            UserUpdateDTO B = new UserUpdateDTO { Email = "B", FirstName = "A", LastName = "A", Organization = organization, UserType = Role.Student };
            List<string> changes = userService.GetChangedProperties<UserUpdateDTO>(A, B);
            Assert.Equal(changes.Count, 1);
        }

        [Fact]
        public void delete_User()
        {
            User userStudent = userService.GetUserById(1);
            userService.delete(userStudent);
            List<User> users = userService.getUsersBySearch(null);
            Assert.Equal(users.Count, 3);
        }

        [Fact]
        public void Truncate()
        {
            string longstringToTruncate = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string truncatedString = userService.Truncate(longstringToTruncate, 8);
            Assert.Equal(truncatedString, "ABCDEFGH");
        }

    }

}
