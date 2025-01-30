using System.ComponentModel.DataAnnotations;

namespace PacePalAPI.Requests
{
    public class TrackDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public byte[] GpxData { get; set; }
        [Required]
        public byte[] LogData { get; set; }
    }
}
