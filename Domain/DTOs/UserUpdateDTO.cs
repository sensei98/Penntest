using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json;
using VRefSolutions.Domain.Enums;

namespace VRefSolutions.Domain.DTO
{
    public class UserUpdateDTO
    {
        [JsonRequired]
        [OpenApiProperty(Description = "The email of an user")]
        public string Email { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "The firstname of an user")]
        public string FirstName { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "the lastname of an user")]
        public string LastName { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "The type of an user")]
        public Role UserType { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "The organization which the user is linked to")]
        public OrganizationDTO Organization { get; set; }
    }
}
