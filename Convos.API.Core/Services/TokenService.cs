using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Convos.API.Data;
using Convos.API.Data.Entities;
using Convos.API.Data.Responses;
using Microsoft.IdentityModel.Tokens;

namespace Convos.API.Core.Services;

public class TokenService
{
    private readonly DatabaseContext _context;
    private readonly IConfiguration _configuration;

    public TokenService(DatabaseContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public AccessTokenResponse GenerateAccessToken(User user)
    {
        var (token, expireDate) = GenerateJwt(user);

        return new AccessTokenResponse
        {
            AccessToken = token,
            User = user,
            ExpireDate = expireDate
        };
    }

    private (string, DateTime) GenerateJwt(User user)
    {
        var expireDate = DateTime.Now.AddDays(30);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new(ClaimTypes.Name, user.Username)
            }),

            Expires = expireDate,
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:key"])),
                    SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenHandler.WriteToken(securityToken), expireDate);
    }
}