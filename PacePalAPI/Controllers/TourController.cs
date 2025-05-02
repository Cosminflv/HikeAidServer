using Microsoft.AspNetCore.Mvc;
using PacePalAPI.Requests;
using PacePalAPI.Services.TrackService;

namespace PacePalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TourController : ControllerBase
    {
        private readonly ITourCollectionService _trackCollectionService;

        public TourController(ITourCollectionService trackCollectionService)
        {
            _trackCollectionService = trackCollectionService ?? throw new ArgumentNullException(nameof(trackCollectionService));
        }

        [HttpGet("{id}/userTours")]
        public async Task<List<TourDto>> GetUserTours(int userId)
        {
            List<TourDto>? recordedTracks = await _trackCollectionService.GetUserRecordedTours(userId);

            if (recordedTracks == null) return new List<TourDto>();

            return recordedTracks;
        }

        [HttpPost("{id}/uploadTour")]
        public async Task<IActionResult> UploadTourBase64(TourDto tour)
        {
            bool result = await _trackCollectionService.UploadTour(tour);

            if (!result) return BadRequest("Error while uploading tour."); ;

            return Ok(true);
        }
    }
}
