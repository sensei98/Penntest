using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using YamlDotNet.Core.Tokens;

namespace VRefSolutions.Domain.DTO
{
    public class UserLoginResponseDTO
    {
        private JwtSecurityToken Token { get; }

        [OpenApiProperty(Description = "The Access token to be used in every subsequent operation for this user")]
        [JsonRequired]
        public string AccessToken => new JwtSecurityTokenHandler().WriteToken(Token);

        [OpenApiProperty(Description = "The token type")]
        [JsonRequired]
        public string TokenType { get; set; } = "Bearer";

        [OpenApiProperty(Description = "The amount of seconds until the token expires.")]
        [JsonRequired]
        public int ExpiresIn => (int)(Token.ValidTo - DateTime.UtcNow).TotalSeconds;

        public UserResponseDTO user { get; set; }

        public UserLoginResponseDTO(JwtSecurityToken Token)
        {
            this.Token = Token;
        }
    }
}