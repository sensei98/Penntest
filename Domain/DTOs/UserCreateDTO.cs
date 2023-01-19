using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json;

namespace VRefSolutions.Domain.DTO
{
    public class UserCreateDTO
    {
        [JsonRequired]
        [OpenApiProperty(Description = "The email of an user")]
        public string Email { get;set; }

        [JsonRequired]
        [OpenApiProperty(Description = "The firstname of an user")]
        public string FirstName {get;set;}
        
        [JsonRequired]
        [OpenApiProperty(Description = "The lastname of an user")]
        public string LastName {get;set;}
        
        [JsonRequired]
        [OpenApiProperty(Description = "The organization which a user is linked to")]
        public OrganizationDTO Organization { get; set; }
    }
}