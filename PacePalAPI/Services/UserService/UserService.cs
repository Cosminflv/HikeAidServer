using Microsoft.EntityFrameworkCore;
using PacePalAPI.Models;
using PacePalAPI.Models.Enums;
using PacePalAPI.Requests;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Xml.Linq;

namespace PacePalAPI.Services.UserService
{
    public enum EFriendshipStatus
    {
        None,
        Friends,
        Pending
    }
    public class UserService : IUserCollectionService
    {
        private readonly PacePalContext _context;
        private readonly IDbContextFactory<PacePalContext> _contextFactory;
        private readonly IWebHostEnvironment _environment;
        private static readonly object _fileLock = new object();
        private static readonly HttpClient _httpClient = new HttpClient();

        public UserService(PacePalContext context, IDbContextFactory<PacePalContext> contextFactory, IWebHostEnvironment environment)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        public async Task<bool> AcceptFriendRequest(int requestId)
        {
            FriendshipModel? friendshipRequest = await _context.Friendships
                .FirstOrDefaultAsync(f => f.Id == requestId && f.Status == EFriendshipState.Pending);

            if (friendshipRequest == null) return false;

            // Update status to accepted
            friendshipRequest.Status = EFriendshipState.Accepted;
            _context.Friendships.Update(friendshipRequest);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeclineFriendRequest(int requestId)
        {
            FriendshipModel? friendshipRequest = await _context.Friendships
                .FirstOrDefaultAsync(f => f.Id == requestId && f.Status == EFriendshipState.Pending);

            if (friendshipRequest == null) return false;

            // Update status to accepted
            friendshipRequest.Status = EFriendshipState.Declined;
            _context.Friendships.Update(friendshipRequest);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteProfilePicture(int userId)
        {
            UserModel? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return false;

            string filePath = Path.Combine(_environment.WebRootPath, user.ProfilePictureUrl);

            //lock (_fileLock)
            //{
            if (!File.Exists(filePath) || user.ProfilePictureUrl.Contains("default")) return false;

            try
            {
                System.IO.File.Delete(filePath);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error deleting the file: {ex.Message}");
                return false;
            }

            user.ProfilePictureUrl = "uploads\\profile_pictures\\default.base64";

            _context.Users.Update(user);
            _context.SaveChanges();

            return true;
            //}
        }

        public async Task<byte[]> GetDefaultUserPicture()
        {
            string filePath = Path.Combine(_environment.WebRootPath, "uploads\\profile_pictures\\default.base64");

            return await System.IO.File.ReadAllBytesAsync(filePath);
        }

        public async Task<List<FriendshipModel>?> GetFriendshipRequests(int receiverId)
        {
            return await _context.Friendships.Where(u => u.ReceiverId == receiverId).ToListAsync();
        }

        public async Task<byte[]> GetProfilePicture(int userId)
        {
            UserModel user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId) ?? throw new InvalidOperationException();

            string filePath = Path.Combine(_environment.WebRootPath, user.ProfilePictureUrl);

            if (!System.IO.File.Exists(filePath)) return await GetDefaultUserPicture();

            return await System.IO.File.ReadAllBytesAsync(filePath);
        }
        public async Task<int> SendFriendRequest(int requesterId, int receiverId)
        {
            // Check if the friendship already exists
            var existingFriendship = await _context.Friendships
                .FirstOrDefaultAsync(f => (f.RequesterId == requesterId && f.ReceiverId == receiverId) ||
                                     (f.RequesterId == receiverId && f.ReceiverId == requesterId));

            if (existingFriendship != null) return -1;

            // Create a new friendship request
            var friendshipRequest = new FriendshipModel
            {
                RequesterId = requesterId,
                ReceiverId = receiverId,
                CreatedAt = DateTime.UtcNow,
                Status = EFriendshipState.Pending
            };

            await _context.Friendships.AddAsync(friendshipRequest);
            await _context.SaveChangesAsync();

            return friendshipRequest.Id;
        }

        public async Task<int> NumberOfFriends(int userId)
        {
            int number = await _context.Friendships
                .Where(f => (f.ReceiverId == userId || f.RequesterId == userId) && f.Status == EFriendshipState.Accepted)
                .CountAsync();

            return number;
        }

        public async Task<bool> UploadProfilePicture(int userId, byte[] imageData)
        {
            UserModel? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return false;

            var (filePath, fileName) = CreateUserImageFilePath(userId);

            string base64String = Convert.ToBase64String(imageData);

            string previousImageUrl = Path.Combine(_environment.WebRootPath, user.ProfilePictureUrl);

            // If the user has an existing profile picture, delete it
            if (File.Exists(previousImageUrl) && !user.ProfilePictureUrl.Contains("default")) System.IO.File.Delete(previousImageUrl);

            File.WriteAllText(filePath, base64String);

            var profilePictureUrl = $"uploads\\profile_pictures\\{fileName}";

            user.ProfilePictureUrl = profilePictureUrl;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> Create(UserModel model)
        {
            UserModel? foundUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);

            if (foundUser != null) return false;

            await _context.Users.AddAsync(model);
            _context.SaveChanges();
            return true;
        }

        public async Task<bool> Delete(int id)
        {
            UserModel? userToDelete = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

            if (userToDelete == null) return false;

            string filePath = Path.Combine(_environment.WebRootPath, userToDelete.ProfilePictureUrl);

            if (File.Exists(filePath) && !userToDelete.ProfilePictureUrl.Contains("default")) System.IO.File.Delete(filePath);

            _context.Users.Remove(userToDelete);
            return true;
        }

        public async Task<UserModel?> Get(int id)
        {
            using (var factoryContext = _contextFactory.CreateDbContext()) // Creates a fresh instance
            {
                return await factoryContext.Users.FirstOrDefaultAsync(x => x.Id == id);
            }
        }

        public async Task<List<UserModel>?> GetAll()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<bool> Update(UserModel modifiedUser)
        {
            _context.Attach(modifiedUser);

            // Mark it as Modified so that EF Core will update all properties.
            _context.Entry(modifiedUser).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return true;
        }

        private (string, string) CreateUserImageFilePath(int userId)
        {
            string fileName = $"{userId}_{Guid.NewGuid()}.base64";

            string uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "profile_pictures");

            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            string filePath = Path.Combine(uploadPath, fileName);

            return (filePath, fileName);
        }

        public async Task<EFriendshipStatus> GetFriendshipStatus(int user1, int user2)
        {
            FriendshipModel? friendshipRequest = null;
            using (var factoryContext = _contextFactory.CreateDbContext())
            {
                friendshipRequest = await factoryContext.Friendships
                    .FirstOrDefaultAsync(f => (f.ReceiverId == user1 && f.RequesterId == user2) || (f.ReceiverId == user2 && f.RequesterId == user1));
            }

            EFriendshipStatus status = EFriendshipStatus.None;

            if (friendshipRequest == null) return status;


            switch (friendshipRequest.Status)
            {
                case EFriendshipState.Accepted:
                    status = EFriendshipStatus.Friends;
                    break;
                case EFriendshipState.Declined:
                    status = EFriendshipStatus.None;
                    break;
                case EFriendshipState.Pending:
                    status = EFriendshipStatus.Pending;
                    break;
            }

            return status;
        }

        public async Task<bool> AddProgressCoordinates(int userId, List<Coordinate> coordinates)
        {
            ConfirmedCurrentHike? confirmedCurrentHike = await _context.ConfirmedCurrentHikes
                .FirstOrDefaultAsync(h => h.UserId == userId && h.IsActive);

            if (confirmedCurrentHike == null) return false;

            // Add the coordinates to the existing list
            confirmedCurrentHike.UserProgressCoordinates.AddRange(coordinates);
            // Save the changes to the database
            _context.ConfirmedCurrentHikes.Update(confirmedCurrentHike);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ConfirmHike(int userId, List<Coordinate> trackCoordinates)
        {
            // Check if user exists using async method
            UserModel? userModel = await _context.Users.FindAsync(userId);
            if (userModel == null) return false;

            // Create transaction for atomic operations
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Efficiently deactivate all existing active hikes for this user
                await _context.ConfirmedCurrentHikes
                    .Where(h => h.UserId == userId && h.IsActive)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(h => h.IsActive, false));

                // Create and add new active hike
                var newHike = new ConfirmedCurrentHike
                {
                    UserId = userId,
                    TrackCoordinates = trackCoordinates,
                    IsActive = true
                };

                await _context.ConfirmedCurrentHikes.AddAsync(newHike);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw; // Or handle exception as needed
            }
        }

        public async Task<ConfirmedCurrentHike> GetActiveHike(int userId)
        {
            var userWithHikes = await _context.Users
                    .Include(u => u.ConfirmedCurrentHikes)
                    .FirstOrDefaultAsync(u => u.Id == userId);

            if (userWithHikes == null)
                throw new InvalidOperationException($"Unable to retrieve user; no user with ID '{userId}'.");


            ConfirmedCurrentHike? activeHike = userWithHikes.ConfirmedCurrentHikes.FirstOrDefault(hike => hike.IsActive == true);

            if (activeHike == null)
                throw new InvalidOperationException($"Unable to retrieve active hike; no active hike for user with ID '{userId}'.");

            return activeHike;
        }

        public async Task<bool> UpdateHikeProgress(ConfirmedCurrentHike hike)
        {
            _context.ConfirmedCurrentHikes.Update(hike);

            var affected = await _context.SaveChangesAsync();
            return affected > 0;
        }

        public async Task<List<double>> PredictUserPosition(int userId)
        {
            // 1. Load the user’s track points
            var path = Path.Combine(_environment.ContentRootPath, "points.gpx");
            var trackPoints =  LoadTrackPointsFromGpx(path);
            if (trackPoints == null || !trackPoints.Any())
                throw new ArgumentException($"No track points found for user {userId}", nameof(userId));

            try
            {
                // 2. Serialize to JSON
                var json = JsonSerializer.Serialize(trackPoints);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                // 3. Call the Flask service
                var response = await _httpClient.PostAsync("http://localhost:5000/predict", content);

                // 4. Bubble up any non-success
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new InvalidOperationException(
                        $"Prediction service returned {(int)response.StatusCode}: {error}");
                }

                // 5. Deserialize, case‐insensitive
                var responseJson = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var result = JsonSerializer.Deserialize<PositionPredictResponse>(responseJson, options);

                if(result == null) return new List<double>(0);


                // 6. Return the raw list of doubles
                return result.prediction;
            }
            catch (JsonException jex)
            {
                // wrap or rethrow as desired
                throw new ApplicationException("Error parsing prediction response JSON.", jex);
            }
            catch (Exception ex) when (!(ex is ApplicationException))
            {
                throw new ApplicationException("Error calling prediction service.", ex);
            }
        }
        /// <summary>
        /// Loads track points from a GPX file and maps them to a list of TrackPointDto.
        /// </summary>
        /// <param name="filePath">The path to the GPX file.</param>
        /// <returns>List of TrackPointDto parsed from the GPX.</returns>
        private List<TrackPointDto> LoadTrackPointsFromGpx(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path must be provided.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("GPX file not found.", filePath);

            XDocument gpx;
            try
            {
                gpx = XDocument.Load(filePath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to load or parse the GPX file.", ex);
            }

            // GPX uses a default namespace, so we need to capture it
            XNamespace ns = gpx.Root.GetDefaultNamespace();

            // Extract all <trkpt> elements
            var trackPoints = gpx
                .Descendants(ns + "trkpt")
                .Select(element =>
                {
                    var latAttr = element.Attribute("lat")?.Value;
                    var lonAttr = element.Attribute("lon")?.Value;
                    var eleElem = element.Element(ns + "ele")?.Value;
                    var timeElem = element.Element(ns + "time")?.Value;

                    if (latAttr == null || lonAttr == null)
                        throw new FormatException("Track point missing latitude or longitude attributes.");

                    return new TrackPointDto
                    {
                        latitude = double.Parse(latAttr, System.Globalization.CultureInfo.InvariantCulture),
                        longitude = double.Parse(lonAttr, System.Globalization.CultureInfo.InvariantCulture),
                        elevation = eleElem != null
                            ? double.Parse(eleElem, System.Globalization.CultureInfo.InvariantCulture)
                            : 0,
                        time = timeElem != null
                            ? DateTimeOffset.Parse(timeElem, System.Globalization.CultureInfo.InvariantCulture)
                            : DateTimeOffset.MinValue
                    };
                })
                .ToList();

            return trackPoints;
        }

    }


}
