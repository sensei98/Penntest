using AutoMapper;
using FluentValidation.Results;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Service;
using System.Net;
using System.Security.Claims;
using VRefSoltutions.Validators;
using VRefSolutions.Domain.DTO;
using VRefSolutions.Domain.Entities;
using VRefSolutions.Domain.Enums;
using VRefSolutions.Domain.Models;
using VRefSolutions.Service;
using VRefSolutions.Service.Interfaces;

namespace Company.VRefSolutions.Controller
{
    public class TrainingController : BaseController
    {

        private ILogger<TrainingController> Logger { get; }
        private IMapper Mapper { get; }
        private ITrainingService TrainingService { get; }
        private IUserService UserService { get; }
        private IEventTypeService EventTypeService { get; }
        private IEventService EventService { get; }
        private IAltitudeService AltitudeService { get; }

        private const int DEFAULT_EVENT_RANGE = 5;

        public TrainingController(ILogger<TrainingController> logger, IMapper mapper,
         ITrainingService trainingService, IUserService userService, IEventService eventService,
         IEventTypeService eventTypeService, IAltitudeService altitudeService)
        {
            Logger = logger;
            Mapper = mapper;
            TrainingService = trainingService;
            UserService = userService;
            EventTypeService = eventTypeService;
            EventService = eventService;
            AltitudeService = altitudeService;
        }

        [Auth]
        [Function("getTrainings")]
        [OpenApiOperation(operationId: "getTrainings", tags: new[] { "training" }, Summary = "Get all trainings the user has access to.")]
        public async Task<HttpResponseData> getTrainings(
                            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "training")] HttpRequestData req, FunctionContext executionContext)
        {
            // Initial Authentication and Authorization 
            UserIdentityResult identityResult = GetUserIdentityResult(executionContext, req);
            if (identityResult.ResponseMessage is not null)
                return identityResult.ResponseMessage;

            List<Training> trainings = new List<Training>();
            if (identityResult.Role == Role.Student || identityResult.Role == Role.Instructor)
            {
                trainings.AddRange(TrainingService.GetTrainingsByUserId(identityResult.UserId));
            }
            else if (identityResult.Role == Role.Admin)
            {
                trainings.AddRange(TrainingService.GetTrainingsByUserOrganization(identityResult.UserId));
            }
            else if (identityResult.Role == Role.SuperAdmin)
            {
                trainings.AddRange(TrainingService.GetAll());
            }

            List<TrainingsResponseDTO> trainingsResponseDTOs = new List<TrainingsResponseDTO>();
            trainings.ForEach(training => trainingsResponseDTOs.Add(Mapper.Map<TrainingsResponseDTO>(training)));

            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(trainingsResponseDTOs);
            return response;
        }

        [Auth]
        [Function("getTraining")]
        [OpenApiOperation(operationId: "getTraining", tags: new[] { "training" }, Summary = "Get a specific training from the authorized user.")]
        [OpenApiParameter(name: "TrainingID", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The **TrainingID** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TrainingResponseDTO), Description = "OK Response with the Training Object")]

        public async Task<HttpResponseData> getTraining(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "training/{TrainingID}")] HttpRequestData req, int TrainingID, FunctionContext executionContext)
        {
            // Initial Authentication and Authorization 
            UserIdentityResult identityResult = GetUserIdentityResult(executionContext, req);
            if (identityResult.ResponseMessage is not null)
                return identityResult.ResponseMessage;

            User loggedInUser = UserService.GetUserById(identityResult.UserId);
            Training targetTraining = TrainingService.GetTrainingById(TrainingID);
            if (targetTraining is null || !TrainingService.IsUserAuthorizedToViewTraining(targetTraining,loggedInUser))
                return await CreateErrorResponse(req, HttpStatusCode.NotFound);

            awakenMapperIdkWtfIsGoingOn();
            TrainingResponseDTO trainingResponse = Mapper.Map<TrainingResponseDTO>(targetTraining);
            if (TrainingService.IsStudentPartOfTraining(targetTraining, identityResult.UserId))
                trainingResponse.VideoAccesURLs = TrainingService.GetVideoAccessURLs(targetTraining).ToList();
            else
                trainingResponse.VideoAccesURLs = new List<string>();
            return await CreateJsonResponse(req, HttpStatusCode.OK, trainingResponse);
        }

        [Auth]
        [Function("createTraining")]
        [OpenApiOperation(operationId: "createTraining", tags: new[] { "training" }, Summary = "Create a training session")]
        [OpenApiRequestBody("application/json", typeof(TrainingRequestDTO), Required = true, Description = "The TrainingRequestDTO Object")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Created, contentType: "application/json", bodyType: typeof(TrainingResponseDTO), Description = "OK Response with Training object")]
        public async Task<HttpResponseData> createTraining(
                            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "training")] HttpRequestData req, FunctionContext executionContext)
        {
            // Initial Authentication and Authorization
            UserIdentityResult identityResult = GetUserIdentityResult(executionContext, req, new Role[1] { Role.Instructor });
            if (identityResult.ResponseMessage is not null)
                return identityResult.ResponseMessage;

            // DTO Validation
            TrainingRequestDTO trainingDTO = GetDeserializedJsonObject<TrainingRequestDTO>(req.Body);
            var validationResult = new TrainingRequestDTOValidator().Validate(trainingDTO);
            if (!validationResult.IsValid)
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, validationResult.Errors);

            List<User> participants = new List<User>();
            foreach (int id in trainingDTO.Students)
            {
                var studentUser = UserService.GetUserById(id);
                if (Object.Equals(studentUser, null) || studentUser.UserType != Role.Student)
                    return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "You can only add Students to the list of Participants");
                participants.Add(studentUser);
            }
            User instructor = UserService.GetUserById(identityResult.UserId);
            participants.Add(instructor);

            Training training = new Training(participants);
            TrainingService.CreateTraining(training);

            TrainingResponseDTO trainingResponse = Mapper.Map<TrainingResponseDTO>(training);
            return await CreateJsonResponse(req, HttpStatusCode.Created, trainingResponse);
        }

        [Auth]
        [Function("deleteTraining")]
        [OpenApiOperation(operationId: "deleteTraining", tags: new[] { "training" }, Summary = "Delete a Training")]
        [OpenApiParameter(name: "TrainingID", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The **TrainingID** parameter")]
        public async Task<HttpResponseData> deleteTraining(
                    [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "training/{TrainingID}")] HttpRequestData req, int TrainingID, FunctionContext executionContext)
        {
            // Initial Authentication and Authorization
            UserIdentityResult identityResult = GetUserIdentityResult(executionContext, req, new Role[2] { Role.Admin, Role.SuperAdmin });
            if (identityResult.ResponseMessage is not null)
                return identityResult.ResponseMessage;
            User loggedInUserObject = UserService.GetUserById(identityResult.UserId);
            Training training = TrainingService.GetTrainingById(TrainingID);
            // not found or Admin attempts to delete another Org's training.
            if (training == null || identityResult.Role == Role.Admin  
               && TrainingService.GetInstructorFromTraining(training).Organization.Id != loggedInUserObject.Organization.Id)
                return await CreateErrorResponse(req, HttpStatusCode.NotFound, "The requested Training cannot be found.");
            TrainingService.DeleteTraining(training);

            return await CreateJsonResponse(req, HttpStatusCode.NoContent);
        }

        [Auth]
        [Function("updateTraining")]
        [OpenApiOperation(operationId: "updateTraining", tags: new[] { "training" }, Summary = "Update a training session")]
        [OpenApiParameter(name: "TrainingID", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The **TrainingID** parameter")]
        [OpenApiRequestBody("application/json", typeof(TrainingRequestDTO), Required = true, Description = "The TrainingDTO Object")]
        public async Task<HttpResponseData> updateTraining(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "training/{TrainingID}")] HttpRequestData req, int TrainingID, FunctionContext executionContext)
        {
            // Initial Authentication and Authorization
            UserIdentityResult identityResult = GetUserIdentityResult(executionContext, req, new Role[3] { Role.Instructor, Role.Admin, Role.SuperAdmin });
            if (identityResult.ResponseMessage is not null)
                return identityResult.ResponseMessage;
            Training targetTraining = TrainingService.GetTrainingById(TrainingID);
            if (targetTraining is null)
                return await CreateErrorResponse(req, HttpStatusCode.NotFound, "The requested Training cannot be found.");
            if (identityResult.Role == Role.Instructor && !TrainingService.IsInstructorOfTraining(identityResult.UserId, targetTraining)
                || targetTraining.Status == Status.Finished )
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized);

            User instructor = TrainingService.GetInstructorFromTraining(targetTraining);
            // DTO Validation
            TrainingRequestDTO trainingDTO = GetSerializedJsonObject<TrainingRequestDTO>(await new StreamReader(req.Body).ReadToEndAsync());
            var validationResult = new TrainingRequestDTOValidator().Validate(trainingDTO);
            if (!validationResult.IsValid)
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, validationResult.Errors);

            List<User> participants = new List<User>();
            foreach (int id in trainingDTO.Students)
            {
                var studentUser = UserService.GetUserById(id);
                if (Object.Equals(studentUser, null) || studentUser.UserType != Role.Student)
                    return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "You can only add Students to the list of Participants");
                participants.Add(studentUser);
            }
            participants.Add(instructor);

            targetTraining.Participants = participants;
            TrainingService.updateTraining(targetTraining);
            TrainingResponseDTO trainingResponse = Mapper.Map<TrainingResponseDTO>(targetTraining);
            return await CreateJsonResponse(req, HttpStatusCode.OK, trainingResponse);
        }

        [Auth]
        [Function("addEventCustom")]
        [OpenApiOperation(operationId: "addEventCustom", tags: new[] { "training" }, Summary = "Add a custom event to the specified Training with the given message.")]
        [OpenApiParameter(name: "TrainingID", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The **TrainingID** parameter")]
        [OpenApiRequestBody("application/json", typeof(EventRequestDTO), Required = true, Description = "The EventRequestDTO Object")]
        public async Task<HttpResponseData> addEventCustom(
                    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "training/{TrainingID}/event")] HttpRequestData req, int TrainingID, FunctionContext executionContext)
        {
            // Initial Authentication and Authorization
            UserIdentityResult identityResult = GetUserIdentityResult(executionContext, req, new Role[3] { Role.Instructor, Role.Admin, Role.SuperAdmin });
            if (identityResult.ResponseMessage is not null)
                return identityResult.ResponseMessage;
            Training targetTraining = TrainingService.GetTrainingById(TrainingID);
            if (targetTraining is null)
                return await CreateErrorResponse(req, HttpStatusCode.NotFound, "The requested Training cannot be found.");
            if (identityResult.Role == Role.Instructor && !TrainingService.IsInstructorOfTraining(identityResult.UserId, targetTraining))
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized);

            // DTO Validation
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            EventRequestDTO eventRequestDTO = GetSerializedJsonObject<EventRequestDTO>(requestBody);
            var validationResult = new EventRequestDTOValidator().Validate(eventRequestDTO);
            if (!validationResult.IsValid)
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, validationResult.Errors);

            EventType markedEvent = EventTypeService.GetByName("Feedback");

            if (markedEvent is null)
                return await CreateErrorResponse(req, HttpStatusCode.NotFound, "The requested Event Type cannot be not found.");

            Event trainingEvent = new()
            {
                EventType = markedEvent,
                OverwriteName = eventRequestDTO.Name,
                OverwriteMessage = eventRequestDTO.Message,
                OverwriteSymbol = eventRequestDTO.Symbol,
                TimeStamp = eventRequestDTO.TimeStamp,
                Training = targetTraining
            };

            EventService.CreateEvent(trainingEvent);

            EventResponseDTO eventResponse = Mapper.Map<EventResponseDTO>(trainingEvent);

            HttpResponseData response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(eventResponse);

            return response;
        }

        [Auth]
        [Function("getEvents")]
        [OpenApiOperation(operationId: "getEvents", tags: new[] { "training" }, Summary = "Get events from the specified Training.")]
        [OpenApiParameter(name: "TrainingID", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The **TrainingID** parameter")]
        [OpenApiParameter(name: "Time", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "Timestamp for fetching events near a timestamp (eg. 01:23:45)")]
        [OpenApiParameter(name: "Range", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Range (in seconds) for specifying the range of events that need to be fetched (eg. 10)")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(EventResponseDTO), Description = "OK Response with the Event object list")]
        public async Task<HttpResponseData> getEvents(
                            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "training/{TrainingID}/event")] HttpRequestData req, int TrainingID, RangeFilterQuery query, FunctionContext executionContext)
        {
            // Initial Authentication and Authorization
            UserIdentityResult identityResult = GetUserIdentityResult(executionContext, req);
            if (identityResult.ResponseMessage is not null)
                return identityResult.ResponseMessage;
            Training targetTraining = TrainingService.GetTrainingById(TrainingID);
            if (targetTraining is null)
                return await CreateErrorResponse(req, HttpStatusCode.NotFound, "The requested Training cannot be found.");
            if (identityResult.Role == Role.Instructor && !TrainingService.IsInstructorOfTraining(identityResult.UserId, targetTraining))
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized);
            if (identityResult.Role == Role.Student && !TrainingService.IsStudentPartOfTraining(targetTraining, identityResult.UserId))
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized);

            HttpResponseData response;
            List<EventResponseDTO> eventsResponseDTO = new();
            List<Event> events;

            if (!string.IsNullOrEmpty(query.Time))
            {
                bool isRangeNumeric = int.TryParse(query.Range, out int range);

                range = isRangeNumeric ? range : DEFAULT_EVENT_RANGE;
                TimeStamp time = TimeStamp.Parse(query.Time, ':');

                events = EventService.GetEventsInRangeByTrainingId(TrainingID, time, range);
            }
            else
            {
                events = EventService.GetEventsByTrainingId(TrainingID);
            }

            foreach (Event eventObj in events)
            {
                eventsResponseDTO.Add(Mapper.Map<EventResponseDTO>(eventObj));
            }

            response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(eventsResponseDTO);

            return response;
        }

        [Auth]
        [Function("deleteEvent")]
        [OpenApiOperation(operationId: "deleteEvent", tags: new[] { "training" }, Summary = "Delete an Event")]
        [OpenApiParameter(name: "TrainingID", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The **TrainingID** parameter")]
        [OpenApiParameter(name: "EventID", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The **EventID** parameter")]
        public async Task<HttpResponseData> deleteEvent(
                           [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "training/{TrainingID}/event/{EventID}")] HttpRequestData req, int TrainingID, int EventID, FunctionContext executionContext)
        {
            // Initial Authentication and Authorization
            UserIdentityResult identityResult = GetUserIdentityResult(executionContext, req, new Role[3] { Role.Instructor, Role.Admin, Role.SuperAdmin });
            if (identityResult.ResponseMessage is not null)
                return identityResult.ResponseMessage;
            Training targetTraining = TrainingService.GetTrainingById(TrainingID);
            if (targetTraining is null)
                return await CreateErrorResponse(req, HttpStatusCode.NotFound, "The requested Training cannot be found.");
            if (identityResult.Role == Role.Instructor && !TrainingService.IsInstructorOfTraining(identityResult.UserId, targetTraining))
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized);

            Event eventObj = EventService.GetEventById(EventID);

            EventService.DeleteEvent(eventObj);

            var response = req.CreateResponse(HttpStatusCode.OK);

            return response;
        }

        [Auth]
        [Function("updateEvent")]
        [OpenApiOperation(operationId: "updateEvent", tags: new[] { "training" }, Summary = "Update an event's (overrided) message")]
        [OpenApiParameter(name: "TrainingID", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The **TrainingID** parameter")]
        [OpenApiParameter(name: "EventID", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The **EventID** parameter")]
        [OpenApiRequestBody("application/json", typeof(EventRequestDTO), Required = true, Description = "The EventDTO Object")]
        public async Task<HttpResponseData> updateEvent(
                           [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "training/{TrainingID}/event/{EventID}")] HttpRequestData req, int TrainingID, int EventID, FunctionContext executionContext)
        {
            // Initial Authentication and Authorization
            UserIdentityResult identityResult = GetUserIdentityResult(executionContext, req, new Role[3] { Role.Instructor, Role.Admin, Role.SuperAdmin });
            if (identityResult.ResponseMessage is not null)
                return identityResult.ResponseMessage;
            Training targetTraining = TrainingService.GetTrainingById(TrainingID);
            if (targetTraining is null)
                return await CreateErrorResponse(req, HttpStatusCode.NotFound, "The requested Training cannot be found.");
            if (identityResult.Role == Role.Instructor && !TrainingService.IsInstructorOfTraining(identityResult.UserId, targetTraining))
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized);

            // DTO Validation
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            EventRequestDTO eventRequestDTO = GetSerializedJsonObject<EventRequestDTO>(requestBody);
            var validationResult = new EventRequestDTOValidator().Validate(eventRequestDTO);
            if (!validationResult.IsValid)
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, validationResult.Errors);

            Event eventObj = EventService.GetEventById(EventID);

            if (eventObj is null)
                return await CreateErrorResponse(req, HttpStatusCode.NotFound);

            if (!eventObj.OverwriteName.Equals(eventRequestDTO.Name))
                eventObj.OverwriteName = eventRequestDTO.Name;
            if (!eventObj.OverwriteMessage.Equals(eventRequestDTO.Message))
                eventObj.OverwriteMessage = eventRequestDTO.Message;
            if (!eventObj.OverwriteSymbol.Equals(eventRequestDTO.Symbol))
                eventObj.OverwriteSymbol = eventRequestDTO.Symbol;
            if (!eventObj.TimeStamp.Equals(eventRequestDTO.TimeStamp))
                eventObj.TimeStamp = eventRequestDTO.TimeStamp;

            Event updatedEvent = EventService.UpdateEvent(eventObj);
            
            EventResponseDTO eventResponseDTO = Mapper.Map<EventResponseDTO>(updatedEvent);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(eventResponseDTO);

            return response;
        }

        [Auth]
        [Function("getAltitudes")]
        [OpenApiOperation(operationId: "getAltitudes", tags: new[] { "training" }, Summary = "Get altitudes from the specified Training.")]
        [OpenApiParameter(name: "TrainingID", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The **TrainingID** parameter")]
        [OpenApiParameter(name: "Time", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "Timestamp for fetching altitudes near a timestamp (eg. 01:23:45)")]
        [OpenApiParameter(name: "Range", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Range (in seconds) for specifying the range of altitudes that need to be fetched (eg. 10)")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AltitudeResponseDTO), Description = "OK Response with the Altitude object list")]
        public async Task<HttpResponseData> getAltitudes(
                    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "training/{TrainingID}/altitude")] HttpRequestData req, int TrainingID, RangeFilterQuery query, FunctionContext executionContext)
        {
            // Initial Authentication and Authorization
            UserIdentityResult identityResult = GetUserIdentityResult(executionContext, req, new Role[3] { Role.Instructor, Role.Admin, Role.SuperAdmin });
            if (identityResult.ResponseMessage is not null)
                return identityResult.ResponseMessage;
            Training targetTraining = TrainingService.GetTrainingById(TrainingID);
            if (targetTraining is null)
                return await CreateErrorResponse(req, HttpStatusCode.NotFound, "The requested Training cannot be found.");
            if (identityResult.Role == Role.Instructor && !TrainingService.IsInstructorOfTraining(identityResult.UserId, targetTraining))
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized);

            HttpResponseData response;
            List<AltitudeResponseDTO> altitudeResponseDTO = new();
            List<Altitude> altitudes;

            if (!string.IsNullOrEmpty(query.Time))
            {
                bool isRangeNumeric = int.TryParse(query.Range, out int range);

                range = isRangeNumeric ? range : DEFAULT_EVENT_RANGE;
                TimeStamp time = TimeStamp.Parse(query.Time, ':');

                altitudes = AltitudeService.GetAltitudesInRangeByTrainingId(TrainingID, time, range);
            }
            else
            {
                altitudes = AltitudeService.GetAltitudesByTrainingId(TrainingID);
            }

            foreach (Altitude altitude in altitudes)
            {
                altitudeResponseDTO.Add(Mapper.Map<AltitudeResponseDTO>(altitude));
            }

            response = req.CreateResponse(HttpStatusCode.OK);

            await response.WriteAsJsonAsync(altitudeResponseDTO);

            return response;
        }

        [Auth]
        [Function("startTraining")]
        [OpenApiOperation(operationId: "startTraining", tags: new[] { "training" }, Summary = "[Local API call]: Start the local recording of the training")]
        [OpenApiParameter(name: "TrainingID", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The **TrainingID** parameter")]
        [OpenApiRequestBody("application/json", typeof(CockpitDTO), Required = true, Description = "The Cockpit Object with a list of Camera objects")]
        public async Task<HttpResponseData> startTraining(
                            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "training/{TrainingID}/start")] HttpRequestData req, int TrainingID, FunctionContext executionContext)
        {
            // Initial Authentication and Authorization
            UserIdentityResult identityResult = GetUserIdentityResult(executionContext, req, new Role[3] { Role.Instructor, Role.Admin, Role.SuperAdmin });
            if (identityResult.ResponseMessage is not null)
                return identityResult.ResponseMessage;

            // DTO Validation
            CockpitDTO cockpitDTO = GetSerializedJsonObject<CockpitDTO>(await new StreamReader(req.Body).ReadToEndAsync());
            var validationResult = new CockpitDTOValidator().Validate(cockpitDTO);
            if (!validationResult.IsValid)
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, validationResult.Errors);

            // get the training
            Training training = TrainingService.GetTrainingById(TrainingID);

            if (training == null)
                return await CreateErrorResponse(req, HttpStatusCode.NotFound, "The requested Training cannot be found.");

            User trainingInstructor = TrainingService.GetInstructorFromTraining(training);
            if (trainingInstructor.Id != identityResult.UserId)
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Instructor can't start the training of another Instructor");

            if (training.Status != Status.Created && training.Status != Status.Paused)
                return await CreateErrorResponse(req, HttpStatusCode.MethodNotAllowed, "The Training can only be started if its current status is \"Created\" or \"Paused\"");

            training.Status = Status.Recording; // Change status to Recording

            List<string> trainingVideoNames = new List<string>(); // Set Cockpit camera names to Video names
            cockpitDTO.Cameras.ForEach(camera => trainingVideoNames.Add(camera.Name));
            training.Videos = trainingVideoNames;
            TrainingService.updateTraining(training);
            return await CreateJsonResponse(req, HttpStatusCode.NoContent);
        }


        [Function("stopTraining")]
        [OpenApiOperation(operationId: "stopTraining", tags: new[] { "training" }, Summary = "[Local API call]: Stop/Pause the local recording of the training")]
        [OpenApiParameter(name: "TrainingID", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The **TrainingID** parameter")]
        [OpenApiRequestBody("application/json", typeof(StopTrainingDTO), Required = true, Description = "Stops or pauses the local recording depending on the boolean")]
        public async Task<HttpResponseData> stopTraining(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "training/{TrainingID}/stop")] HttpRequestData req, int TrainingID, FunctionContext executionContext)
        {
            // Initial Authentication and Authorization
            UserIdentityResult identityResult = GetUserIdentityResult(executionContext, req, new Role[3] { Role.Instructor, Role.Admin, Role.SuperAdmin });
            if (identityResult.ResponseMessage is not null)
                return identityResult.ResponseMessage;

            // DTO Validation
            StopTrainingDTO stopTrainingDTO = GetSerializedJsonObject<StopTrainingDTO>(await new StreamReader(req.Body).ReadToEndAsync());
            var validationResult = new StopTrainingDTOValidator().Validate(stopTrainingDTO);
            if (!validationResult.IsValid)
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, validationResult.Errors);

            // get the training
            Training training = TrainingService.GetTrainingById(TrainingID);
            // not found
            if (training == null)
                return await CreateErrorResponse(req, HttpStatusCode.NotFound, "The requested Training cannot be found.");
                
            User trainingInstructor = TrainingService.GetInstructorFromTraining(training);
            if (trainingInstructor.Id != identityResult.UserId)
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Instructor can't stop the training of another Instructor");

            if (training.Status != Status.Recording && training.Status != Status.Paused)
                return await CreateErrorResponse(req, HttpStatusCode.MethodNotAllowed, "The Training can only be Stopped if its current status is \"Recording\" or \"Paused\"");

            training.Status = stopTrainingDTO.EndTrainingSession ? Status.Processing : Status.Paused;
            TrainingService.updateTraining(training);
            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            // give the Local server the organizationId so it can upload the recordings to the correct organization folder.  
            response.WriteString(trainingInstructor.Organization.Id.ToString(), System.Text.Encoding.UTF8);
            return response;
        }

        private void awakenMapperIdkWtfIsGoingOn()
        {
            // Talked with Consultants about this.
            // For some Reason a List of Objects does not use the Custom Mapper Profile untill a single object is Mapped
            // So we map a dummy object to "Warm up" the Mapper. 
            EventType eventType = new EventType { Name = "Feedback", Symbol = "feedback.svg", Message = "whatever" };
            Event trainingEvent = new Event
            {
                EventType = eventType,
                OverwriteMessage = "Dumb message",
                OverwriteName = "Dumb Name",
                OverwriteSymbol ="dumb.svg",
                TimeStamp = TimeStamp.Parse("00:00:00", ':'),
                Training = new Training { }
            };
            EventResponseDTO eventResponse = Mapper.Map<EventResponseDTO>(trainingEvent);
            trainingEvent = new Event{
                OverwriteMessage = "",
                OverwriteName = "",
                OverwriteSymbol ="",
                TimeStamp = TimeStamp.Parse("00:00:00", ':'),
                Training = new Training { }
            };
            eventResponse = Mapper.Map<EventResponseDTO>(trainingEvent);
        }

    }
}