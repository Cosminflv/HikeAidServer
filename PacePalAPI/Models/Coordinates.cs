using System.Text.Json.Serialization;

namespace PacePalAPI.Models
{
    public class Coordinate
    {
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Elevation { get; set; }
        public DateTime Timestamp { get; set; }

        // Foreign keys
        public int? TrackCoordinatesConfirmedCurrentHikeId { get; set; }
        public int? UserProgressCoordinatesConfirmedCurrentHikeId { get; set; }

        // Navigation properties
        [JsonIgnore]
        public ConfirmedCurrentHike TrackCoordinatesConfirmedCurrentHike { get; set; }

        [JsonIgnore]
        public ConfirmedCurrentHike UserProgressCoordinatesConfirmedCurrentHike { get; set; }
    }
}