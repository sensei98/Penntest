using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json;

namespace VRefSolutions.Domain.DTO
{
    public class TrainingRequestDTO
    {
        [JsonRequired]
        [OpenApiProperty(Description = "The students who are associated with the training.")]
        public List<int> Students { get; set; }
        // [JsonRequired]
        // [OpenApiProperty(Description = "The Instructor overseeing the training.")]
        // public int InstructorId { get; set; }
    }
}