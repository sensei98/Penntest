using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json;

namespace VRefSolutions.Domain.DTO
{
    public class UserLoginRequestDTO
    {
        [JsonRequired]
        [OpenApiProperty(Description = "The email of an user")]
        public string Email { get;set; }

        [JsonRequired]
        [OpenApiProperty(Description = "The password of an user")]
        public string Password { get; set; }
    }
}