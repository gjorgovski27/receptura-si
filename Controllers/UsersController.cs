using Microsoft.AspNetCore.Mvc;
using CookingAssistantAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CookingAssistantAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly CookingAssistantDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;

        public UsersController(CookingAssistantDbContext context, IConfiguration configuration)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
            _configuration = configuration;
        }

        // 1. Login Endpoint
        [HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
{
    if (string.IsNullOrEmpty(loginRequest.Email) || string.IsNullOrEmpty(loginRequest.Password))
        return BadRequest(new { Error = "Email and password are required." });

    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserEmail == loginRequest.Email);
    if (user == null)
        return Unauthorized(new { Error = "Invalid email or password." });

    var result = _passwordHasher.VerifyHashedPassword(user, user.UserPassword, loginRequest.Password);
    if (result != PasswordVerificationResult.Success)
        return Unauthorized(new { Error = "Invalid email or password." });

    // Generate JWT
    var token = GenerateJwtToken(user);
    return Ok(new
    {
        Token = token,
        User = new
        {
            user.UserId,
            user.UserName,
            user.UserFullName,
            user.UserEmail
        }
    });
}

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

        // 2. Signup Endpoint
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] User newUser)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _context.Users.AnyAsync(u => u.UserEmail == newUser.UserEmail);
            if (existingUser)
                return Conflict(new { Error = "Email is already registered." });

            // Hash the password before saving
            newUser.UserPassword = _passwordHasher.HashPassword(newUser, newUser.UserPassword);

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(SignUp), new { id = newUser.UserId }, new
            {
                newUser.UserId,
                newUser.UserName,
                newUser.UserFullName,
                newUser.UserEmail
            });
        }

        // Helper Method to Generate JWT Token
        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserEmail),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("FullName", user.UserFullName),
                new Claim("UserId", user.UserId.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
