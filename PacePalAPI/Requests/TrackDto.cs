namespace PacePalAPI.Requests
{
    public class TrackDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public byte[] GpxData { get; set; }

        public byte[] LogData { get; set; }
    }
}
