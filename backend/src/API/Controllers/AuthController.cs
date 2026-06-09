using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using SistemaTraction.Application.Common.Interfaces;

namespace SistemaTraction.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthSettings authSettings, IWebHostEnvironment env) : ControllerBase
{
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (!BCrypt.Net.BCrypt.Verify(request.Password, authSettings.PasswordHash))
            return Unauthorized(new { error = "Credenciais inválidas" });

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authSettings.JwtSecret));
        var token = new JwtSecurityToken(
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        Response.Cookies.Append("auth_token", tokenString, new CookieOptions
        {
            HttpOnly = true,
            Secure = !env.IsDevelopment(),
            SameSite = env.IsDevelopment() ? SameSiteMode.Strict : SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            Path = "/"
        });

        return Ok(new { ok = true });
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("auth_token");
        return Ok();
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me() => Ok(new { authenticated = true });
}

public record LoginRequest(string Password);
