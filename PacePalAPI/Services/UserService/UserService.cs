﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PacePalAPI.Models;

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

            user.ProfilePictureUrl = "uploads\\profile_pictures\\default.jpg";

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<byte[]> GetDefaultUserPicture()
        {
            string filePath = Path.Combine(_environment.WebRootPath, "uploads\\profile_pictures\\default.jpg");

            return await System.IO.File.ReadAllBytesAsync(filePath);
        }

        public async Task<List<FriendshipModel>?> GetFriendshipRequests()
        {
           return await _context.Friendships.ToListAsync();
        }

        public async Task<byte[]?> GetProfilePicture(int userId)
        {
            UserModel? userFound = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (userFound == null) return null;

            // Ensure the profile picture URL is a relative path
            var relativeProfilePictureUrl = userFound.ProfilePictureUrl.TrimStart('\\');


            string filePath = Path.Combine(_environment.WebRootPath, relativeProfilePictureUrl);

            if (!System.IO.File.Exists(filePath)) return null;

            return await System.IO.File.ReadAllBytesAsync(filePath);
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
        //TODO: REFACTOR IS UGLY AHH
        public async Task<bool> UploadProfilePicture(int userId, IFormFile file)
        {
            if (file == null || file.Length == 0) return false;

            // Ensure the file is an image
            string fileExtension = Path.GetExtension(file.FileName);
            if (!new[] { ".jpg", ".jpeg", ".png", ".gif" }.Contains(fileExtension.ToLower())) return false;

            // Create a unique file name
            string fileName = $"{userId}_{Guid.NewGuid()}{fileExtension}";

            // Define the path where the file will be saved
            var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "profile_pictures");

            if(!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            string filePath = Path.Combine(uploadPath, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var profilePictureUrl = $"\\uploads\\profile_pictures\\{fileName}";
            UserModel? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if(user == null) return false;

            // If the user has an existing profile picture, delete it
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                var existingFilePath = Path.Combine(_environment.WebRootPath, user.ProfilePictureUrl.TrimStart('/'));
                if (System.IO.File.Exists(existingFilePath))
                {
                    System.IO.File.Delete(existingFilePath);
                }
            }

            user.ProfilePictureUrl = profilePictureUrl;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }

        async Task<bool> ICollectionService<UserModel>.Create(UserModel model)
        {
            await _context.Users.AddAsync(model);
            _context.SaveChanges();
            return true;
        }

        async Task<bool> ICollectionService<UserModel>.Delete(int id)
        {
            UserModel? userToDelete = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

            if (userToDelete == null) return false;

            _context.Users.Remove(userToDelete);
            return true;
        }

        async Task<UserModel?> ICollectionService<UserModel>.Get(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
        }

        async Task<List<UserModel>?> ICollectionService<UserModel>.GetAll()
        {
            return await _context.Users.ToListAsync();
        }

        async Task<bool> ICollectionService<UserModel>.Update(int id, UserModel model)
        {
            UserModel? userToUpdate = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (userToUpdate == null) return false;
            _context.Users.Update(userToUpdate);
            return true;
        }
    }
}
