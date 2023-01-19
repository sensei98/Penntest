using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json;

namespace VRefSolutions.Domain.DTO
{
    public class OrganizationResponseDTO
    {
        [JsonRequired]
        [OpenApiProperty(Description = "Id of the organization")]
        public int Id { get; set; }
        [JsonRequired]
        [OpenApiProperty(Description = "Name of the organization")]
        public string? Name { get; set; }
        [JsonRequired]
        [OpenApiProperty(Description = "List of the users of this organization")]
        public List<UserResponseDTO>? Users { get; set; }

        public OrganizationResponseDTO(int Id, string Name, List<UserResponseDTO> Users)
        {
            this.Id = Id;
            this.Name = Name;
            this.Users = Users;
        }
    }
}