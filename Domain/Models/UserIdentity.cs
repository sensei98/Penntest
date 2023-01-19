using Microsoft.Azure.Functions.Worker.Http;
using VRefSolutions.Domain.Enums;

namespace VRefSolutions.Domain.Models
{
    public class UserIdentityResult
    {
        public int UserId { get; set; }
        public Role Role { get; set; }
        public HttpResponseData? ResponseMessage { get; set; }


        public UserIdentityResult()
        {
        }
    }
}