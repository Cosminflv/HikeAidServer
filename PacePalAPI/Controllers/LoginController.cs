using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PacePalAPI.Models;
using PacePalAPI.Requests;
using PacePalAPI.Services.UserService;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace PacePalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IUserCollectionService _userCollectionService;
        private readonly IPasswordHasher<UserModel> _hasher;
        private IConfiguration _config;

        public LoginController(IUserCollectionService userService, IConfiguration config, IPasswordHasher<UserModel> hasher)
        {
            _userCollectionService = userService ?? throw new ArgumentNullException(nameof(userService));
            _hasher = hasher;
            _config = config;
        }

        // Log in a user
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginInfo)
        {
            if (!ModelState.IsValid) return BadRequest("Invalid model state.");

            var users = await _userCollectionService.GetAll();

            if (users == null || !users.Any()) return NotFound("There are no users available.");

            var foundUser = users.FirstOrDefault(x => x.Username == loginInfo.Username);

            if (foundUser == null)
            {
                return BadRequest("Invalid username.");
            }

            var result = _hasher.VerifyHashedPassword(foundUser, foundUser.PasswordHash, loginInfo.Password);
            if (result == PasswordVerificationResult.Failed)
                return BadRequest("Invalid username or password.");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var tokenExpiry = DateTime.Now.AddMinutes(120);

            // Add claims to the token
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, foundUser.Id.ToString()), // Use the user's ID as the NameIdentifier
                new Claim(ClaimTypes.Name, foundUser.Username), // Optionally add username
    };

            var Sectoken = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              claims,
              expires: tokenExpiry,
              signingCredentials: credentials);

            var token = new JwtSecurityTokenHandler().WriteToken(Sectoken);

            var sessionId = Guid.NewGuid().ToString();
            var sessionData = new
            {
                Token = token,
                User = foundUser,
                ExpiresAt = tokenExpiry
            };

            // Store session (stateful)
            var userJson = JsonSerializer.Serialize(foundUser);
            HttpContext.Session.SetString("User", userJson);
       
            //HttpContext.Session.SetString("User", foundUser.ToString()!);
            //HttpContext.Session.SetString("UserId", foundUser.Id.ToString());
            //HttpContext.Session.SetString("Username", foundUser.Username);

            return Ok(new
            {
                SessionId = sessionId,
                Token = token,
                User = foundUser
            });
        }
        [HttpPost("register")]
        public IActionResult CreateUser([FromBody] UserDto userDto)
        {
            if(!ModelState.IsValid) return BadRequest("Invalid model state.");

            UserModel user = new UserModel();
            user.Username = userDto.Username;
            user.PasswordHash = userDto.PasswordHash;
            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.Bio = "";
            user.ProfilePictureUrl = "";
            user.City = userDto.City;
            user.Country = userDto.Country;
            user.Age = userDto.CalculateAge();
            user.Weight = userDto.Weight;
            user.Gender = userDto.EGender;
            user.BirthDate = userDto.Birthdate;

            user.PasswordHash = _hasher.HashPassword(user, userDto.PasswordHash);

            bool hasCreated = _userCollectionService.Create(user).Result;

            if (!hasCreated) return BadRequest("Username already exists.");

            return CreatedAtAction(nameof(CreateUser), new { id = user.Id }, user);
        }
    }
}
