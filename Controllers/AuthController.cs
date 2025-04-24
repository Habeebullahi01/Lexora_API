using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using lexora_api.Data;
using lexora_api.Models;
using lexora_api.Models.Dto.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace lexora_api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;
    private readonly AuthDbContext _authContext;

    public AuthController(UserManager<ApplicationUser> userManager, IConfiguration config, AuthDbContext authContext)
    {
        _userManager = userManager;
        _config = config;
        _authContext = authContext;
    }

    [HttpPost("register/reader")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<IEnumerable<IdentityError>>(StatusCodes.Status400BadRequest, "application/json")]
    public async Task<IActionResult> RegisterReader([FromBody] RegisterDto dto)
    {
        ApplicationUser user = new() { Email = dto.Email, UserName = dto.Username };
        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        // add the user to the reader role
        await _userManager.AddToRoleAsync(user, "Reader");

        // add the user profile to the db
        ReaderProfile newReader = new() { UserId = user.Id, User = user };
        await _authContext.Readers.AddAsync(newReader);
        await _authContext.SaveChangesAsync();

        return Ok("User registration succeeded");
    }
    [HttpPost("register/librarian")]
    public async Task<IActionResult> RegisterLibrarian([FromBody] RegisterDto dto)
    {
        ApplicationUser user = new() { Email = dto.Email, UserName = dto.Username };
        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        // add the user to the librarian role
        await _userManager.AddToRoleAsync(user, "Librarian");

        // add the user profile to the db
        LibrarianProfile newLibrarian = new() { UserId = user.Id, User = user };
        await _authContext.Librarians.AddAsync(newLibrarian);
        await _authContext.SaveChangesAsync();

        return Ok("User registration succeeded");
    }

    [HttpPost("login")]
    [ProducesResponseType<object>(StatusCodes.Status200OK, "application/json")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            return Unauthorized("Invalid Credentials");
        }
        string token = await GenerateJWT(user);
        return Ok(new { token });
    }

    private async Task<string> GenerateJWT(ApplicationUser user)
    {
        var role = await _userManager.GetRolesAsync(user);

        string stringRoles = string.Join(",", role);

        var claims = new[]
       {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Role, stringRoles),
            new Claim("role", stringRoles)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}