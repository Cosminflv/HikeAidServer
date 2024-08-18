using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PacePalAPI.Models;
using System.Collections;

namespace PacePalAPI.Services.UserService
{
    public class UserService : IUserCollectionService
    {
        private readonly PacePalContext _context;
        private readonly IWebHostEnvironment _environment;

        public UserService(PacePalContext context, IWebHostEnvironment environment)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public async Task<bool> AcceptFriendRequest(int requestId)
        {
            FriendshipModel? friendshipRequest = await _context.Friendships
                .FirstOrDefaultAsync(f => f.Id == requestId && f.Status == FriendshipStatus.Pending);

            if (friendshipRequest == null) return false;

            // Update status to accepted
            friendshipRequest.Status = FriendshipStatus.Accepted;
            _context.Friendships.Update(friendshipRequest);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeclineFriendRequest(int requestId)
        {
            FriendshipModel? friendshipRequest = await _context.Friendships
                .FirstOrDefaultAsync(f => f.Id == requestId && f.Status == FriendshipStatus.Pending);

            if (friendshipRequest == null) return false;

            // Update status to accepted
            friendshipRequest.Status = FriendshipStatus.Declined;
            _context.Friendships.Update(friendshipRequest);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteProfilePicture(int userId)
        {
            UserModel? user = await _context.Users.FirstOrDefaultAsync(u => u.Id ==  userId);

            if (user == null) return false;

            string filePath = Path.Combine(_environment.WebRootPath, user.ProfilePictureUrl);

            if (!File.Exists(filePath) || user.ProfilePictureUrl.Contains("default")) return false;

            System.IO.File.Delete(filePath);

            user.ProfilePictureUrl = "uploads\\profile_pictures\\default.base64";

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string> GetDefaultUserPicture()
        {
            string filePath = Path.Combine(_environment.WebRootPath, "uploads\\profile_pictures\\default.base64");

            return await System.IO.File.ReadAllTextAsync(filePath);
        }

        public async Task<List<FriendshipModel>?> GetFriendshipRequests()
        {
           return await _context.Friendships.ToListAsync();
        }

        public async Task<string?> GetProfilePicture(int userId)
        {
            UserModel? userFound = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (userFound == null) return null;

            string filePath = Path.Combine(_environment.WebRootPath, userFound.ProfilePictureUrl);

            if (!System.IO.File.Exists(filePath)) return null;

            return await System.IO.File.ReadAllTextAsync(filePath);
        }

        public async Task<bool> SendFriendRequest(int requesterId, int receiverId)
        {
            // Check if the friendship already exists
            var existingFriendship = await _context.Friendships
                .FirstOrDefaultAsync(f => (f.RequesterId == requesterId && f.ReceiverId == receiverId) ||
                                     (f.RequesterId == receiverId && f.ReceiverId == requesterId));

            if (existingFriendship != null) return false;

            // Create a new friendship request
            var friendshipRequest = new FriendshipModel
            {
                RequesterId = requesterId,
                ReceiverId = receiverId,
                CreatedAt = DateTime.UtcNow,
                Status = FriendshipStatus.Pending
            };

            await _context.Friendships.AddAsync(friendshipRequest);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UploadProfilePicture(int userId, byte[] imageData)
        {
            UserModel? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return false;

            var (filePath, fileName) = _createFilePath(userId);

            string base64String = Convert.ToBase64String(imageData);

            // If the user has an existing profile picture, delete it
            if (File.Exists(filePath) && !user.ProfilePictureUrl.Contains("default")) System.IO.File.Delete(filePath);

            File.WriteAllText(filePath, base64String);

            var profilePictureUrl = $"uploads\\profile_pictures\\{fileName}";

            user.ProfilePictureUrl = profilePictureUrl;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> Create(UserModel model)
        {
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

        private (string, string) _createFilePath(int userId)
        {
            string fileName = $"{userId}_{Guid.NewGuid()}.base64";

            string uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "profile_pictures");

            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            string filePath = Path.Combine(uploadPath, fileName);

            return (filePath, fileName);
        }
    }
}
