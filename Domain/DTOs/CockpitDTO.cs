using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json;

namespace VRefSolutions.Domain.DTO
{
    public class CockpitDTO
    {
        [JsonRequired]
        [OpenApiProperty(Description = "The cameras that will be used to stream the training session.")]
        public List<CameraDTO> Cameras { get; set; }
    }
}