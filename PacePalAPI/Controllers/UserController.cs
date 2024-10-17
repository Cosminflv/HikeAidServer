﻿using Microsoft.AspNetCore.Identity.Data;
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

        // Get user by id
        [HttpGet("{id}/getUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            UserModel? user = await _userCollectionService.Get(id);

            if (user == null) return NotFound("User not found");

            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                // Validate the input DTO (for example, ensure required fields are present)
                if (updateUserDto == null || updateUserDto.Id <= 0 ||
                    string.IsNullOrWhiteSpace(updateUserDto.FirstName) ||
                    string.IsNullOrWhiteSpace(updateUserDto.LastName))
                {
                    return BadRequest("Invalid input data");
                }

                UserModel? user = await _userCollectionService.Get(updateUserDto.Id);
                if (user == null) return NotFound("User not found");

                user.FirstName = updateUserDto.FirstName;
                user.LastName = updateUserDto.LastName;
                user.Bio = updateUserDto.Bio;

                byte[] imageBytes = Convert.FromBase64String(updateUserDto.imageData);

                bool result1 = await _userCollectionService.Update(updateUserDto.Id, user);
                bool result2 = false;

                if (updateUserDto.hasDeletedImage)
                {
                   result2 = await _userCollectionService.DeleteProfilePicture(updateUserDto.Id);
                }
                else
                {
                   result2 = await _userCollectionService.UploadProfilePicture(updateUserDto.Id, imageBytes);
                }

                return Ok(result1 && result2);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // Create a new user
        [HttpPost]
        public IActionResult CreateUser([FromBody] UserDto userDto)
        {
            if (userDto == null)
            {
                return BadRequest("The user cannot be null.");
            }

            UserModel user = new UserModel();
            user.Username = userDto.Username;
            user.PasswordHash = userDto.PasswordHash;
            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.Bio = "";
            user.ProfilePictureUrl = "";

           bool hasCreated = _userCollectionService.Create(user).Result;

            if (!hasCreated) return BadRequest("Username already exists.");

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
                return BadRequest("Invalid username or password.");
            }

            return Ok(foundUser);
        }

        [HttpGet("getFriendRequests")]
        public async Task<List<FriendshipModel>> GetFriendshipRequests()
        {
            List<FriendshipModel>? friendships = await _userCollectionService.GetFriendshipRequests();

            if(friendships == null) return new List<FriendshipModel>();

            return friendships;
        }

        [HttpPost("sendFriendRequest")]
        public async Task<IActionResult> SendFriendRequest(int requesterId, int receiverId)
        {
            bool result = await _userCollectionService.SendFriendRequest(requesterId, receiverId);

            if (!result) return BadRequest("Friendship request already exists or you are already friends.");

            return Ok(result);
        }

        [HttpGet("{id}/friendsNumber")]
        public async Task<int> GetFriendsNumber(int userId)
        {
            int number = await _userCollectionService.NumberOfFriends(userId);

            return number;
        }

        [HttpPost("acceptFriendRequest")]
        public async Task<IActionResult> AcceptFriendRequest(int requestId)
        {
            bool result = await _userCollectionService.AcceptFriendRequest(requestId);

            if (!result) return BadRequest("Friendship request not found or already accepted.");

            return Ok(result);
        }

        [HttpPost("declineFriendRequest")]
        public async Task<IActionResult> DeclineFriendRequest(int requestId)
        {
            bool result = await _userCollectionService.DeclineFriendRequest(requestId);

            if (!result) return BadRequest("Friendship request not found or already processed.");

            return Ok(result);
        }

        [HttpPost("{id}/uploadProfilePictureBase64")]
        public async Task<IActionResult> UploadProfilePicture(int userId, byte[] file)
        {
            bool result = await _userCollectionService.UploadProfilePicture(userId, file);

            if (!result) return BadRequest("Error while uploading picture.");

            return Ok(result);
        }

        [HttpPost("{id}/deleteProfilePicture")]
        public async Task<IActionResult> DeleteProfilePicture(int userId)
        {
            bool result = await _userCollectionService.DeleteProfilePicture(userId);

            if (!result) return NotFound("User not found.");

            return Ok(result);
        }

        [HttpGet("{id}/getProfilePicture")]
        public async Task<string> GetProfilePicture(int userId)
        {
            string? bytes = await _userCollectionService.GetProfilePicture(userId);

            if (bytes == null) return await _userCollectionService.GetDefaultUserPicture();

            return bytes!;
        }
    }
}
