namespace PacePalAPI.Requests
{
    public class TourDto
    {
        public int Id { get; set; }

        public int AuthorId { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public int Distance { get; set; }

        public int Duration { get; set; }

        public int TotalUp { get; set; }

        public int TotalDown { get; set; }

        public string PreviewImageUrl { get; set; } = string.Empty;

        public List<TourCoordinatesDto> Coordinates { get; set; } = new List<TourCoordinatesDto>();
    }
}
