using Microsoft.Extensions.Hosting;

namespace PacePalAPI.Models
{
    public class LikeModel
    {
        public int Id { get; set; }  // Primary key
        public int PostId { get; set; }  // Foreign key
        public int UserId { get; set; }  // Foreign key

        public LikeModel(int id, int postId, int userId, SocialPostModel post, UserModel user)
        {
            Id = id;
            PostId = postId;
            UserId = userId;
            Post = post;
            User = user;
            Post = new SocialPostModel();
            User = new UserModel();
        }

        public LikeModel()
        {

        }

        // Navigation properties
        public SocialPostModel Post { get; set; }
        public UserModel User { get; set; }
    }
}
