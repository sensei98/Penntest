using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using VRefSolutions.Domain.Enums;

namespace VRefSolutions.Domain.DTO
{
    public class TrainingsResponseDTO
    {
        [OpenApiProperty(Description = "Id of the training response")]
        public int Id { get; set; }
        [OpenApiProperty(Description = "The creation datetime of a training")]
        public DateTime CreationDateTime { get; set; }

        [OpenApiProperty(Description = "The status of a training")]
        public Status Status { get; set; }
        
        [OpenApiProperty(Description = "List of students which participated in the training")]
        public List<UserResponseDTO> Students { get; set; }

        [OpenApiProperty(Description = "The Instructor which gave the training")]
        public UserResponseDTO Instructor { get; set; }
    }
}