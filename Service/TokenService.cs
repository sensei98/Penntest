using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using VRefSolutions.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using VRefSolutions.Domain.DTO;
using VRefSolutions.Domain.Entities;

namespace VRefSolutions.Service
{

    public class TokenIdentityValidationParameters : TokenValidationParameters
    {
        public TokenIdentityValidationParameters(string Issuer, string Audience, SymmetricSecurityKey SecurityKey)
        {
            RequireSignedTokens = true;
            ValidAudience = Audience;
            ValidateAudience = true;
            ValidIssuer = Issuer;
            ValidateIssuer = true;
            ValidateIssuerSigningKey = true;
            ValidateLifetime = true;
            IssuerSigningKey = SecurityKey;
            AuthenticationType = "Bearer";
        }
    }

    public class TokenService : ITokenService
    {
        private ILogger Logger { get; }
        private string Issuer { get; }
        private string Audience { get; }
        private TimeSpan ValidityDuration { get; }
        private SigningCredentials Credentials { get; }
        private TokenIdentityValidationParameters ValidationParameters { get; }

        public TokenService(IConfiguration Configuration, ILogger<TokenService> Logger)
        {
            this.Logger = Logger;

            Issuer = /*Configuration.GetClassValueChecked("JWT:Issuer", */"DebugIssuer";//, Logger);
            Audience = /*Configuration.GetClassValueChecked("JWT:Audience", */"DebugAudience";//, Logger);
            ValidityDuration = TimeSpan.FromHours(9);// Todo: configure
            string Key = /*Configuration.GetClassValueChecked("JWT:Key", */"DebugKey DebugKey";//, Logger);

            SymmetricSecurityKey SecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));

            Credentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256Signature);

            ValidationParameters = new TokenIdentityValidationParameters(Issuer, Audience, SecurityKey);
        }

        public async Task<UserLoginResponseDTO> CreateToken(User userToLogin)
        {
            JwtSecurityToken Token = await CreateToken(new Claim[] {
            new Claim(ClaimTypes.Role, userToLogin.UserType.ToString()),
            new Claim(ClaimTypes.Name, userToLogin.Email),
            new Claim(ClaimTypes.PrimarySid, userToLogin.Id.ToString())
                });

            return new UserLoginResponseDTO(Token);
        }

        private async Task<JwtSecurityToken> CreateToken(Claim[] Claims)
        {
            JwtHeader Header = new JwtHeader(Credentials);

            JwtPayload Payload = new JwtPayload(Issuer,
                           Audience,
                                                Claims,
                                                DateTime.UtcNow,
                                                DateTime.UtcNow.Add(ValidityDuration),
                                                DateTime.UtcNow);

            JwtSecurityToken SecurityToken = new JwtSecurityToken(Header, Payload);

            return await Task.FromResult(SecurityToken);
        }

        public async Task<ClaimsPrincipal> GetByValue(string Value)
        {
            if (Value == null)
            {
                throw new Exception("No Token supplied");
            }

            JwtSecurityTokenHandler Handler = new JwtSecurityTokenHandler();

            try
            {
                SecurityToken ValidatedToken;
                ClaimsPrincipal Principal = Handler.ValidateToken(Value, ValidationParameters, out ValidatedToken);

                return await Task.FromResult(Principal);
            }
            catch (Exception e)
            {
                string exception = e.ToString();
                if (exception.Contains("The token is expired") == true)
                {
                    Console.WriteLine("Token is expired!");
                }
                throw;

            }
        }
    }
}