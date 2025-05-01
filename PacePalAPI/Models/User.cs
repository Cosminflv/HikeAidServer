using Microsoft.Extensions.Hosting;
using PacePalAPI.Models.Enums;
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
        public string Bio { get; set; }
        public int Age { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public int Weight { get; set; } 
        public EGender Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string ProfilePictureUrl { get; set; }

        public UserModel() { }

        // Navigation properties
        [JsonIgnore]
        public List<FriendshipModel> SentFriendships { get; set; } = new List<FriendshipModel>(); // Friendships where user is the requester

        [JsonIgnore]
        public List<FriendshipModel> ReceivedFriendships { get; set; } = new List<FriendshipModel>(); // Friendships where user is the receiver
        [JsonIgnore]
        public List<SocialPostModel> Posts { get; set; } = new List<SocialPostModel>();
        [JsonIgnore]
        public List<CommentModel> Comments { get; set; } = new List<CommentModel>();
        [JsonIgnore]
        public List<LikeModel> Likes { get; set; } = new List<LikeModel>();
        [JsonIgnore]
        public List<TourModel> RecordedTracks { get; set; } = new List<TourModel>();

        // Navigation properties
        [JsonIgnore]
        public List<Alert> CreatedAlerts { get; set; } = new List<Alert>(); // Alerts created by this user

        [JsonIgnore]
        public List<Alert> ConfirmedAlerts { get; set; } = new List<Alert>(); // Alerts confirmed by this user
    }
}
