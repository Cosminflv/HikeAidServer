using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PacePalAPI.Requests;
using PacePalAPI.Services.UserService;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace PacePalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IUserCollectionService _userCollectionService;
        private IConfiguration _config;

        public LoginController(IUserCollectionService userService, IConfiguration config)
        {
            _userCollectionService = userService ?? throw new ArgumentNullException(nameof(userService));
            _config = config;
        }

        // Log in a user
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginInfo)
        {
            if (loginInfo == null || string.IsNullOrEmpty(loginInfo.Username) || string.IsNullOrEmpty(loginInfo.Password))
            {
                return BadRequest("Username and password are required.");
            }

            var users = await _userCollectionService.GetAll();

            if (users == null || !users.Any()) return NotFound("There are no users available.");

            var foundUser = users.FirstOrDefault(x => x.Username == loginInfo.Username && x.PasswordHash == loginInfo.Password);

            if (foundUser == null)
            {
                return BadRequest("Invalid username or password.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var Sectoken = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              null,
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: credentials);

            var token = new JwtSecurityTokenHandler().WriteToken(Sectoken);

            return Ok(new
            {
                Token = token,
                User = foundUser
            });
        }
    }
}
