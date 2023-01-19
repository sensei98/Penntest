using System.Net;
using VRefSolutions.Domain.DTO;
using VRefSolutions.Domain.Enums;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using VRefSolutions.Domain.Entities;
using VRefSolutions.Service.Interfaces;
using System.Security.Claims;
using Service;
using AutoMapper;
using VRefSolutions.Service;
using VRefSolutions.Domain.Models;

namespace Company.VRefSolutions.Controller
{
    public class OrganizationController : BaseController
    {
        private ILogger<OrganizationController> Logger { get; }
        private IOrganizationService  OrganizationService {get;}
        private IMapper Mapper { get; }
        private IUserService UserService { get; }
        public OrganizationController(ILogger<OrganizationController> logger, IOrganizationService organizationService, IMapper mapper, IUserService userService)
        {
            Logger = logger;
            OrganizationService = organizationService;
            Mapper = mapper;
            UserService = userService;
        }

        [Auth]
        [Function("create-organization")]
        [OpenApiOperation(operationId: "create-organization", tags: new[] { "organization" }, Summary = "Create an Organization")]
        [OpenApiRequestBody("application/json", typeof(OrganizationCreateDTO), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(OrganizationDTO), Description = "Organization created successfully.")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Conflict, Description = "Organization with this name already exists")]
        public async Task<HttpResponseData> createOrganization(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "organization")] HttpRequestData req, FunctionContext executionContext)
        {
            #region Authentication/Authorization Checks
            UserIdentityResult identityResult = GetUserIdentityResult(executionContext, req, new Role[1] { Role.SuperAdmin });
            if (identityResult.ResponseMessage is not null)
                return identityResult.ResponseMessage;
            #endregion

            #region Check if organization name exists
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            OrganizationCreateDTO createOrganizationDTO = JsonConvert.DeserializeObject<OrganizationCreateDTO>(requestBody);
            if (OrganizationService.CheckIfOrganizationNameExists(createOrganizationDTO.Name)) 
            {
                HttpResponseData badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                return badRequest;
            }
            #endregion

            #region Create organization
            var organization = new Organization{
                Name = createOrganizationDTO.Name};
            OrganizationService.CreateOrganization(organization);
            #endregion

            OrganizationDTO organizationCreateResponseDTO = Mapper.Map<OrganizationDTO>(organization);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(organizationCreateResponseDTO);

            return response;
        }

        [Auth]
        [Function("edit-organization")]
        [OpenApiOperation(operationId: "edit-organization", tags: new[] { "organization" }, Summary = "Edit an Organization")]
        [OpenApiParameter(name: "organizationId", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The organization id")]
        [OpenApiRequestBody("application/json", typeof(OrganizationCreateDTO), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(OrganizationDTO), Description = "The organization name changed successfully.")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Conflict, Description = "Organization with this name already exists")]
        public async Task<HttpResponseData> editOrganization(
                    [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "organization/{organizationId}")] HttpRequestData req, FunctionContext executionContext, int organizationId)
        {
            #region Authentication/Authorization Checks
            UserIdentityResult identityResult = GetUserIdentityResult(executionContext, req, new Role[2] { Role.SuperAdmin, Role.Admin });
            if (identityResult.ResponseMessage is not null)
                return identityResult.ResponseMessage;
            #endregion
            #region Create DTO and check if new name of organization already exists
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            OrganizationCreateDTO organizationRequestDTO = JsonConvert.DeserializeObject<OrganizationCreateDTO>(requestBody);

            Organization organizationFromOrganizationId = OrganizationService.GetOrganizationById(organizationId);
            if (object.ReferenceEquals(null, organizationFromOrganizationId) || OrganizationService.CheckIfOrganizationNameExists(organizationRequestDTO.Name))
            {
                HttpResponseData badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                return badRequest;
            }
            #endregion
            User loggedInUser = UserService.GetUserById(identityResult.UserId);

            if (loggedInUser.UserType == Role.Admin && loggedInUser.Organization.Id != organizationId)
            {
                HttpResponseData forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                return forbidden;
            }

            #region Update organization
            organizationFromOrganizationId.Name = organizationRequestDTO.Name;
            List<UserResponseDTO> OrganizationUsers = new List<UserResponseDTO>();
            if (organizationFromOrganizationId.Users.Count > 0)
            {
                foreach (User u in organizationFromOrganizationId.Users)
                {
                    UserResponseDTO userResponseDTO = Mapper.Map<UserResponseDTO>(u);
                    OrganizationUsers.Add(userResponseDTO);
                }
            }
            OrganizationService.updateOrganization(organizationFromOrganizationId);
            #endregion
            OrganizationResponseDTO organizationResponseDTO = new OrganizationResponseDTO(organizationFromOrganizationId.Id, organizationFromOrganizationId.Name, OrganizationUsers);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(organizationResponseDTO);
            return response;
        }


        [Auth]
        [Function("delete-organization")]
        [OpenApiOperation(operationId: "delete-organization", tags: new[] { "organization" }, Summary = "Delete an Organization")]
        [OpenApiParameter(name: "organizationId", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The organization id")]
        public async Task<HttpResponseData> deleteOrganization(
                        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "organization/{organizationId}")] HttpRequestData req, FunctionContext executionContext, int organizationId)
        {
            #region Authentication/Authorization Checks
            UserIdentityResult identityResult = GetUserIdentityResult(executionContext, req, new Role[1] { Role.SuperAdmin });
            if (identityResult.ResponseMessage is not null)
                return identityResult.ResponseMessage;
            #endregion

            #region Check if organization exists
            Organization organization = OrganizationService.GetOrganizationById(organizationId);
            if (object.ReferenceEquals(null, organization))
            {
                HttpResponseData badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                return badRequest;
            }
            #endregion
            OrganizationService.delete(organization);

            var response = req.CreateResponse(HttpStatusCode.NoContent);
            return response;
        }

        [Auth]
        [Function("get-organization")]
        [OpenApiOperation(operationId: "get-organization", tags: new[] { "organization" }, Summary = "return specific Organization")]
        [OpenApiParameter(name: "organizationId", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The organization id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(OrganizationResponseDTO), Description = "Retrieved organization successfull")]
        public async Task<HttpResponseData> getOrganization(
                [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "organization/{organizationId}")] HttpRequestData req, FunctionContext executionContext, int organizationId)
        {
            #region Authentication/Authorization Checks
            UserIdentityResult identityResult = GetUserIdentityResult(executionContext, req, new Role[2] {Role.Admin, Role.SuperAdmin });
            if (identityResult.ResponseMessage is not null)
                return identityResult.ResponseMessage;
            #endregion

            #region Check if given organizationId exists within the DB
            Organization organization = OrganizationService.GetOrganizationById(organizationId);
            if (object.ReferenceEquals(null, organization))
            {
                HttpResponseData badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                return badRequest;
            }
            //if requester is of role admin, check if the admin is assigned to the organization
            if (identityResult.Role == Role.Admin)
            {
                #region check if requested object is of same ID
                User targetUser = UserService.GetUserById(identityResult.UserId);
                if (targetUser.Organization.Id != organizationId)
                {
                    HttpResponseData forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                    return forbidden;
                }
                #endregion
            }
            #endregion

            OrganizationResponseDTO organizationResponseDTO = Mapper.Map<OrganizationResponseDTO>(organization);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(organizationResponseDTO);
            return response;
        }

        [Auth]
        [Function("get-all-organizations")]
        [OpenApiOperation(operationId: "get-all-organizations", tags: new[] { "organization" }, Summary = "return all Organizations")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(OrganizationDTO), Description = "Retrieved list of organization successfull")]
        public async Task<HttpResponseData> getAllOrganizations(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "organization")] HttpRequestData req, FunctionContext executionContext)
        {
            #region Authentication/Authorization Checks
            UserIdentityResult identityResult = GetUserIdentityResult(executionContext, req, new Role[1] { Role.SuperAdmin });
            if (identityResult.ResponseMessage is not null)
                return identityResult.ResponseMessage;
            #endregion

            #region Get all organizations
            List<Organization> organizations = OrganizationService.GetAllOrganizations();
            List<OrganizationsDTO> organizationListToResponse = new List<OrganizationsDTO>();
            foreach (Organization o in organizations)
            {
                OrganizationsDTO organizationDTO = Mapper.Map<OrganizationsDTO>(o);
                organizationListToResponse.Add(organizationDTO);
            }
            #endregion

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(organizationListToResponse);
            return response;

        }
    }
}