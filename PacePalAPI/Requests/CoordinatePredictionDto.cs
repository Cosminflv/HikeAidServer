﻿namespace PacePalAPI.Requests
{
    public class CoordinatePredictionDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Elevation { get; set; }
        public DateTime Time { get; set; }
    }
}
