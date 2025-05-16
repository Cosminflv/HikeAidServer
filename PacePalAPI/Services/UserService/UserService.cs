using Microsoft.EntityFrameworkCore;
using PacePalAPI.Models;
using PacePalAPI.Models.Enums;

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

            if(confirmedCurrentHike == null) return false;

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
    }
}
