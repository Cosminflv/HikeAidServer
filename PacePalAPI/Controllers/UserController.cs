using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using PacePalAPI.Models;
using PacePalAPI.Requests;
using PacePalAPI.Services.UserService;

namespace PacePalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserCollectionService _userCollectionService;

        public UserController(IUserCollectionService userService)
        {
            _userCollectionService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        // Get all users
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userCollectionService.GetAll();

            if (users == null || !users.Any()) return NotFound("There are no users available.");

            return Ok(users);
        }

        // Create a new user
        [HttpPost]
        public IActionResult CreateUser([FromBody] UserModel user)
        {
            if (user == null)
            {
                return BadRequest("The user cannot be null.");
            }

            _userCollectionService.Create(user);
            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user);
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
                return Unauthorized("Invalid username or password.");
            }

            return Ok(foundUser);
        }
    }
}
