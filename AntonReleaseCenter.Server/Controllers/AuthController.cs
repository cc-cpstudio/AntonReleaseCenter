using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AntonReleaseCenter.Core.Models;
using AntonReleaseCenter.Server.DbContexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AntonReleaseCenter.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _db;
    
    public AuthController(IConfiguration configuration, AppDbContext db)
    {
        _configuration = configuration;
        _db = db;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Admin admin)
    {
        var adminInDb = await _db.Admins.FirstOrDefaultAsync(a => a.Username == admin.Username);
        if (adminInDb is null) return Unauthorized("Invalid Username");
        if (admin.PasswordHash != adminInDb.PasswordHash) return Unauthorized("Invalid Password");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, admin.AdminId.ToString()),
            new Claim(ClaimTypes.Name, admin.Username),
            new Claim(ClaimTypes.Role, "Admin")
        };
        
        var jwt = _configuration.GetSection("Jwt");
        var secretKey = Encoding.UTF8.GetBytes(jwt["SecretKey"]!);
        var expiresMin = Convert.ToDouble(jwt["ExpiresMinutes"]);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = jwt["Issuer"],
            Audience = jwt["Audience"],
            Expires = DateTime.UtcNow.AddMinutes(expiresMin),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = tokenHandler.WriteToken(securityToken);

        return Ok(new { token = jwtToken });
    }
}