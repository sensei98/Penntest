using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json;

namespace VRefSolutions.Domain.DTO
{
    public class StopTrainingDTO
    {
        [JsonRequired]
        [OpenApiProperty(Default = false, Description = "Determines whether the training session is ended (true) or just paused (false)")]
        public bool EndTrainingSession { get; set; }
    }
}