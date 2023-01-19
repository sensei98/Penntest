using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json;
using VRefSolutions.Domain.Models;

namespace VRefSolutions.Domain.DTO
{
    public class PredictionRequestDTO
    {
        [JsonRequired]
        [OpenApiProperty(Description = "The current timestamp of the training.")]
        public TimeStamp TimeStamp { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "The snapshot of the cockpit.")]
        public byte[] File { get; set; }
    }
}