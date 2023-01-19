using System.Linq;
using System.Net;
using VRefSolutions.Domain.DTO;
using VRefSolutions.Domain.Enums;
using VRefSolutions.Domain.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using VRefSolutions.Service.Interfaces;
using AutoMapper;
using Service;
using System.Collections.Generic;
using System.Security.Claims;
using VRefSolutions.Repository;
using VRefSolutions.Domain.Models;

namespace Company.VRefSolutions.Controller
{
    public class UserController : BaseController
    {
        private ILogger<UserController> Logger { get; }
        private IUserService UserService { get; }
        private IOrganizationService OrganizationService { get; }
        private IMapper Mapper { get; }
        private ITokenService TokenService { get; }
        private IEmailService EmailService { get; }

        public UserController(ILogger<UserController> logger, IUserService userService, IOrganizationService organizationService, ITokenService tokenService, IEmailService emailService, IMapper mapper)
        {
            Logger = logger;
            UserService = userService;
            OrganizationService = organizationService;
            Mapper = mapper;
            TokenService = tokenService;
            EmailService = emailService;
        }

        [Auth]
        [Function("getUser")]
        [OpenApiOperation(operationId: "getUser", tags: new[] { "user" }, Summary = "Get a user by user ID")]
        [OpenApiParameter(name: "userId", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The user id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(UserResponseDTO), Description = "The user has been retrieved successfully")]
        public async Task<HttpResponseData> getUserAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/{userId}")] HttpRequestData req, FunctionContext executionContext, int userId)
        {
            #region Authentication/Authentication Checks
            UserIdentityResult identityResult = GetUserIdentityResult(executionContext, req);
            if (identityResult.ResponseMessage is not null)
                return identityResult.ResponseMessage;
            User loggedInUser = UserService.GetUserById(identityResult.UserId);
            User targetUser = UserService.GetUserById(userId);
            if (object.ReferenceEquals(null, targetUser))
            {
                HttpResponseData notFound = req.CreateResponse(HttpStatusCode.NotFound);
                return notFound;
            }

            if (UserService.IsObjectOfSuperAdmin(userId) && loggedInUser.UserType != Role.SuperAdmin)
            {
                HttpResponseData forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                return forbidden;
            }

            if (loggedInUser.UserType == Role.Instructor)
            {
                if (targetUser.Organization.Id != loggedInUser.Organization.Id || targetUser.UserType > loggedInUser.UserType)
                {
                    HttpResponseData forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                    return forbidden;
                }
            }

            if (loggedInUser.UserType == Role.Student)
            {
                if (targetUser.Id != loggedInUser.Id)
                {
                    HttpResponseData forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                    return forbidden;
                }
            }

            #endregion

            UserResponseDTO userResponseDTO = Mapper.Map<UserResponseDTO>(targetUser);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(userResponseDTO);

            return response;
        }

        [Auth]
        [Function("getUsers")]
        [OpenApiOperation(operationId: "getUsers", tags: new[] { "user" }, Summary = "Get all users")]
        [OpenApiParameter(name: "searchField", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The searchField looping through email, name and lastname")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(UserResponseDTO[]), Description = "The users have been retrieved successfully")]
        public async Task<HttpResponseData> getUsers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user")] HttpRequestData req, FunctionContext executionContext, string? searchField = null)
        {
            #region Authentication/Authorization Checks
            UserIdentityResult identityResult = GetUserIdentityResult(executionContext, req, new Role[3] {Role.Instructor, Role.Admin, Role.SuperAdmin });
            if (identityResult.ResponseMessage is not null)
                return identityResult.ResponseMessage;
            #endregion

            #region Get users
            List<User> users = UserService.getUsersBySearch(searchField);
            List<UserResponseDTO> userListToResponse = new List<UserResponseDTO>();

            if (identityResult.Role == Role.Admin || identityResult.Role == Role.SuperAdmin)
            {
                foreach (User u in users)
                {
                    UserResponseDTO userResponseDTO = Mapper.Map<UserResponseDTO>(u);
                    userListToResponse.Add(userResponseDTO);
                }
            }
            else
            {
                User user = UserService.GetUserById(identityResult.UserId);

                foreach (User u in users)
                {
                    if (user.Organization.Id == u.Organization.Id)
                    {
                        UserResponseDTO userResponseDTO = Mapper.Map<UserResponseDTO>(u);
                        userListToResponse.Add(userResponseDTO);
                    }
                }
            }
            #endregion

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(userListToResponse);

            return response;
        }

        [Auth]
        [Function("updateUser")]
        [OpenApiOperation(operationId: "updateUser", tags: new[] { "user" }, Summary = "Update a user")]
        [OpenApiParameter(name: "userId", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The user id")]
        [OpenApiRequestBody("application/json", typeof(UserUpdateDTO), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(UserUpdateDTO), Description = "The user has been updated successfully")]
        public async Task<HttpResponseData> updateUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "user/{userId}")] HttpRequestData req, FunctionContext executionContext, int userId)
        {
            #region Authentication/Authorization Checks
            UserIdentityResult identityResult = GetUserIdentityResult(executionContext, req);
            if (identityResult.ResponseMessage is not null)
                return identityResult.ResponseMessage;

            User loggedInUser = UserService.GetUserById(identityResult.UserId);
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            UserUpdateDTO userUpdateFromRequestBody = JsonConvert.DeserializeObject<UserUpdateDTO>(requestBody);
            User userFromUserIdSearchField = UserService.GetUserById(userId);

            if (identityResult.Role != Role.SuperAdmin)
            {
                if (identityResult.Role == Role.Student && !UserService.checkIfUserIsSameForRequestedId(identityResult.UserId, userId) || identityResult.Role == Role.Instructor && !UserService.checkIfUserIsSameForRequestedId(identityResult.UserId, userId) || identityResult.Role == Role.Admin && loggedInUser.Organization.Id != userFromUserIdSearchField.Organization.Id)
                {
                    HttpResponseData unauthortized = req.CreateResponse(HttpStatusCode.Unauthorized);
                    return unauthortized;
                }
                if (UserService.IsObjectOfSuperAdmin(userId))
                {
                    //not allowed to change superadmin values if your not a superadmin yourself.
                    HttpResponseData unauthortized = req.CreateResponse(HttpStatusCode.Unauthorized);
                    return unauthortized;
                }
            }
            #endregion

            #region Restrictions
            if (identityResult.Role != Role.SuperAdmin)
            {
                #region student and instructor restrictions
                UserUpdateDTO userUpdateDTOFromRequestBody = Mapper.Map<UserUpdateDTO>(userUpdateFromRequestBody);
                UserUpdateDTO userUpdateDTOFromDB = Mapper.Map<UserUpdateDTO>(userFromUserIdSearchField);
                string notAllowed = "";

                if (identityResult.Role == Role.Student || identityResult.Role == Role.Instructor)
                {
                    List<string> changes = UserService.GetChangedProperties<UserUpdateDTO>(userUpdateDTOFromRequestBody, userUpdateDTOFromDB);
                    if (changes.Contains("Email"))
                    {
                        notAllowed = notAllowed + "Email can only be changed by SuperAdmins";
                    }
                    if (changes.Contains("UserType"))
                    {
                        notAllowed = notAllowed + "," + Environment.NewLine + "UserType can only be changed by SuperAdmins";
                    }
                    if (changes.Contains("OrganizationId"))
                    {
                        notAllowed = notAllowed + "," + Environment.NewLine + "Organization can only be changed by SuperAdmins";
                    }
                }
                #endregion

                #region Admin restrictions
                else if (identityResult.Role == Role.Admin)
                {
                    List<string> changes = UserService.GetChangedProperties<UserUpdateDTO>(userUpdateDTOFromRequestBody, userUpdateDTOFromDB);
                    if (changes.Contains("Email"))
                    {
                        notAllowed = notAllowed + "Email can only be changed by SuperAdmins";
                    }
                    if (userUpdateDTOFromRequestBody.UserType == Role.SuperAdmin)
                    {
                        notAllowed = notAllowed + "," + Environment.NewLine + "Superadmins can only be created by other Superadmins";
                    }
                    if (changes.Contains("OrganizationId"))
                    {
                        if (userFromUserIdSearchField.UserType != Role.Student && userFromUserIdSearchField.UserType != Role.Instructor)
                        {
                            notAllowed = notAllowed + "," + Environment.NewLine + "Organization for admins can only be changed by SuperAdmins";
                        }

                    }

                }

                if (!String.IsNullOrEmpty(notAllowed))
                {
                    HttpResponseData unauthortized = req.CreateResponse(HttpStatusCode.Unauthorized);
                    return unauthortized;
                }
                #endregion
            }
            #endregion

            #region update user object
            Organization organization = OrganizationService.GetOrganizationById(userUpdateFromRequestBody.Organization.Id);
            if (!userFromUserIdSearchField.Email.Equals(userUpdateFromRequestBody.Email))
            {

                if (UserService.userExistsByEmail(userUpdateFromRequestBody.Email))
                {
                    HttpResponseData conflict = req.CreateResponse(HttpStatusCode.Conflict);
                    return conflict;
                }
            }
            if (object.ReferenceEquals(null, organization))
            {
                HttpResponseData conflict = req.CreateResponse(HttpStatusCode.Conflict);
                return conflict;
            }
            OrganizationsDTO organizationsDTO = Mapper.Map<OrganizationsDTO>(organization);
            userFromUserIdSearchField.Email = userUpdateFromRequestBody.Email;
            userFromUserIdSearchField.FirstName = userUpdateFromRequestBody.FirstName;
            userFromUserIdSearchField.LastName = userUpdateFromRequestBody.LastName;
            userFromUserIdSearchField.UserType = userUpdateFromRequestBody.UserType;
            userFromUserIdSearchField.Organization = organization;

            UserService.UpdateUser(userFromUserIdSearchField);
            #endregion

            UserResponseDTO userResponseDTOResult = new UserResponseDTO(
                userFromUserIdSearchField.Id, userUpdateFromRequestBody.Email, userUpdateFromRequestBody.FirstName, userUpdateFromRequestBody.LastName, userUpdateFromRequestBody.UserType, organizationsDTO);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(userResponseDTOResult);

            return response;
        }

        [Auth]
        [Function("deleteUser")]
        [OpenApiOperation(operationId: "deleteUser", tags: new[] { "user" }, Summary = "Delete a user")]
        [OpenApiParameter(name: "userId", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The user id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The user has been deleted successfully")]
        public async Task<HttpResponseData> deleteUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "user/{userId}")] HttpRequestData req, FunctionContext executionContext, int userId)
        {
            #region Authentication/Authorization Checks
            UserIdentityResult identityResult = GetUserIdentityResult(executionContext, req, new Role[2] { Role.Admin, Role.SuperAdmin });
            if (identityResult.ResponseMessage is not null)
                return identityResult.ResponseMessage;

            //get user and check if it exists
            User user = UserService.GetUserById(userId);
            if (object.ReferenceEquals(null, user))
            {
                HttpResponseData badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                return badRequest;
            }

            // check when admin deleting an account is in same organization
            User adminOrSuperAdmin = UserService.GetUserById(identityResult.UserId);
            if (adminOrSuperAdmin.UserType == Role.Admin)
            {
                if (user.Organization.Id != adminOrSuperAdmin.Organization.Id)
                {
                    HttpResponseData badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                    return badRequest;
                }
            }

            //get organization and check if there are more than 1 admins (atleast one admin has to exists)
            Organization organization = OrganizationService.GetOrganizationById(user.Organization.Id);
            int adminCounts = 0;
            if (UserService.IsObjectOfAdmin(userId))
            {
                foreach (User o in organization.Users)
                {
                    if (o.UserType.Equals(Role.Admin))
                    {
                        adminCounts++;
                    }
                }
                if (adminCounts <= 1)
                {
                    HttpResponseData badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                    return badRequest;
                }
            }
            else if(UserService.IsObjectOfSuperAdmin(userId))
            {
                foreach (User o in organization.Users)
                {
                    if (o.UserType.Equals(Role.SuperAdmin))
                    {
                        adminCounts++;
                    }
                }
                if (adminCounts <= 1)
                {
                    HttpResponseData badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                    return badRequest;
                }
            }

            UserService.delete(user);
            #endregion

            var response = req.CreateResponse(HttpStatusCode.NoContent);
            return response;
        }

        [Function("activateUser")]
        [OpenApiOperation(operationId: "activateUser", tags: new[] { "user" }, Summary = "Activate a new account with a password and other details")]
        [OpenApiRequestBody("application/json", typeof(UserActivateDTO), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(UserActivateDTO), Description = "The user has been activated successfully")]
        public async Task<HttpResponseData> activateUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "user/activate")] HttpRequestData req, FunctionContext executionContext, string activationCode)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            UserActivateDTO userDTO = JsonConvert.DeserializeObject<UserActivateDTO>(requestBody);

            if (!string.IsNullOrEmpty(userDTO.ActivationCode))
            {
                User userToActivate = UserService.GetUserByActivationCode(activationCode);
                if (userToActivate == null)
                {
                    var response = req.CreateResponse(HttpStatusCode.NotFound);
                    return response;
                }
                else
                {
                    string hashedpassword = UserService.HashPassword(userDTO.Password);
                    userToActivate.Password = hashedpassword;
                    userToActivate.ActivationCode = String.Empty;
                    UserService.UpdateUser(userToActivate);
                    var response = req.CreateResponse(HttpStatusCode.OK);
                    await response.WriteAsJsonAsync(Mapper.Map<UserResponseDTO>(userToActivate));
                    return response;
                }
            }
            else
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                return response;
            }
        }

        [Auth]
        [Function("createUser")]
        [OpenApiOperation(operationId: "createUser", tags: new[] { "user" }, Summary = "Create a new unactivated user with given email", Description = "Sends an activation mail to the new user's email address")]
        [OpenApiRequestBody("application/json", typeof(UserCreateDTO), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(UserResponseDTO), Description = "The user has been created successfully")]
        public async Task<HttpResponseData> createUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user")] HttpRequestData req, FunctionContext executionContext)
        {
            #region Authentication Checks
            UserIdentityResult identityResult = GetUserIdentityResult(executionContext, req, new Role[2] { Role.Admin, Role.SuperAdmin });
            if (identityResult.ResponseMessage is not null)
                return identityResult.ResponseMessage;
            #endregion

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            UserCreateDTO userCreateDTO = JsonConvert.DeserializeObject<UserCreateDTO>(requestBody);

            #region Checks
            // check if email is in correct format
            if (!EmailService.IsValidEmail(userCreateDTO.Email))
            {
                HttpResponseData badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                return badRequest;
            }
            // check if user exists
            if (UserService.userExistsByEmail(userCreateDTO.Email))
            {
                HttpResponseData conflict = req.CreateResponse(HttpStatusCode.Conflict);
                return conflict;
            }
            // check if generated guid already exists
            string activationCode = Guid.NewGuid().ToString();
            activationCode = UserService.Truncate(activationCode, 8);
            User UserExists = UserService.GetUserByActivationCode(activationCode.ToString());
            while (UserExists != null)
            {
                activationCode = Guid.NewGuid().ToString();
                activationCode = UserService.Truncate(activationCode,8);
                UserExists = UserService.GetUserByActivationCode(activationCode);
            }
            // check when admin creating an account is in same organization
            User userCreating = UserService.GetUserById(identityResult.UserId);
            if (userCreating.UserType == Role.Admin)
            {
                if (userCreateDTO.Organization.Id != userCreating.Organization.Id)
                {
                    HttpResponseData badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                    return badRequest;
                }
            }
            #endregion
            #region Create user
            Organization organization = OrganizationService.GetOrganizationById(userCreateDTO.Organization.Id);
            User createdUser = Mapper.Map<User>(userCreateDTO);
            createdUser.ActivationCode = activationCode;
            createdUser.Organization = organization;
            createdUser = UserService.CreateUser(createdUser);

            UserResponseDTO user = Mapper.Map<UserResponseDTO>(createdUser);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(user);
            string subject = "Activate your account.";
            string content = "Welcome to VrefSolutions, \n\nActivate your account with the following ActivationCode: " + createdUser.ActivationCode;

            EmailService.sendEmail(user.Email, subject, content);
            #endregion

            return response;
        }

        [Function("login")]
        [OpenApiOperation(operationId: "login", tags: new[] { "user" }, Summary = "Login with user credentials.")]
        [OpenApiRequestBody("application/json", typeof(UserLoginRequestDTO), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(UserLoginResponseDTO), Description = "The user has logged in successfully")]
        public async Task<HttpResponseData> login(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user/login")] HttpRequestData req)
        {
            #region Checks if login data is correct
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            UserLoginRequestDTO userLoginDTO = JsonConvert.DeserializeObject<UserLoginRequestDTO>(requestBody);
            User userToLogin = UserService.GetUserByEmail(userLoginDTO.Email);
            if (object.ReferenceEquals(null, userToLogin))
            {
                HttpResponseData badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                return badRequest;
            }

            var response = req.CreateResponse();
            if (UserService.verifyPasswordHash(userLoginDTO.Password, userToLogin.Password))
            {
                UserLoginResponseDTO result = await TokenService.CreateToken(userToLogin);
                UserResponseDTO userResponseDTO = Mapper.Map<UserResponseDTO>(userToLogin);
                result.user = userResponseDTO;


                response.StatusCode = HttpStatusCode.OK;
                await response.WriteAsJsonAsync(result);
            }
            else
            {
                response.StatusCode = HttpStatusCode.BadRequest;
            }
            #endregion

            return response;
        }

    }
}
