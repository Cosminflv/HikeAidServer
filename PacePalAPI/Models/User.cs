using Microsoft.Extensions.Hosting;
using System.Text.Json.Serialization;

namespace PacePalAPI.Models
{

    public class UserModel
    {
        public int Id { get; set; }  // Primary key
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PasswordHash { get; set; }
        public string ProfilePictureUrl { get; set; }
        public string Bio { get; set; }

        public UserModel() { }

        // Navigation properties
        [JsonIgnore]
        public List<FriendshipModel> Friendships { get; set; } = new List<FriendshipModel>();
        [JsonIgnore]
        public List<SocialPostModel> Posts { get; set; } = new List<SocialPostModel>();
        [JsonIgnore]
        public List<CommentModel> Comments { get; set; } = new List<CommentModel>();
        [JsonIgnore]
        public List<LikeModel> Likes { get; set; } = new List<LikeModel>();
    }
}
