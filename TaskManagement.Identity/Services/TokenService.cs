using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TaskManagement.Identity.Interfaces;
using TaskManagement.Identity.Models;

namespace TaskManagement.Identity.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // jwt har 3 dele, adskilt med .: header aka metadata (algoritme(bruger HmacSha256), Payload (Claims - ikke encrypted), 
    // Signature (Server signer header+payload med secret nøgle))
    public string GenerateToken(User user)
    {
        // claims = info om brugeren der skal i token
        var claims = new List<Claim> {
            new(ClaimTypes.NameIdentifier, user.id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        // får secret key fra config og convert til bytes. Key signer token, så verificering senere
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:key"]!));

        // Opret credentials med nøgle + Hmac algoritme
        var credientials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // opretter selve JWT token med alle details
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credientials
        );

        // convert jwt token til string (vi sender den her til client)
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
