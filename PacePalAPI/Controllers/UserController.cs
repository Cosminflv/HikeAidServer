using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PacePalAPI.Models;
using PacePalAPI.Requests;
using PacePalAPI.Services.UserSearchService;
using PacePalAPI.Services.UserService;

namespace PacePalAPI.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserCollectionService _userCollectionService;
        private readonly IUserSearchService _userSearchService;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public UserController(IServiceScopeFactory serviceScopeFactory, IUserCollectionService userService, IUserSearchService userSearchService, IConfiguration config)
        {
            _userCollectionService = userService ?? throw new ArgumentNullException(nameof(userService));
            _userSearchService = userSearchService ?? throw new ArgumentNullException(nameof(_userSearchService));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
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
                user.Age = updateUserDto.Age;
                user.Gender = updateUserDto.Gender;
                user.BirthDate = updateUserDto.BirthDate;
                user.City = updateUserDto.City;
                user.Country = updateUserDto.Country;
                user.Weight = updateUserDto.Weight;

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
            //TODO Add those into UserDto by collecting data when user signs up
            user.Bio = "";
            user.ProfilePictureUrl = "";
            user.City = "";
            user.Country = "";
            user.Age = 0;
            user.Weight = 0;
            user.Gender = EGender.Man;
            user.BirthDate = DateTime.MinValue;

           bool hasCreated = _userCollectionService.Create(user).Result;

            if (!hasCreated) return BadRequest("Username already exists.");

            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user);
        }

        [HttpGet("searchUser")]
        public async Task<List<SearchUserDto>> SearchUser(string querry, int userSearchingId)
        {
            var user = await _userCollectionService.Get(userSearchingId);

            if (user == null)
            {
                return new List<SearchUserDto>();
            }

            List<(int userId, int commonFriendsNum)> foundUsers = _userSearchService.SearchUsers(querry, (user.Id, user.City, user.Country));

            var userTasks = foundUsers.Select(async pair =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var scopedUserCollectionService = scope.ServiceProvider.GetRequiredService<IUserCollectionService>();
                    return await scopedUserCollectionService.Get(pair.userId);
                }
            });

            // Use Task.WhenAll to fetch each user from _userCollectionService asynchronously
            var userResults = await Task.WhenAll(userTasks);

            if(userResults == null) return new List<SearchUserDto>();

            // Combine the fetched users with their common friends number
            List<(UserModel? user, int commonFriendsNum)> usersWithCommonFriends = userResults
                .Select((user, index) => (user, foundUsers[index].commonFriendsNum))
                .ToList();

            // Create a new scope for each asynchronous operation while mapping the users to SearchUserDto
            var users = await Task.WhenAll(usersWithCommonFriends.Select(async pair =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var scopedUserCollectionService = scope.ServiceProvider.GetRequiredService<IUserCollectionService>();
                    EFriendshipStatus friendshipStatus = (await scopedUserCollectionService.GetFriendshipStatus(userSearchingId, pair.user!.Id));

                    return new SearchUserDto
                    {
                        Id = pair.user!.Id,
                        Name = pair.user.FirstName + " " + pair.user.LastName,
                        City = pair.user.City,
                        Country = pair.user.Country,
                        CommonFriends = pair.commonFriendsNum,
                        FriendshipStatus = friendshipStatus!,
                        ImageData = (await scopedUserCollectionService.GetProfilePicture(pair.user.Id))!
                    };
                }
            }));

            return users.ToList();
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

        [HttpGet("/getDefaultProfilePicture")]
        public async Task<IActionResult> GetDefaultProfilePicture()
        {
            string? bytes = await _userCollectionService.GetDefaultUserPicture();

            if (bytes == null) return NotFound("Default image not found.");

            return Ok(bytes);
        }
    }
}
