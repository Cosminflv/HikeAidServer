using Microsoft.Extensions.Hosting;

namespace PacePalAPI.Models
{
    public class CommentModel
    {
        public int Id { get; set; }  // Primary key
        public int PostId { get; set; }  // Foreign key
        public int UserId { get; set; }  // Foreign key
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }

        public CommentModel(int id, int postId, int userId, string content, DateTime createdAt) {
            this.Id = id;
            this.PostId = postId;
            this.UserId = userId;
            this.Content = content;
            this.CreatedAt = createdAt;
            this.Post = new SocialPostModel();
            this.User = new UserModel();
        }

        public CommentModel() { }

        // Navigation properties
        public SocialPostModel Post { get; set; }
        public UserModel User { get; set; }
    }
}
