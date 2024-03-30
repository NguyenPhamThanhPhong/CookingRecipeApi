using CookingRecipeApi.Configs;
using CookingRecipeApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CookingRecipeApi.Services.AuthenticationServices
{
    public class TokenGenerator
    {
        private readonly AuthenticationConfigs _authenticationConfigs;

        public TokenGenerator(AuthenticationConfigs AuthenticationConfigs)
        {
            _authenticationConfigs = AuthenticationConfigs;
        }
        public string GenerateAccessToken(User user)
        {
            SecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _authenticationConfigs.AccessTokenSecret));
            SigningCredentials credential = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier,user.id),
                new Claim(ClaimTypes.Email,user.authenticationInfo.email??""),
            };

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

    }
}
