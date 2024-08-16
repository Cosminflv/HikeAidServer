using Microsoft.AspNetCore.Mvc;
using PacePalAPI.Models;

namespace PacePalAPI.Services.UserService
{
    public interface IUserCollectionService : ICollectionService<UserModel>
    {
        //TODO More complex methods than CRUD

        Task<bool> SendFriendRequest(int requesterId, int recieverId);

        Task<bool> AcceptFriendRequest(int requestId);

        Task<bool> DeclineFriendRequest(int requestId);

        Task<List<FriendshipModel>?> GetFriendshipRequests();
    }
}
