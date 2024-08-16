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

        Task<bool> UploadProfilePicture(int userId, IFormFile file);

        Task<byte[]?> GetProfilePicture(int userId);

        Task<byte[]> GetDefaultUserPicture();
    }
}
