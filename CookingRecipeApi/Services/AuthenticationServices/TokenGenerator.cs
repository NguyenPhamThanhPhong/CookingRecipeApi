using CookingRecipeApi.Configs;
using CookingRecipeApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace CookingRecipeApi.Services.AuthenticationServices
{
    public class TokenGenerator
    {
        private readonly AuthenticationConfigs _authenticationConfigs;
        private readonly JwtSecurityTokenHandler _tokenHandler;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public TokenGenerator(AuthenticationConfigs AuthenticationConfigs)
        {
            _authenticationConfigs = AuthenticationConfigs;
            _tokenHandler = new JwtSecurityTokenHandler();
            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidIssuer = _authenticationConfigs.Issuer,
                ValidAudience = _authenticationConfigs.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationConfigs.AccessTokenSecret))
            };
        }
        public string GenerateAccessToken(User user)
        {
            SecurityKey key = 
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationConfigs.AccessTokenSecret));
            SigningCredentials credential = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier,user.id),
                new Claim(ClaimTypes.Email,user.authenticationInfo.email??""),
                new Claim(ClaimTypes.Name,user.profileInfo.fullName),
                new Claim(ClaimTypes.AuthenticationMethod,user.authenticationInfo.linkedAccountType??"default"),
            };
            Console.WriteLine(JsonSerializer.Serialize(claims));

            JwtSecurityToken token = new JwtSecurityToken(
                _authenticationConfigs.Issuer,
                _authenticationConfigs.Audience,
                claims,
                DateTime.UtcNow,
                DateTime.UtcNow.AddMinutes(_authenticationConfigs.AccessTokenExpirationMinutes),
                credential
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            SecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _authenticationConfigs.RefreshTokenSecret));
            SigningCredentials credential = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            List<Claim> claims = new List<Claim>();

            JwtSecurityToken token = new JwtSecurityToken(
                _authenticationConfigs.Issuer,
                _authenticationConfigs.Audience,
                claims,
                DateTime.UtcNow,
                DateTime.UtcNow.AddMinutes(_authenticationConfigs.RefreshTokenExpirationMinutes),
                credential
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool ValidateToken(string refreshToken)
        {
            try
            {
                _tokenHandler.ValidateToken(
                    refreshToken, _tokenValidationParameters, out SecurityToken validatedToken);
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
