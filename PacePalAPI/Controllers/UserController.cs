using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PacePalAPI.Controllers.Middleware;
using PacePalAPI.Extensions;
using PacePalAPI.Models;
using PacePalAPI.Models.Enums;
using PacePalAPI.Requests;
using PacePalAPI.Services.UserSearchService;
using PacePalAPI.Services.UserService;
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

                int userId = HttpContextExtensions.GetUserId(HttpContext) ?? throw new UnauthorizedAccessException();

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

                byte[] imageBytes = Convert.FromBase64String(updateUserDto.ImageData);

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
        public async Task<List<SearchUserDto>> SearchUser(string query)
        {
            int userSearchingId = HttpContextExtensions.GetUserId(HttpContext) ?? throw new UnauthorizedAccessException();
            UserModel user = await _userCollectionService.Get(userSearchingId) ?? throw new UnauthorizedAccessException();

            List<(int userId, int commonFriendsNum)> foundUsers = _userSearchService.SearchUsers(query, (user.Id, user.City, user.Country));

            List<UserModel?> userModels = await FetchUsersAsync(foundUsers);
            if (userModels == null || userModels.Count == 0) return new List<SearchUserDto>();

            return await MapToSearchUserDto(userSearchingId, foundUsers, userModels);
        }

        [HttpGet("getFriendRequests")]
        public async Task<List<FriendshipDto>> GetFriendshipRequests()
        {
            // Retrieve the user's ID from claims
            int userId = HttpContextExtensions.GetUserId(HttpContext) ?? throw new UnauthorizedAccessException();

            List<FriendshipModel>? friendships = await _userCollectionService.GetFriendshipRequests(userId);

            if (friendships == null) return new List<FriendshipDto>();

            var friendshipDtos = new List<FriendshipDto>();

            foreach (var model in friendships)
            {
                if (model.Status != EFriendshipState.Pending) continue;
                // Fetch the requester's full name asynchronously
                var requester = await _userCollectionService.Get(model.RequesterId);
                if (requester == null) continue;
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
        public async Task<IActionResult> SendFriendRequest(int recivId)
        {
            // Retrieve the user's ID from claims
            int reqId = HttpContextExtensions.GetUserId(HttpContext) ?? throw new UnauthorizedAccessException();

            int result = await _userCollectionService.SendFriendRequest(reqId, recivId);

            UserModel? requester = await _userCollectionService.Get(reqId);
            if (requester == null) return BadRequest("User " + reqId + " does not exist");

            string reqName = requester.FirstName + " " + requester.LastName;

            if (result == -1) return BadRequest("Friendship request already exists or you are already friends.");

            // Create a message object to send as JSON
            var message = new
            {
                requesterId = reqId,
                recieverId = recivId,
                id = result,
                requesterName = reqName,
            };

            // Serialize the object to JSON
            string jsonMessage = JsonSerializer.Serialize(message);

            // Send the JSON message to the receiver using WebSocket
            await _webSocketManager.SendMessageAsync(recivId.ToString(), jsonMessage);

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

        // Utils Methods    
        private async Task<List<UserModel?>> FetchUsersAsync(List<(int userId, int commonFriendsNum)> foundUsers)
        {
            var userTasks = foundUsers.Select(pair => _userCollectionService.Get(pair.userId));
            return (await Task.WhenAll(userTasks)).ToList();
        }

        private async Task<List<SearchUserDto>> MapToSearchUserDto(
            int userSearchingId,
            List<(int userId, int commonFriendsNum)> foundUsers,
            List<UserModel?> userModels)
        {
            var dtoTasks = userModels
                .Select(async (user, index) =>
                {
                    if (user == null) return null;

                    var friendshipStatus = await _userCollectionService.GetFriendshipStatus(userSearchingId, user.Id);
                    var profilePicture = await _userCollectionService.GetProfilePicture(user.Id);

                    return new SearchUserDto
                    {
                        Id = user.Id,
                        Name = $"{user.FirstName} {user.LastName}",
                        City = user.City,
                        Country = user.Country,
                        CommonFriends = foundUsers[index].commonFriendsNum,
                        FriendshipStatus = friendshipStatus,
                        ImageData = profilePicture ?? ""
                    };
                });

            var result = await Task.WhenAll(dtoTasks);
            return result.Where(dto => dto != null).ToList()!;
        }
    }
}
