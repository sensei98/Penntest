using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRefSolutions.Domain.Enums;

namespace VRefSolutions.Domain.DTO
{
    public class UserResponseDTO
    {
        [JsonRequired]
        [OpenApiProperty(Description = "The id of an user")]
        public int Id { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "The email of an user")]
        public string Email { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "The firstname of an user")]
        public string? FirstName { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "The lastname of an user")]
        public string? LastName { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "The organization which the user is linked to")]
        public OrganizationsDTO Organization { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "The type of an user (Student, Instructur, Admin, Superadmin)")]
        public Role UserType { get; set; }

        public UserResponseDTO(int Id, string Email, string FirstName, string LastName, Role UserType, OrganizationsDTO Organization)
        {
            this.Id = Id;
            this.Email = Email;
            this.FirstName = FirstName;
            this.LastName = LastName;
            this.UserType = UserType;
            this.Organization = Organization;
        }
        public UserResponseDTO()
        {
            
        }
    }

}
