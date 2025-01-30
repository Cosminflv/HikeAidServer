using PacePalAPI.Models.Enums;
using System.Text.Json.Serialization;

namespace PacePalAPI.Models
{
    public class Alert
    {
        public int Id { get; set; }

        public int AuthorId { get; set; } // Foreign key reference

        public DateTime CreatedAt { get; set; } 

        public DateTime ExpiresAt { get; set; } 

        public string Title { get; set; }

        public string Description { get; set; }

        public EAlertType AlertType { get; set; } 

        public bool IsActive { get; set; } 

        public string ImageUrl { get; set; } 

        public int CoordinatesId { get; set; } // Foreign key reference

        public Coordinates LocationCoords { get; set; } //TODO: CHANGE COORDINATES WITH LATITUDE AND LONGITUDE

        public List<int> ConfirmedUserIds { get; set; }

        [JsonIgnore]
        public UserModel Author { get; set; }

        [JsonIgnore]
        public List<UserModel> ConfirmedUsers { get; set; } = new List<UserModel>(); // Users who confirmed this alert
    }
}
