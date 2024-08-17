using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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

        Task<bool> UploadProfilePicture(int userId, byte[] imageData);

        Task<bool> DeleteProfilePicture(int userId);

        Task<string?> GetProfilePicture(int userId);

        Task<string> GetDefaultUserPicture();
    }
}
