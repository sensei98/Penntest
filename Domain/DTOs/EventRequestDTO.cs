using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json;
using VRefSolutions.Domain.Models;

namespace VRefSolutions.Domain.DTO
{
    public class EventRequestDTO
    {
        [JsonRequired]
        [OpenApiProperty(Description = "The name of the custom event.")]
        public string Name { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "The message that describes the custom event.")]
        public string Message { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "The symbol of the custom event.")]
        public string Symbol { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "The timestamp expressed in Hours, Minutes, Seconds, and Miliseconds.")]
        public TimeStamp TimeStamp { get; set; }

        public EventRequestDTO()
        {
        }
    }
}