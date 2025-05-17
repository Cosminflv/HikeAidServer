using PacePalAPI.Models;

namespace PacePalAPI.Requests
{
    public class HikeDto
    {
        public DateTime LastCoordinateTimeStamp { get; set; }
        public List<CoordinatesDto> TrackCoordinates { get; set; } = new List<CoordinatesDto>();

        public List<CoordinatesDto> UserProgressCoordinates { get; set; } = new List<CoordinatesDto>();
    }
}
