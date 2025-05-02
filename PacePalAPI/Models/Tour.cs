using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PacePalAPI.Models
{
    [Table("Tours")]
    public class TourModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AuthorId { get; set; }

        [ForeignKey(nameof(AuthorId))]
        public UserModel Author { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int Distance { get; set; }

        [Required]
        public int Duration { get; set; }

        [Required]
        public int TotalUp { get; set; }

        [Required]
        public int TotalDown { get; set; }

        [Required]
        [MaxLength(500)]
        public string PreviewImageUrl { get; set; } = string.Empty;

        public ICollection<TourCoordinates> Coordinates { get; set; } = new List<TourCoordinates>();
    }
}
