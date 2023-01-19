using System;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using VRefSolutions.Domain.Models;
using Newtonsoft.Json;

namespace VRefSolutions.Domain.DTO
{
    public class AltitudeResponseDTO
    {
        [JsonRequired]
        [OpenApiProperty(Description = "Id of altitude")]
        public string Id { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "The AMSL(Above mean sea level) of the altitude object")]
        public double Amsl { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "Timestamp of the altitude")]
        public TimeStamp TimeStamp { get; set; }

        public AltitudeResponseDTO()
        {
        }
    }
}