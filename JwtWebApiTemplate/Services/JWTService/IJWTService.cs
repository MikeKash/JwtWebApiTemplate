using JwtWebApiTemplate.Models;
using System.IdentityModel.Tokens.Jwt;

namespace JwtWebApiTemplate.Services.JWTService
{
    public interface IJWTService
    {
        JwtSecurityToken VerifyJWTToken(string jwt);
        string CreateToken(User user);
        RefreshToken GenerateRefreshToken();
        void SetRefreshToken(RefreshToken newRefreshToken, HttpResponse Reponse);
        void DeleteRefreshToken(string refreshTokenName, HttpResponse Response);
    }
}
