using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PacePalAPI.Controllers.Middleware;
using PacePalAPI.Extensions;
using PacePalAPI.Models;
using PacePalAPI.Models.Enums;
using PacePalAPI.Requests;
using PacePalAPI.Services.UserSearchService;
using PacePalAPI.Services.UserService;
using System.Security.Claims;
using System.Text.Json;


namespace PacePalAPI.Controllers
{
    [Authorize]
    //[SessionCheck("User")]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserCollectionService _userCollectionService;
        private readonly IUserSearchService _userSearchService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly MyWebSocketManager _webSocketManager;

        public UserController(IServiceScopeFactory serviceScopeFactory, IUserCollectionService userService, IUserSearchService userSearchService, MyWebSocketManager webSocketManager)
        {
            _webSocketManager = webSocketManager;
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
            //var userJson = HttpContext.Session.GetString("User");

            //if (userJson == null) return NotFound("No active session found.");
            //UserModel user1;
            //if (userJson != null)
            //{
            //    user1 = JsonSerializer.Deserialize<UserModel>(userJson);
            //}


            //if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(username))
            //{
            //    return Unauthorized("No active session found.");
            //}
            
           

            UserModel? user = await _userCollectionService.Get(id);

            if (user == null) return NotFound("User not found");

            return Ok(user);
        }

        [HttpPut("updateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest("Invalid input data.");

                var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return BadRequest("Invalid user id");
                }

                if (!int.TryParse(userIdClaim.Value, out int userId))
                {
                    return BadRequest("Invalid user id");
                }

                UserModel? user = await _userCollectionService.Get(userId);
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

                bool result1 = await _userCollectionService.Update(userId, user);
                bool result2 = false;

                if (updateUserDto.hasDeletedImage)
                {
                   result2 = await _userCollectionService.DeleteProfilePicture(userId);
                }
                else
                {
                   result2 = await _userCollectionService.UploadProfilePicture(userId, imageBytes);
                }

                return Ok(result1 && result2);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("searchUser")]
        public async Task<List<SearchUserDto>> SearchUser(string querry)
        {
            // Retrieve the user's ID from claims
            int userSearchingId = HttpContextExtensions.GetUserId(HttpContext) ?? throw new UnauthorizedAccessException();

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
        public async Task<List<FriendshipDto>> GetFriendshipRequests()
        {
            // Retrieve the user's ID from claims
            int userId = HttpContextExtensions.GetUserId(HttpContext) ?? throw new UnauthorizedAccessException();

            List<FriendshipModel>? friendships = await _userCollectionService.GetFriendshipRequests(userId);

            if(friendships == null) return new List<FriendshipDto>();

            var friendshipDtos = new List<FriendshipDto>();

            foreach (var model in friendships)
            {
                if (model.Status != EFriendshipState.Pending) continue;
                // Fetch the requester's full name asynchronously
                var requester = await _userCollectionService.Get(model.RequesterId);
                if(requester == null) continue;
                var requesterName = $"{requester.FirstName} {requester.LastName}";

                // Map the FriendshipModel to FriendshipDto
                var dto = new FriendshipDto
                {
                    Id = model.Id,
                    receiverId = model.ReceiverId,
                    requesterId = model.RequesterId,
                    requesterName = requesterName,
                };

                friendshipDtos.Add(dto);
            }

            return friendshipDtos;
        }

        [HttpPost("sendFriendRequest")]
        public async Task<IActionResult> SendFriendRequest(int receiverId)
        {
            // Retrieve the user's ID from claims
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return BadRequest("Invalid user from claims.");
            }

            if (!int.TryParse(userIdClaim.Value, out int requesterId))
            {
                return BadRequest("Invalid user from claims.");
            }

            int result = await _userCollectionService.SendFriendRequest(requesterId, receiverId);

            UserModel? requester = await _userCollectionService.Get(requesterId);
            if(requester == null) return BadRequest("User " +  requesterId + " does not exist");
            UserModel? receiver = await _userCollectionService.Get(receiverId);
            if (receiver == null) return BadRequest("User " + receiverId + " does not exist");

            string requesterName = requester.FirstName + " " + requester.LastName;
            string receiverName = receiver.FirstName + " " + receiver.LastName;

            if (result == -1) return BadRequest("Friendship request already exists or you are already friends.");

            // Create a message object to send as JSON
            var message = new
            {
                requesterId = requesterId,
                receiverId = receiverId,
                id = result,
                requesterName = requesterName,
            };
            

            // Serialize the object to JSON
            string jsonMessage = JsonSerializer.Serialize(message);

            // Send the JSON message to the receiver using WebSocket
            await _webSocketManager.SendMessageAsync(receiverId.ToString(), jsonMessage);

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

        [HttpPost("uploadProfilePictureBase64")]
        public async Task<IActionResult> UploadProfilePicture(byte[] file)
        {
            // Retrieve the user's ID from claims
            int userId = HttpContextExtensions.GetUserId(HttpContext) ?? throw new UnauthorizedAccessException();

            bool result = await _userCollectionService.UploadProfilePicture(userId, file);

            if (!result) return BadRequest("Error while uploading picture.");

            return Ok(result);
        }

        [HttpPost("deleteProfilePicture")]
        public async Task<IActionResult> DeleteProfilePicture()
        {
            // Retrieve the user's ID from claims
            int userId = HttpContextExtensions.GetUserId(HttpContext) ?? throw new UnauthorizedAccessException();

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

        [HttpGet("getDefaultProfilePicture")]
        public async Task<IActionResult> GetDefaultProfilePicture()
        {
            string? bytes = await _userCollectionService.GetDefaultUserPicture();

            if (bytes == null) return NotFound("Default image not found.");

            return Ok(bytes);
        }
    }
}
