using Microsoft.Extensions.Hosting;
using System.Text.Json.Serialization;

namespace PacePalAPI.Models
{
    public class CommentModel
    {
        public int Id { get; set; }  // Primary key
        public int PostId { get; set; }  // Foreign key
        public int UserId { get; set; }  // Foreign key
        public string Content { get; set; }
        public DateTime TimeStamp { get; set; }

        public CommentModel(int id, int postId, int userId, string content, DateTime createdAt) {
            this.Id = id;
            this.PostId = postId;
            this.UserId = userId;
            this.Content = content;
            this.TimeStamp = createdAt;
            this.Post = new SocialPostModel();
            this.User = new UserModel();
        }

        public CommentModel() { }

        // Navigation properties
        [JsonIgnore]
        public SocialPostModel Post { get; set; }
        [JsonIgnore]
        public UserModel User { get; set; }
    }
}
