using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRefSolutions.Domain.DTO
{
    public class OrganizationsDTO
    {
        [JsonRequired]
        [OpenApiProperty(Description = "Id of the organization")]
        public int Id { get; set; }
        [JsonRequired]
        [OpenApiProperty(Description = "Name of the organization")]
        public string? Name { get; set; }

        public OrganizationsDTO(int Id, string Name)
        {
            this.Id = Id;
            this.Name = Name;
        }
    }
}