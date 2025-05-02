namespace PacePalAPI.Requests
{
    public class TourCoordinatesDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Speed { get; set; }
        public int Altitude { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
