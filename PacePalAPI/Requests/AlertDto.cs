using System.ComponentModel.DataAnnotations;

namespace PacePalAPI.Requests
{
    public class AlertDto
    {
        [Required]
        public int AuthorId { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public DateTime ExpiresAt { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string AlertType { get; set; }
        [Required]
        public bool IsActive { get; set; }
        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }

        [Required]
        public string ImageData { get; set; }
    }
}
