using PacePalAPI.Models;
using PacePalAPI.Requests;

namespace PacePalAPI.Services.SocialService
{
    public interface ISocialPostCollectionService : ICollectionService<SocialPostModel>
    {
        // TODO MORE COMPLEX LOGIC THAN CRUD

        Task<List<SocialPostModel>> GetUserPosts(int userId);
        Task<bool> LikePost(int postId, int userId);
        Task<bool> RemoveLike(int likeId);
        Task<bool> CommentPost(int postId, int userId, string content, DateTime timeStamp);

        Task<List<CommentModel>> GetPostComments(int postId);

        Task<bool> DeleteComment(int commentId);
    }
}
