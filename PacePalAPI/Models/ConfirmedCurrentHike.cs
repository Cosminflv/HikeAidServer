using System.Text.Json.Serialization;

namespace PacePalAPI.Models
{
    public class ConfirmedCurrentHike
    {
        public int Id { get; set; } // Primary key
        public bool IsActive { get; set; }

        // List of coordinates representing the hike's path
        public List<Coordinate> TrackCoordinates { get; set; } = new List<Coordinate>();

        public List<Coordinate> UserProgressCoordinates { get; set; } = new List<Coordinate>();

        // Foreign key to UserModel
        public int UserId { get; set; }

        // Navigation property (EF Core relationship)
        [JsonIgnore]
        public UserModel User { get; set; }
    }
}