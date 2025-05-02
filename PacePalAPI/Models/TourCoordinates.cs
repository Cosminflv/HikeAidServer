using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PacePalAPI.Models
{
    [Table("TourCoordinates")]
    public class TourCoordinates
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Tour))]
        public int TourId { get; set; }
        public TourModel Tour { get; set; } = null!;

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        public double? Speed { get; set; }

        [Required]
        public int Altitude { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }
    }
}
