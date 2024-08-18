namespace PacePalAPI.Models
{
    public class TrackModel
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string GpxFilePath { get; set; }

        public string LogFilePath { get; set; }

        public TrackModel() { }

        // Navigation proprieties

        public UserModel? UserModel { get; set; }
    }
}
