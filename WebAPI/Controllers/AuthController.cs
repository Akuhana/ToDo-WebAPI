using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebAPI.Models;
using WebAPI.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public AuthController(UserManager<ApplicationUser> users, IConfiguration config, ILogger<AuthController> logger)
        {
            _users = users;
            _config = config;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterVM vm)
        {
            _logger.LogInformation("Register attempt for {Email} at {Time}", vm.Email, DateTime.UtcNow);

            var user = new ApplicationUser { UserName = vm.Email, Email = vm.Email };
            var result = await _users.CreateAsync(user, vm.Password);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Registration failed for {Email}: {Errors}",
                                   vm.Email,
                                   string.Join("; ", result.Errors.Select(e => e.Description)));

                return BadRequest(new
                {
                    message = "Registration failed",
                    errors = result.Errors.Select(e => e.Description)
                });
            }

            _logger.LogInformation("Registration successful for {Email}", vm.Email);
            return Ok(new { message = "Registration successful" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginVM vm)
        {
            var user = await _users.FindByEmailAsync(vm.Email);
            if (user == null || !await _users.CheckPasswordAsync(user, vm.Password))
                return Unauthorized(new { message = "Invalid credentials" });

            // create JWT
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });
        }

        /// <summary>
        /// Checks if the current JWT token is valid.
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("validate")]
        public async Task<IActionResult> Validate([FromServices] UserManager<ApplicationUser> users)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("User id claim missing");
            var user = await users.FindByIdAsync(userId!);

            // Check if user exists
            if (user is null)
                return Unauthorized(new { message = "User no longer exists" });

            return Ok(new { message = "Token is valid and user exists" });
        }

    }
}