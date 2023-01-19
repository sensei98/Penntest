using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRefSolutions.Domain.DTO
{
    public class UserActivateDTO
    {
        [JsonRequired]
        [OpenApiProperty(Description = "Activation which is needed to activate an user")]
        public string ActivationCode { get; set; }

        [JsonRequired]
        [OpenApiProperty(Description = "Password for the user which is needed to activate the user")]
        public string Password { get; set; }
    }
}
