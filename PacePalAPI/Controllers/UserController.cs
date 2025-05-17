using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PacePalAPI.Controllers.Middleware;
using PacePalAPI.Extensions;
using PacePalAPI.Models;
using PacePalAPI.Models.Enums;
using PacePalAPI.Requests;
using PacePalAPI.Services.UserSearchService;
using PacePalAPI.Services.UserService;
using System.Text;
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
        private readonly MyWebSocketManager _webSocketManager;

        //private static readonly HttpClient _httpClient = new HttpClient();

        public UserController(IUserCollectionService userService, IUserSearchService userSearchService, MyWebSocketManager webSocketManager)
        {
            _webSocketManager = webSocketManager;
            _userCollectionService = userService ?? throw new ArgumentNullException(nameof(userService));
            _userSearchService = userSearchService ?? throw new ArgumentNullException(nameof(_userSearchService));
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

                bool result1 = await _userCollectionService.Update(user);
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
                receiverId = recivId,
                id = result,
                requesterName = reqName,
            };

            // Send the JSON message to the receiver using WebSocket
            await EventsController.SendFriendshipRequest(recivId.ToString(), message);

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

        [HttpGet("{userId}/getProfilePicture")]

        public async Task<IActionResult> GetProfilePicture(int userId)
        {
            byte[] bytes = await _userCollectionService.GetProfilePicture(userId);

            return File(bytes, "image/jpeg");
        }

        [HttpPost("{userId}/predictDistance")]
        public async Task<IActionResult> PredictDistance([FromBody] int userId)
        {
            List<double> prediction = await _userCollectionService.PredictUserPosition(userId);

            if (prediction == null || prediction.Count == 0)
                return BadRequest("Prediction failed or no data available.");

            // Create a response object
            var response = new PositionPredictResponse
            {
                prediction = prediction,
                points_processed = prediction.Count
            };

            return Ok(response);
        }

        [HttpPost("confirmHike")]
        public async Task<IActionResult> ConfirmHike([FromBody] List<CoordinatesDto> hikeCoordinates)
        {
            if (hikeCoordinates == null || hikeCoordinates.Count == 0)
                return BadRequest("Track coordinates payload is empty or invalid.");

            // Retrieve the user's ID from claims
            int userId = HttpContextExtensions.GetUserId(HttpContext) ?? throw new UnauthorizedAccessException();


            List<Coordinate> coords = new List<Coordinate>();
            hikeCoordinates.ForEach(coord =>
            {
                coords.Add(new Coordinate
                {
                    Latitude = coord.Latitude,
                    Longitude = coord.Longitude
                });
            });

            bool result = await _userCollectionService.ConfirmHike(userId, coords);

            if (!result) return BadRequest("Error while confirming hike.");

            return Ok(result);
        }

        [HttpGet("{userId}/getUserConfirmedHike")]
        public async Task<IActionResult> GetUserConfirmedHike(int userId)
        {

            ConfirmedCurrentHike? hike = await _userCollectionService.GetActiveHike(userId);

            if (hike == null) return NotFound("No active hike found.");

            // Convert the coordinates to a list of CoordinatesDto
            List<CoordinatesDto> trackCoordinatesDtos = hike.TrackCoordinates
                .Select(coord => new CoordinatesDto
                {
                    Latitude = coord.Latitude,
                    Longitude = coord.Longitude
                })
                .ToList();
            // Convert the user progress coordinates to a list of CoordinatesDto
            List<CoordinatesDto> userProgressDto = hike.UserProgressCoordinates
                .Select(coord => new CoordinatesDto
                {
                    Latitude = coord.Latitude,
                    Longitude = coord.Longitude
                })
                .ToList();
            // Create a response object
            HikeDto response = new HikeDto
            {
                TrackCoordinates = trackCoordinatesDtos,
                UserProgressCoordinates = userProgressDto,
                LastCoordinateTimeStamp = hike.UserProgressCoordinates.Last().Timestamp

            };

            return Ok(response);
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

                    return new SearchUserDto
                    {
                        Id = user.Id,
                        Name = $"{user.FirstName} {user.LastName}",
                        City = user.City,
                        Country = user.Country,
                        CommonFriends = foundUsers[index].commonFriendsNum,
                        FriendshipStatus = friendshipStatus,
                    };
                });

            var result = await Task.WhenAll(dtoTasks);
            return result.Where(dto => dto != null).ToList()!;
        }
    }
}
