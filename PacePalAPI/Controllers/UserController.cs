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
            List<UserModel> users = await _userCollectionService.GetAllUsers();
            return Ok(users);
        }

        [HttpPost]
        public IActionResult CreateUser([FromBody] UserModel user)
        {
            if(user == null)
            {
                return BadRequest("The user cannot be null.");
            }

            _userCollectionService.AddUser(user);
            return Ok(user);
        }

        [HttpPost("login/")]
        public IActionResult LogUser([FromBody] LoginInformation loginInfo)
        {
            if (loginInfo == null || string.IsNullOrEmpty(loginInfo.Username) || string.IsNullOrEmpty(loginInfo.Password))
            {
                return BadRequest("Username and password are required.");
            }

            UserModel? user = _userCollectionService.LogUser(loginInfo.Username, loginInfo.Password).Result;
            return Ok(user);
        }


    }
}
