using System.Net;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Company.VRefSolutions.Controller;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Service;
using VRefSoltutions.Profiles;
using VRefSolutions.DAL;
using VRefSolutions.Domain.DTO;
using VRefSolutions.Domain.Entities;
using VRefSolutions.Repository;
using VRefSolutions.Repository.Interfaces;
using VRefSolutions.Service;
using VRefSolutions.Service.Interfaces;
using Xunit.Extensions.AssertExtensions;
using TestHelper;
using System.ComponentModel;
using VRefSolutions.Domain.Enums;
using VRefSolutions.Domain.Models;
using Xunit.Abstractions;

namespace TestUnit;

public class TrainingControllerTests : ControllerTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private TrainingController TrainingController;
    private OrganizationService OrganizationService;
    private UserService UserService;

    public TrainingControllerTests(ITestOutputHelper testOutputHelper) //setup 
    {
        _testOutputHelper = testOutputHelper;
        TrainingController = new TrainingController(host.Services.GetRequiredService<ILogger<TrainingController>>(),
                                                    host.Services.GetRequiredService<IMapper>(),
                                                    host.Services.GetRequiredService<ITrainingService>(),
                                                    host.Services.GetRequiredService<IUserService>(),
                                                    host.Services.GetRequiredService<IEventService>(),
                                                    host.Services.GetRequiredService<IEventTypeService>(),
                                                    host.Services.GetRequiredService<IAltitudeService>());

        OrganizationService = new OrganizationService(host.Services.GetRequiredService<IOrganizationRepository>());
        UserService = new UserService(host.Services.GetRequiredService<IUserRepository>());
        // Setup all your database entities. 
        Organization organization1 = new Organization { Name = "The Testing Organization" };
        OrganizationService.CreateOrganization(organization1);
        organization1.ShouldNotBeNull();
        organization1.Id.ShouldEqual(1);
        Organization organization2 = new Organization { Name = "The Other Testing Organization" };
        // Setup users for both organizations
        #region Org 1 users
        User org1User1Student = UserService.CreateUser(new User
        {
            Email = "630457@student.klm.nl",
            FirstName = "Tim",
            LastName = "Kierkegaard",
            Organization = organization1,
            UserType = Role.Student,
        });
        User org1User2Student = UserService.CreateUser(new User
        {
            Email = "705145@student.klm.nl",
            FirstName = "Pim",
            LastName = "Anderegaard",
            Organization = organization1,
            UserType = Role.Student,
        });
        User org1User3Student = UserService.CreateUser(new User
        {
            Email = "629547@student.klm.nl",
            FirstName = "Jeremy",
            LastName = "de Groot",
            Organization = organization1,
            UserType = Role.Student,
        });
        User org1User4Instructor = UserService.CreateUser(new User
        {
            Email = "ronald.dejong@klm.nl",
            FirstName = "Ronald",
            LastName = "de Jong",
            Organization = organization1,
            UserType = Role.Instructor,
        });
        User org1User5Instructor = UserService.CreateUser(new User
        {
            Email = "mark.paulusma@klm.nl",
            FirstName = "Mark",
            LastName = "Paulusma",
            Organization = organization1,
            UserType = Role.Instructor,
        });
        User org1User6Admin = UserService.CreateUser(new User
        {
            Email = "martijn.boomgaard@klm.nl",
            FirstName = "Martijn",
            LastName = "Boomgaard",
            Organization = organization1,
            UserType = Role.Admin,
        });
        #endregion
        #region Org 2 users
        User org2User7Student = UserService.CreateUser(new User
        {
            Email = "632457@student.klm.nl",
            FirstName = "Jim",
            LastName = "Hierkegaard",
            Organization = organization2,
            UserType = Role.Student,
        });
        User org2User8Student = UserService.CreateUser(new User
        {
            Email = "702457@student.klm.nl",
            FirstName = "Sam",
            LastName = "Banderegaard",
            Organization = organization2,
            UserType = Role.Student,
        });
        User org2User9Student = UserService.CreateUser(new User
        {
            Email = "629667@student.klm.nl",
            FirstName = "Beremy",
            LastName = "de Goot",
            Organization = organization2,
            UserType = Role.Student,
        });
        User org2User10Instructor = UserService.CreateUser(new User
        {
            Email = "donald.debom@klm.nl",
            FirstName = "Donald",
            LastName = "de Bom",
            Organization = organization2,
            UserType = Role.Instructor,
        });
        User org2User11Instructor = UserService.CreateUser(new User
        {
            Email = "markus.paulussen@klm.nl",
            FirstName = "Markus",
            LastName = "Paulussen",
            Organization = organization2,
            UserType = Role.Instructor,
        });
        User org2User12Admin = UserService.CreateUser(new User
        {
            Email = "martein.boomgaard@klm.nl",
            FirstName = "Martein",
            LastName = "Boomgaard",
            Organization = organization2,
            UserType = Role.Admin,
        });
        #endregion
        TrainingController.ShouldNotBeNull();

    }

    [Fact]
    public async Task training_Controller_Methods_Called_By_Anonymous_Should_Return_Forbidden()
    {

        var fakeContext = new FakeFunctionContext("", "", host);
        var body = new MemoryStream(Encoding.Default.GetBytes("")); // body does not matter
        var request = new FakeHttpRequestData(fakeContext,
                                              new Uri("https://dummy-url.com/api/training/"),
                                              body);
        var response = await TrainingController.createTraining(request, fakeContext);
        response.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
        response = await TrainingController.deleteTraining(request, 1, fakeContext);
        response.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
        response = await TrainingController.updateTraining(request, 1, fakeContext);
        response.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
        response = await TrainingController.getTraining(request, 1, fakeContext);
        response.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
        response = await TrainingController.getTrainings(request, fakeContext);
        response.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
    }

    #region createTraining [POST] /training
    [Fact]
    public async Task createTraining_Should_Return_Training_With_Correct_Students_and_Instructor()
    {
        var fakeContext = new FakeFunctionContext("4", "Instructor", host);
        var students = new List<int>() { 1, 2 };

        var response = await CreateTraining(fakeContext, students);
        response.StatusCode.ShouldEqual(HttpStatusCode.Created);
        response.Body.Position = 0;
        var reader = new StreamReader(response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var training = GetDeserializedJsonObject<TrainingResponseDTO>(responseBody);
        training.Id.ShouldEqual(1);
        training.Students.ForEach(s => {
            s.Id.ShouldNotBeNull();
            s.FirstName.ShouldNotBeNull();
            s.LastName.ShouldNotBeNull();
            s.Organization.Name.ShouldNotBeNull();
            s.Email.ShouldNotBeNull();
            s.UserType.ShouldNotBeNull();
        });
        //training.Students.ShouldEqual(new List<int>() { 1, 2 });
        training.Instructor.Id.ShouldEqual(4);
    }
    [Fact]
    public async Task createTraining_With_Instructor_As_Participant_Should_Return_BadRequest()
    {
        var fakeContext = new FakeFunctionContext("4", "Instructor", host);
        var students = new List<int>() { 1, 4 };
        var response = await CreateTraining(fakeContext, students);
        response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
        response.Body.Position = 0;
        var reader = new StreamReader(response.Body);
        var responseBody = await reader.ReadToEndAsync();
        responseBody.ShouldEqual("You can only add Students to the list of Participants");
    }
    [Fact]
    public async Task createTraining_Called_By_Student_Should_Return_Unauthorized()
    {
        var fakeContext = new FakeFunctionContext("1", "Student", host);
        var students = new List<int>() { 2, 3 };

        var response = await CreateTraining(fakeContext, students);
        response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
    }
    [Fact]
    public async Task createTraining_with_Invalid_DTO_Should_Return_BadRequest()
    {
        var fakeContext = new FakeFunctionContext("4", "Instructor", host);
        var students = new List<int>() { 2, 3, 4 };

        var response = await CreateTraining(fakeContext, students);
        response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
    }
    #endregion
    #region updateTraining [PUT] /training/{TrainingID}
    [Fact]
    public async Task updateTraining_Should_Return_Updated_Training()
    {

        // Create a training
        var fakeContext = new FakeFunctionContext("4", "Instructor", host);
        var students = new List<int>() { 1, 2 };
        var response = await CreateTraining(fakeContext, students);
        response.StatusCode.ShouldEqual(HttpStatusCode.Created);
        response.Body.Position = 0;
        var reader = new StreamReader(response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var oldTraining = GetDeserializedJsonObject<TrainingResponseDTO>(responseBody);

        // Update Training
        var json = JsonConvert.SerializeObject(
            new TrainingRequestDTO
            {
                Students = new List<int>() { 1, 3 }
            }
        );
        var body = new MemoryStream(Encoding.Default.GetBytes(json));
        var request = new FakeHttpRequestData(fakeContext,
                                              new Uri("https://dummy-url.com/api/training/"),
                                              body);
        response = await TrainingController.updateTraining(request, oldTraining.Id, fakeContext);
        response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        response.Body.Position = 0;
        reader = new StreamReader(response.Body);
        responseBody = await reader.ReadToEndAsync();
        var updatedTraining = GetDeserializedJsonObject<TrainingResponseDTO>(responseBody);
        updatedTraining.Students.ShouldNotEqual(oldTraining.Students);

    }
    [Fact]
    public async Task updateTraining_As_Student_Should_Return_Unauthorized()
    {
        // Create a training
        var fakeContext = new FakeFunctionContext("4", "Instructor", host);

        var students = new List<int>() { 1, 2 };
        var response = await CreateTraining(fakeContext, students);
        response.StatusCode.ShouldEqual(HttpStatusCode.Created);
        response.Body.Position = 0;
        var reader = new StreamReader(response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var oldTraining = GetDeserializedJsonObject<TrainingResponseDTO>(responseBody);

        fakeContext = new FakeFunctionContext("1", "Student", host);
        // Update Training
        var json = JsonConvert.SerializeObject(
            new TrainingRequestDTO
            {
                Students = new List<int>() { 1, 3 }
            }
        );
        var body = new MemoryStream(Encoding.Default.GetBytes(json));
        var request = new FakeHttpRequestData(fakeContext,
                                              new Uri("https://dummy-url.com/api/training/"),
                                              body);
        response = await TrainingController.updateTraining(request, oldTraining.Id, fakeContext);
        response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
    }
    [Fact]
    public async Task updateTraining_As_Instructor_On_Unowned_Training_Should_Return_Unauthorized()
    {
        // Create a training
        var fakeContext = new FakeFunctionContext("4", "Instructor", host);
        var students = new List<int>() { 1, 2 };
        var response = await CreateTraining(fakeContext, students);
        response.StatusCode.ShouldEqual(HttpStatusCode.Created);
        response.Body.Position = 0;
        var reader = new StreamReader(response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var oldTraining = GetDeserializedJsonObject<TrainingResponseDTO>(responseBody);
        // Update Training with another instructor
        fakeContext = new FakeFunctionContext("5", "Instructor", host);

        var json = JsonConvert.SerializeObject(
            new TrainingRequestDTO
            {
                Students = new List<int>() { 1, 3 }
            }
        );
        var body = new MemoryStream(Encoding.Default.GetBytes(json));
        var request = new FakeHttpRequestData(fakeContext,
                                              new Uri("https://dummy-url.com/api/training/"),
                                              body);
        response = await TrainingController.updateTraining(request, oldTraining.Id, fakeContext);
        response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task updateTraining_On_Nonexistent_Training_Should_Return_NotFound()
    {
        var fakeContext = new FakeFunctionContext("4", "Instructor", host);

        string json = JsonConvert.SerializeObject(
            new TrainingRequestDTO
            {
                Students = new List<int>() { 1, 3 }
            }
        );
        var body = new MemoryStream(Encoding.Default.GetBytes(json));
        var request = new FakeHttpRequestData(fakeContext,
                                              new Uri("https://dummy-url.com/api/training/"),
                                              body);
        var response = await TrainingController.updateTraining(request, 999, fakeContext);
        response.StatusCode.ShouldEqual(HttpStatusCode.NotFound);
    }


    #endregion
    #region deleteTraining [DELETE] /training/{TrainingID}
    [Fact]
    public async Task deleteTraining_As_Student_Or_Instructor_Should_Return_Unauthorized()
    {
        var fakeContext = new FakeFunctionContext("4", "Instructor", host);

        var body = new MemoryStream(Encoding.Default.GetBytes(""));
        var request = new FakeHttpRequestData(fakeContext,
                                              new Uri("https://dummy-url.com/api/training/1"),
                                              body);

        // id doesn't matter cause you shouldn't get passed authorization check
        var response = await TrainingController.deleteTraining(request, 1, fakeContext);
        response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);

        fakeContext = new FakeFunctionContext("1", "Student", host);
        response = await TrainingController.deleteTraining(request, 1, fakeContext);
        response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);

    }
    [Fact]
    public async Task deleteTraining_On_Success_Should_Return_NoContent()
    {
        // Create a training
        var fakeContext = new FakeFunctionContext("4", "Instructor", host);
        var students = new List<int>() { 1, 2 };
        var response = await CreateTraining(fakeContext, students);
        response.StatusCode.ShouldEqual(HttpStatusCode.Created);
        response.Body.Position = 0;
        var reader = new StreamReader(response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var training = GetDeserializedJsonObject<TrainingResponseDTO>(responseBody);

        // Delete 
        fakeContext = new FakeFunctionContext("6", "Admin", host);
        var body = new MemoryStream(Encoding.Default.GetBytes(""));
        var request = new FakeHttpRequestData(fakeContext,
                                          new Uri("https://dummy-url.com/api/training/"),
                                          body);
        response = await TrainingController.deleteTraining(request, training.Id, fakeContext);
        response.StatusCode.ShouldEqual(HttpStatusCode.NoContent);
    }
    [Fact]
    public async Task deleteTraining_Of_Another_Organization_Or_Nonexisting_Should_Return_NotFound()
    {
        // Create a training
        var fakeContext = new FakeFunctionContext("4", "Instructor", host);
        var students = new List<int>() { 1, 2 };
        var response = await CreateTraining(fakeContext, students);
        response.StatusCode.ShouldEqual(HttpStatusCode.Created);
        response.Body.Position = 0;
        var reader = new StreamReader(response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var training = GetDeserializedJsonObject<TrainingResponseDTO>(responseBody);

        // Delete 
        fakeContext = new FakeFunctionContext("12", "Admin", host);
        var body = new MemoryStream(Encoding.Default.GetBytes(""));
        var request = new FakeHttpRequestData(fakeContext,
                                          new Uri("https://dummy-url.com/api/training/"),
                                          body);
        response = await TrainingController.deleteTraining(request, training.Id, fakeContext);
        response.StatusCode.ShouldEqual(HttpStatusCode.NotFound);

        // Delete nonexisting training
        response = await TrainingController.deleteTraining(request, 999, fakeContext);
        response.StatusCode.ShouldEqual(HttpStatusCode.NotFound);
    }
    #endregion
    #region getTrainings [GET] /training(s)
    [Fact]
    public async Task getTrainings_Should_Return_All_Trainings_Of_User()
    {
        // Create two trainings
        var fakeContext = new FakeFunctionContext("4", "Instructor", host);
        var students = new List<int>() { 1, 2 };
        var response = await CreateTraining(fakeContext, students);
        response.StatusCode.ShouldEqual(HttpStatusCode.Created);
        response = await CreateTraining(fakeContext, students);
        response.StatusCode.ShouldEqual(HttpStatusCode.Created);
        // GET as Student
        fakeContext = new FakeFunctionContext("1", "Student", host);
        var body = new MemoryStream(Encoding.Default.GetBytes(""));
        var request = new FakeHttpRequestData(
                        fakeContext,
                        new Uri("https://dummy-url.com/api/trainings/"),
                        body);
        response = await TrainingController.getTrainings(request, fakeContext);
        response.StatusCode.ShouldEqual(HttpStatusCode.OK);

        response.Body.Position = 0;
        var reader = new StreamReader(response.Body);
        var responseBody = await reader.ReadToEndAsync();
        TrainingsResponseDTO[] responseDTOs = GetDeserializedJsonObject<TrainingsResponseDTO[]>(responseBody);
        responseDTOs.Count().ShouldEqual(2);
        // GET as Instructor
        fakeContext = new FakeFunctionContext("4", "Instructor", host);
        response = await TrainingController.getTrainings(request, fakeContext);
        response.StatusCode.ShouldEqual(HttpStatusCode.OK);

        response.Body.Position = 0;
        reader = new StreamReader(response.Body);
        responseBody = await reader.ReadToEndAsync();
        responseDTOs = GetDeserializedJsonObject<TrainingsResponseDTO[]>(responseBody);
        responseDTOs.Count().ShouldEqual(2);
        responseDTOs[0].Students.ShouldNotBeEmpty();
    }
    [Fact]
    public async Task getTraining_Should_Return_Specific_Training_Of_User()
    {
        // Create two trainings
        var fakeContext = new FakeFunctionContext("4", "Instructor", host);
        var students = new List<int>() { 1, 2 };
        var response = await CreateTraining(fakeContext, students);
        response.StatusCode.ShouldEqual(HttpStatusCode.Created);
        response = await CreateTraining(fakeContext, students);
        response.StatusCode.ShouldEqual(HttpStatusCode.Created);
        response.Body.Position = 0;
        var reader = new StreamReader(response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var training = GetDeserializedJsonObject<TrainingResponseDTO>(responseBody);

        // Get the training
        fakeContext = new FakeFunctionContext("1", "Student", host);
        var body = new MemoryStream(Encoding.Default.GetBytes(""));
        var request = new FakeHttpRequestData(fakeContext,
                                              new Uri("https://dummy-url.com/api/training/"),
                                              body);
        response = await TrainingController.getTraining(request, training.Id, fakeContext);
        response.StatusCode.ShouldEqual(HttpStatusCode.OK);

        response.Body.Position = 0;
        reader = new StreamReader(response.Body);
        responseBody = await reader.ReadToEndAsync();
        var targetTraining = GetDeserializedJsonObject<TrainingResponseDTO>(responseBody);
        targetTraining.Id.ShouldEqual(training.Id);
        targetTraining.Instructor.Id.ShouldEqual(training.Instructor.Id);
        targetTraining.CreationDateTime.ShouldEqual(training.CreationDateTime);
        targetTraining.Status.ShouldEqual(training.Status);
        targetTraining.Students.ShouldNotBeEmpty();
        targetTraining.Students.ForEach(s => {
            s.Email.ShouldNotBeNull();
        });
        //targetTraining.Students.ShouldEqual(training.Students);
    }
    [Fact]
    public async Task getTraining_Invalid_Should_Return_Specific_Training_Of_User()
    {
        var fakeContext = new FakeFunctionContext("1", "Student", host);
        var body = new MemoryStream(Encoding.Default.GetBytes(""));
        var request = new FakeHttpRequestData(fakeContext,
                                              new Uri("https://dummy-url.com/api/training/"),
                                              body);
        var response = await TrainingController.getTraining(request, 9999, fakeContext);
        response.StatusCode.ShouldEqual(HttpStatusCode.NotFound);
    }
    [Fact]
    public async Task getTraining_As_Admin_Of_Different_Organization_Should_Return_NotFound()
    {
        // create training
        var fakeContext = new FakeFunctionContext("4", "Instructor", host);
        var students = new List<int>() { 1, 2 };
        var response = await CreateTraining(fakeContext, students);
        response.StatusCode.ShouldEqual(HttpStatusCode.Created);
        response.Body.Position = 0;
        var reader = new StreamReader(response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var training = GetDeserializedJsonObject<TrainingResponseDTO>(responseBody);

        fakeContext = new FakeFunctionContext("12", "Admin", host);
        var body = new MemoryStream(Encoding.Default.GetBytes(""));
        var request = new FakeHttpRequestData(fakeContext,
                                              new Uri("https://dummy-url.com/api/training/"),
                                              body);
        response = await TrainingController.getTraining(request, training.Id, fakeContext);
        response.StatusCode.ShouldEqual(HttpStatusCode.NotFound);
    }
    [Fact]
    public async Task getTraining_As_Instructor_Of_Training_Should_Return_Training_Without_Videos()
    {
        // create training
        var fakeContext = new FakeFunctionContext("4", "Instructor", host);
        var students = new List<int>() { 1, 2 };
        var response = await CreateTraining(fakeContext, students);
        response.StatusCode.ShouldEqual(HttpStatusCode.Created);
        response.Body.Position = 0;
        var reader = new StreamReader(response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var training = GetDeserializedJsonObject<TrainingResponseDTO>(responseBody);

        // get training
        var body = new MemoryStream(Encoding.Default.GetBytes(""));
        var request = new FakeHttpRequestData(fakeContext,
                                              new Uri($"https://dummy-url.com/api/training/{training.Id}"),
                                              body);
        response = await TrainingController.getTraining(request, training.Id, fakeContext);
        response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        response.Body.Position = 0;
        reader = new StreamReader(response.Body);
        responseBody = await reader.ReadToEndAsync();
        training = GetDeserializedJsonObject<TrainingResponseDTO>(responseBody);
        training.ShouldNotBeNull();
        training.VideoAccesURLs.ShouldBeEmpty();
    }
    [Fact]
    public async Task getTraining_As_Instructor_On_Unowned_Training_Should_Return_NotFound()
    {
        var fakeContext = new FakeFunctionContext("4", "Instructor", host);
        var students = new List<int>() { 1, 2 };
        var response = await CreateTraining(fakeContext, students);
        response.StatusCode.ShouldEqual(HttpStatusCode.Created);
        response.Body.Position = 0;
        var reader = new StreamReader(response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var training = GetDeserializedJsonObject<TrainingResponseDTO>(responseBody);

        // get training with different instructors
        fakeContext = new FakeFunctionContext("5", "Instructor", host);

        var body = new MemoryStream(Encoding.Default.GetBytes(""));
        var request = new FakeHttpRequestData(fakeContext,
                                              new Uri($"https://dummy-url.com/api/training/{training.Id}"),
                                              body);
        response = await TrainingController.getTraining(request, training.Id, fakeContext);
        response.StatusCode.ShouldEqual(HttpStatusCode.NotFound);

        fakeContext = new FakeFunctionContext("10", "Instructor", host);
        request = new FakeHttpRequestData(fakeContext,
                                              new Uri($"https://dummy-url.com/api/training/{training.Id}"),
                                              body);
        response = await TrainingController.getTraining(request, training.Id, fakeContext);
        response.StatusCode.ShouldEqual(HttpStatusCode.NotFound);


    }


    #endregion
    #region StartTraining [POST] /training/{trainingID}/start
    [Fact]
    public async Task startTraining_Should_Return_NoContent()
    {
        var fakeContext = new FakeFunctionContext("4", "Instructor", host);
        var students = new List<int>() { 1, 2 };
        var response = await CreateTraining(fakeContext, students);
        response.StatusCode.ShouldEqual(HttpStatusCode.Created);
        response.Body.Position = 0;
        var reader = new StreamReader(response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var training = GetDeserializedJsonObject<TrainingResponseDTO>(responseBody);

        List<CameraDTO> cameras = new List<CameraDTO>(){
            new CameraDTO{
                Name= "Cockpit",
                Url="rtsp://192.168.178.1",
                Username = "",
                Password = "",
                CaptureMode = CaptureMode.Video_Audio
            },
            new CameraDTO{
                Name= "Dashboard",
                Url="rtsp://192.168.178.2",
                Username = "",
                Password = "",
                CaptureMode = CaptureMode.Video
            }
        };
        var cockpitDTO = new CockpitDTO { Cameras = cameras };
        string json = JsonConvert.SerializeObject(cockpitDTO);
        var body = new MemoryStream(Encoding.Default.GetBytes(json));
        var request = new FakeHttpRequestData(fakeContext,
                                              new Uri($"https://dummy-url.com/api/training/{training.Id}/start"),
                                              body);
        response = await TrainingController.startTraining(request, training.Id, fakeContext);
        response.StatusCode.ShouldEqual(HttpStatusCode.NoContent);

        // Calling Start again should give MethodNotAllowed
        body = new MemoryStream(Encoding.Default.GetBytes(json));
        request = new FakeHttpRequestData(fakeContext,
                                              new Uri($"https://dummy-url.com/api/training/{training.Id}/start"),
                                              body);
        response = await TrainingController.startTraining(request, training.Id, fakeContext);
        response.StatusCode.ShouldEqual(HttpStatusCode.MethodNotAllowed);

        // Check if status changed to Recording
        response = await TrainingController.getTraining(request, training.Id, fakeContext);
        response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        response.Body.Position = 0;
        reader = new StreamReader(response.Body);
        responseBody = await reader.ReadToEndAsync();
        training = GetDeserializedJsonObject<TrainingResponseDTO>(responseBody);
        training.Status.ShouldEqual(Status.Recording);
    }
    [Fact]
    public async Task startTraining_With_Invalid_DTO_Should_Return_BadRequest()
    {
        var fakeContext = new FakeFunctionContext("4", "Instructor", host);
        var students = new List<int>() { 1, 2 };
        var response = await CreateTraining(fakeContext, students);
        response.StatusCode.ShouldEqual(HttpStatusCode.Created);
        response.Body.Position = 0;
        var reader = new StreamReader(response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var training = GetDeserializedJsonObject<TrainingResponseDTO>(responseBody);
        // Same names
        List<CameraDTO> invalidCameras = new List<CameraDTO>(){
            new CameraDTO{
                Name= "Cockpit",
                Url="rtsp://192.168.178.1",
                Username = "",
                Password = "",
                CaptureMode = CaptureMode.Video_Audio
            },
            new CameraDTO{
                Name= "Cockpit",
                Url="rtsp://192.168.178.2",
                Username = "",
                Password = "",
                CaptureMode = CaptureMode.Video
            }
        };
        var cockpitDTO = new CockpitDTO { Cameras = invalidCameras };
        string json = JsonConvert.SerializeObject(cockpitDTO);
        var body = new MemoryStream(Encoding.Default.GetBytes(json));
        var request = new FakeHttpRequestData(fakeContext,
                                              new Uri($"https://dummy-url.com/api/training/{training.Id}/start"),
                                              body);
        response = await TrainingController.startTraining(request, training.Id, fakeContext);
        response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
    }
    [Fact]
    public async Task startTraining_With_Unowned_Training_Should_Return_BadRequest()
    {
        var fakeContext = new FakeFunctionContext("4", "Instructor", host);
        var students = new List<int>() { 1, 2 };
        var response = await CreateTraining(fakeContext, students);
        response.StatusCode.ShouldEqual(HttpStatusCode.Created);
        response.Body.Position = 0;
        var reader = new StreamReader(response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var training = GetDeserializedJsonObject<TrainingResponseDTO>(responseBody);

        fakeContext = new FakeFunctionContext("5", "Instructor", host);
        List<CameraDTO> cameras = new List<CameraDTO>(){
            new CameraDTO{
                Name= "Cockpit",
                Url="rtsp://192.168.178.1",
                Username = "",
                Password = "",
                CaptureMode = CaptureMode.Video_Audio
            }
        };
        var cockpitDTO = new CockpitDTO { Cameras = cameras };
        string json = JsonConvert.SerializeObject(cockpitDTO);
        var body = new MemoryStream(Encoding.Default.GetBytes(json));
        var request = new FakeHttpRequestData(fakeContext,
                                              new Uri($"https://dummy-url.com/api/training/{training.Id}/start"),
                                              body);
        response = await TrainingController.startTraining(request, training.Id, fakeContext);
        response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
    }
    [Fact]
    public async Task startTraining_With_Nonexisting_Training_Should_Return_NotFound()
    {
        var fakeContext = new FakeFunctionContext("5", "Instructor", host);
        List<CameraDTO> cameras = new List<CameraDTO>(){
            new CameraDTO{
                Name= "Cockpit",
                Url="rtsp://192.168.178.1",
                Username = "",
                Password = "",
                CaptureMode = CaptureMode.Video_Audio
            }
        };
        var cockpitDTO = new CockpitDTO { Cameras = cameras };
        string json = JsonConvert.SerializeObject(cockpitDTO);
        var body = new MemoryStream(Encoding.Default.GetBytes(json));
        var request = new FakeHttpRequestData(fakeContext,
                                              new Uri($"https://dummy-url.com/api/training/999/start"),
                                              body);
        var response = await TrainingController.startTraining(request, 999, fakeContext);
        response.StatusCode.ShouldEqual(HttpStatusCode.NotFound);                              
    }

    #endregion

    #region Events

    [Fact]
    public async Task addEventCustom_Called_By_Student_Should_Return_Unauthorized()
    {
        var fakeContext = new FakeFunctionContext("1", "Student", host);

        var response = await CreateEvent(fakeContext, 999);

        response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task addEventCustom_With_Invalid_Timestamp_DTO_Should_Return_BadRequest()
    {
        var fakeContext = new FakeFunctionContext("4", "Instructor", host);
        var students = new List<int>() { 1, 2 };
        var response = await CreateTraining(fakeContext, students);
        response.StatusCode.ShouldEqual(HttpStatusCode.Created);
        response.Body.Position = 0;
        var reader = new StreamReader(response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var training = GetDeserializedJsonObject<TrainingResponseDTO>(responseBody);
        var eventDTO = new EventRequestDTO {
            Name = "Test name",
            Message = "Test message",
            Symbol = "Test symbol",
            TimeStamp = new TimeStamp(0, 70, 0, 0)
        };
        string json = JsonConvert.SerializeObject(eventDTO);
        var body = new MemoryStream(Encoding.Default.GetBytes(json));
        var request = new FakeHttpRequestData(fakeContext,
                                              new Uri($"https://dummy-url.com/api/training/{training.Id}/event/custom"),
                                              body);

        response = await TrainingController.addEventCustom(request, training.Id, fakeContext);

        response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task addEventCustom_With_Nonexisting_Training_Should_Return_NotFound()
    {
        var fakeContext = new FakeFunctionContext("4", "Instructor", host);

        var response = await CreateEvent(fakeContext, 999);

        response.StatusCode.ShouldEqual(HttpStatusCode.NotFound);
    }

    #endregion


    private async Task<HttpResponseData> CreateTraining(FakeFunctionContext fakeContext, List<int> students)
    {
        // Removes some duplicate code for tests
        string json = JsonConvert.SerializeObject(
            new TrainingRequestDTO
            {
                Students = students
            }
        );
        var body = new MemoryStream(Encoding.Default.GetBytes(json));
        var request = new FakeHttpRequestData(fakeContext,
                                              new Uri("https://dummy-url.com/api/training/"),
                                              body);
        return await TrainingController.createTraining(request, fakeContext);
    }

    private async Task<HttpResponseData> CreateEvent(FakeFunctionContext fakeContext, int trainingId)
    {
        string json = JsonConvert.SerializeObject(
            new EventRequestDTO
            {
                Name = "Test name",
                Message = "Test message",
                Symbol = "Test symbol",
                TimeStamp = new TimeStamp(0, 0, 30, 0)
            }
        );
        var body = new MemoryStream(Encoding.Default.GetBytes(json));
        var request = new FakeHttpRequestData(fakeContext,
                                              new Uri($"https://dummy-url.com/api/training/{trainingId}/event/custom"),
                                              body);
        return await TrainingController.addEventCustom(request, trainingId, fakeContext);
    }
}