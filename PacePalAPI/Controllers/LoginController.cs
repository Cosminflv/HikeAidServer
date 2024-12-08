﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PacePalAPI.Models;
using PacePalAPI.Requests;
using PacePalAPI.Services.UserService;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;

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
            var tokenExpiry = DateTime.Now.AddMinutes(120);

            var Sectoken = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              null,
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
            user.City = userDto.City;
            user.Country = userDto.Country;
            user.Age = userDto.CalculateAge();
            user.Weight = userDto.Weight;
            user.Gender = userDto.EGender;
            user.BirthDate = userDto.Birthdate;

            bool hasCreated = _userCollectionService.Create(user).Result;

            if (!hasCreated) return BadRequest("Username already exists.");

            return CreatedAtAction(nameof(CreateUser), new { id = user.Id }, user);
        }
    }
}
