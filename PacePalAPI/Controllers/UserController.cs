using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using PacePalAPI.Models;
using PacePalAPI.Requests;
using PacePalAPI.Services.UserService;

namespace PacePalAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        IUserCollectionService _userCollectionService;

        public UserController(IUserCollectionService userService)
        {
            _userCollectionService = userService ?? throw new ArgumentNullException(nameof(UserService));
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            List<UserModel>? users = await _userCollectionService.GetAll();
            return Ok(users);
        }

        [HttpPost]
        public IActionResult CreateUser([FromBody] UserModel user)
        {
            if(user == null)
            {
                return BadRequest("The user cannot be null.");
            }

            _userCollectionService.Create(user);
            return Ok(user);
        }

        [HttpPost("login/")]
        public async Task<IActionResult> LogUser([FromBody] LoginInformation loginInfo)
        {
            if (loginInfo == null || string.IsNullOrEmpty(loginInfo.Username) || string.IsNullOrEmpty(loginInfo.Password))
            {
                return BadRequest("Username and password are required.");
            }

            List<UserModel>? users = await _userCollectionService.GetAll();

            if (users == null || users.Count == 0) return BadRequest("There are no users available.");

            UserModel? foundUser = users.FirstOrDefault(x => x.Username == loginInfo.Username && x.PasswordHash == loginInfo.Password);

            if (foundUser == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            return Ok(foundUser);
        }
    }
}
