using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TodoListBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace TodoListBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }


        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            Console.WriteLine("Received registration request for: " + model.Username);

            if (!ModelState.IsValid)
            {
                Console.WriteLine("Model state is invalid");
                return BadRequest(ModelState);
            }

            var user = new User
            {
                UserName = model.Username,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                Console.WriteLine("User creation failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                return BadRequest(result.Errors);
            }

            Console.WriteLine("User registered successfully: " + user.UserName);
            return Ok(new { message = "User registered successfully" });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginDto)
        {
            if (loginDto == null || string.IsNullOrWhiteSpace(loginDto.Username) || string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return BadRequest(new { message = "Username and password are required" });
            }

            var normalizedUserName = loginDto.Username.ToUpper(); //Normalize before querying

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName); // ✅ Use NormalizedUserName

            if (user == null)
            {
                Console.WriteLine("User not found");
                return Unauthorized(new { message = "Invalid username" });
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!isPasswordValid)
            {
                Console.WriteLine("Password mismatch for user: " + loginDto.Username);
                return Unauthorized(new { message = "Invalid password" });
            }

            var token = GenerateJwtToken(user);
            Console.WriteLine("Login successful. Token: " + token);
            return Ok(new { token = await token });
        }



        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized(new { message = "User ID not found in token" });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound(new { message = "User not found" });

            return Ok(new { user.UserName, user.Email, user.PhoneNumber });
        }




        private async Task<string> GenerateJwtToken(User user)
        {
            Console.WriteLine("Received Token: " + Request.Headers["Authorization"]);

            if (string.IsNullOrEmpty(user.Id))
            {
                throw new Exception("User ID is null. Ensure user is properly retrieved before generating JWT.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? "")
            };

            // Add roles to claims
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}