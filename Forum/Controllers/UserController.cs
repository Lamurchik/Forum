using Forum.Model.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Forum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ForumDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserController> _logger;
        public UserController(ForumDBContext context, IConfiguration configuration, ILogger<UserController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(string loginName, string password)
        {
            if (_context.Users.Any(u => u.Username == loginName && u.Password == password))
            {
                return BadRequest("User already exists.");
            }
            var user = new User
            {
                Username = loginName,
                Password = password
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully.");
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(string loginName, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u=> u.Username==loginName&&u.Password==password);


            if (user == null) { return Unauthorized("Invalid username or password."); }

            var token = GenerateJwtToken(user, "User");
            _logger.LogInformation("проверка логера");
            return Ok(new { token });
        }
       
        private string GenerateJwtToken(User user, string role)
        {
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, role)
            };
            var q = _configuration["Jwt:Key"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
 }
    

