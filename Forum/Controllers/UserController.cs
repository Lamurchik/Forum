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
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Проверяем, существует ли пользователь с таким логином и паролем
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginName && u.Password == password);
                if (existingUser != null)
                {
                    return BadRequest("User already exists.");
                }

                // Создаем нового пользователя
                var user = new User
                {
                    Username = loginName,
                    Password = password
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Завершаем транзакцию
                await transaction.CommitAsync();

                // Логируем успешную регистрацию пользователя
                _logger.LogInformation($"New user registered: {loginName}");

                return Ok("User registered successfully.");
            }
            catch (Exception ex)
            {
                // Откатываем транзакцию в случае ошибки
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while registering user: {Username}", loginName);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(string loginName, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u=> u.Username==loginName&&u.Password==password);


            if (user == null) 
            {
                _logger.LogInformation("unsuccessful login attempt");
                return Unauthorized("Invalid username or password."); 
            }

            var token = GenerateJwtToken(user, "User");
            _logger.LogInformation("User logged in system");
            return Ok(new { token, user.UserId });
        }
       
        private string GenerateJwtToken(User user, string role)
        {
            var claims = new[]
            {
           // new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())

            };
            //var q = _configuration["Jwt:Key"];
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
    

