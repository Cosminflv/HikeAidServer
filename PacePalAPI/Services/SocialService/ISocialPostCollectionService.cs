using PacePalAPI.Models;
using PacePalAPI.Requests;

namespace PacePalAPI.Services.SocialService
{
    public interface ISocialPostCollectionService : ICollectionService<SocialPostModel>
    {
        // TODO MORE COMPLEX LOGIC THAN CRUD

        Task<List<SocialPostModel>> GetUserPosts(int userId);
        Task<List<CommentModel>> GetPostComments(int postId);
        Task<bool> LikePost(int postId, int userId);
        Task<bool> CommentPost(int postId, int userId, string content, DateTime timeStamp);

        Task<bool> UpdatePost(int id, string content, string imageUrl);

        Task<bool> RemoveLike(int likeId);
        Task<bool> DeleteComment(int commentId);
    }
}
