using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using VRefSolutions.Domain.Models;

namespace VRefSolutions.Domain.DTO
{
    public class EventResponseDTO
    {
        [OpenApiProperty(Description = "Id of the event response")]
        public int Id { get; set; }

        [OpenApiProperty(Description = "Name of the event response")]
        public string Name { get; set; }

        [OpenApiProperty(Description = "Symbol associated with the event")]
        public string Symbol { get; set; }

        [OpenApiProperty(Description = "Timestamp of this event")]
        public TimeStamp TimeStamp { get; set; }

        [OpenApiProperty(Description = "Message added by the instructer for the event")]
        public string Message { get; set; }

        public EventResponseDTO()
        {
        }
    }
}