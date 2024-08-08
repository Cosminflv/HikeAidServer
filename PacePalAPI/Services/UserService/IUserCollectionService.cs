using PacePalAPI.Models;

namespace PacePalAPI.Services.UserService
{
    public interface IUserCollectionService : ICollectionService<UserModel>
    {
        //TODO More complex methods than CRUD
        Task<bool> LikePost(SocialPostModel post, LikeModel like);
        Task<bool> CommentPost(SocialPostModel post, CommentModel comment);
    }
}
