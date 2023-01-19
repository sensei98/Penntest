using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using VRefSolutions.Domain.DTO;
using VRefSolutions.Domain.Entities;

namespace VRefSolutions.Service.Interfaces
{
    public interface ITokenService
    {
        Task<UserLoginResponseDTO> CreateToken(User userToLogin);
        Task<ClaimsPrincipal> GetByValue(string Value);
    }
}
