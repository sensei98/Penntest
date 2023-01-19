using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json;

namespace VRefSolutions.Domain.DTO
{
    public class OrganizationCreateDTO
    {
        [JsonRequired]
        [OpenApiProperty(Description = "Name of organization to be created")]
        public string Name {get;set;}
    }
}