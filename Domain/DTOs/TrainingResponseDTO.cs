using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using VRefSolutions.Domain.Enums;

namespace VRefSolutions.Domain.DTO
{
    public class TrainingResponseDTO
    {
        [OpenApiProperty(Description = "Id of the training response")]
        public int Id { get; set; }

        [OpenApiProperty(Description = "List of videos which are recorded within the training")]
        public List<string> Videos { get; set; } // generated on /training/{trainingID}/stop (if endsession == true)

        [OpenApiProperty(Description = "List of video access urls")]
        public List<string> VideoAccesURLs { get; set; } // generated on /training/{trainingID}

        [OpenApiProperty(Description = "List of events for a training")]
        public List<EventResponseDTO> Events { get; set; }

        [OpenApiProperty(Description = "List of altitudes for a training")]
        public List<AltitudeResponseDTO>? Altitudes { get; set; }

        [OpenApiProperty(Description = "List of students which participated in the training")]
        public List<UserResponseDTO> Students { get; set; }

        [OpenApiProperty(Description = "The Instructor which gave the training")]
        public UserResponseDTO Instructor { get; set; }

        [OpenApiProperty(Description = "The creation datetime of a training")]
        public DateTime CreationDateTime { get; set; }

        [OpenApiProperty(Description = "The status of a training")]
        public Status Status { get; set; }
    }
}