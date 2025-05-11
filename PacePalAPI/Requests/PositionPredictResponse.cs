namespace PacePalAPI.Requests
{
    public class PositionPredictResponse
    {
        public List<double> prediction { get; set; }
        public int points_processed { get; set; }
    }
}
