using Microsoft.Extensions.Hosting;
using System.Text.Json.Serialization;

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
        }

        public LikeModel()
        {

        }

        // Navigation properties
        [JsonIgnore]
        public SocialPostModel Post { get; set; }
        [JsonIgnore]
        public UserModel User { get; set; }
    }
}
