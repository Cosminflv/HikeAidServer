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
        private readonly IWebHostEnvironment _environment;
        private static readonly object _fileLock = new object();

        public UserService(PacePalContext context, IWebHostEnvironment environment)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
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

        public async Task<string> GetDefaultUserPicture()
        {
            string filePath = Path.Combine(_environment.WebRootPath, "uploads\\profile_pictures\\default.base64");

            return await System.IO.File.ReadAllTextAsync(filePath);
        }

        public async Task<List<FriendshipModel>?> GetFriendshipRequests(int receiverId)
        {
           return await _context.Friendships.Where(u => u.ReceiverId == receiverId).ToListAsync();
        }

        public async Task<string?> GetProfilePicture(int userId)
        {
            UserModel? userFound = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (userFound == null) return null;

            string filePath = Path.Combine(_environment.WebRootPath, userFound.ProfilePictureUrl);

            //lock (_fileLock)
            //{
                try
                {
                    if (!System.IO.File.Exists(filePath)) return null;

                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (StreamReader reader = new StreamReader(fs))
                    {
                        return reader.ReadToEnd(); // Use synchronous read in the lock
                    }
                }
                catch (IOException ex)
                {
                    // Log the exception for debugging
                    Console.WriteLine($"Error reading the file: {ex.Message}");
                    return null;
                }
            //}
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
            UserModel? foundUser = await _context.Users.FirstOrDefaultAsync(u => u.Username ==  model.Username);

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

            if(File.Exists(filePath) && !userToDelete.ProfilePictureUrl.Contains("default")) System.IO.File.Delete(filePath);

            _context.Users.Remove(userToDelete);
            return true;
        }

        public async Task<UserModel?> Get(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<UserModel>?> GetAll()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<bool> Update(int id, UserModel model)
        {
            UserModel? userToUpdate = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (userToUpdate == null) return false;
            _context.Users.Update(userToUpdate);
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
            FriendshipModel? friendshipRequest = await _context.Friendships
                .FirstOrDefaultAsync(f => (f.ReceiverId == user1 && f.RequesterId == user2) || (f.ReceiverId == user2 && f.RequesterId == user1));

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
    }
}
