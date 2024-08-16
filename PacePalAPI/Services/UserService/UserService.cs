using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PacePalAPI.Models;

namespace PacePalAPI.Services.UserService
{
    public class UserService : IUserCollectionService
    {
        private readonly PacePalContext _context;

        public UserService(PacePalContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
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

        public async Task<List<FriendshipModel>?> GetFriendshipRequests()
        {
           return await _context.Friendships.ToListAsync();
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
