namespace PacePalAPI.Requests
{
    public class TrackPointDto
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
        public double elevation { get; set; }
        public DateTimeOffset time { get; set; }
    }
}
