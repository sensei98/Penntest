using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using VRefSoltutions.Validators;
using VRefSolutions.Domain.DTO;
using VRefSolutions.Domain.Models;
using VRefSolutions.Service.Interfaces;

namespace Company.VRefSolutions.Controller
{
    public class PredictionController : BaseController
    {

        private ILogger<PredictionController> Logger { get; }
        private IPredictionService PredictionService { get; }

        public PredictionController(ILogger<PredictionController> logger, IPredictionService predictionService)
        {
            Logger = logger;
            PredictionService = predictionService;
        }

        [Function("predictInstruments")]
        [OpenApiOperation(operationId: "predictInstruments", tags: new[] { "prediction" }, Summary = "Predict the states of instruments in the given screenshot. Creates separate Altitude and Event rows that need to be fetched.")]
        [OpenApiParameter(name: "TrainingID", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The **TrainingID** parameter")]
        [OpenApiRequestBody("application/json", typeof(PredictionRequestDTO), Required = true, Description = "The PredictionRequestDTO Object")]
        public async Task<HttpResponseData> predictInstruments(
                            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "prediction/{TrainingID}")] HttpRequestData req, int TrainingID)
        {
            #region DTO Validation
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            PredictionRequestDTO predictionRequestDTO = GetSerializedJsonObject<PredictionRequestDTO>(requestBody);
            var validationResult = new PredictionRequestDTOValidator().Validate(predictionRequestDTO);
            if (!validationResult.IsValid)
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, validationResult.Errors);
            #endregion

            try
            {
                await PredictionService.PredictInstruments(TrainingID, predictionRequestDTO.TimeStamp, predictionRequestDTO.File);
            }
            catch (Exception e)
            {
                return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, e.Message);
            }

            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        [Function("predictAltitude")]
        public async Task predictAltitude([QueueTrigger("altitude-position")] PositionMessage message)
        {
            try
            {
                // Doesn't work due to Azure issues with the Python function
                // await PredictionService.PredictAltitude(message);
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }
        }

        [Function("predictEcamEvents")]
        public async Task predictEcamEvents([QueueTrigger("ecam-position")] PositionMessage message)
        {
            try
            {
                await PredictionService.PredictEcamEvents(message);
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }
        }
    }
}