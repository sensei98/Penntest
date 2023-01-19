using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json;

namespace VRefSolutions.Domain.DTO
{
    public class OrganizationDTO
    {
        [JsonRequired]
        [OpenApiProperty(Description = "Id of the organization")]
        public int Id { get; set; }

        public OrganizationDTO(int id)
        {
            this.Id = id;
        }
        public OrganizationDTO()
        {
            
        }

    }
}
