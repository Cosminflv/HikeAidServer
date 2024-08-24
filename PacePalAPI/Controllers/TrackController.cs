using Microsoft.AspNetCore.Mvc;
using PacePalAPI.Requests;
using PacePalAPI.Services.TrackService;

namespace PacePalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrackController : ControllerBase
    {
        private readonly ITrackCollectionService _trackCollectionService;

        public TrackController(ITrackCollectionService trackCollectionService)
        {
            _trackCollectionService = trackCollectionService ?? throw new ArgumentNullException(nameof(trackCollectionService));
        }

        [HttpGet("{id}/userTracks")]
        public async Task<List<TrackDto>> GetUserTracks(int userId)
        {
            List<TrackDto>? recordedTracks = await _trackCollectionService.GetUserRecordedTracks(userId);

            if (recordedTracks == null) return new List<TrackDto>();

            return recordedTracks;
        }

        [HttpPost("{id}/uploadTrack")]
        public async Task<IActionResult> UploadTrackBase64(TrackDto track)
        {
            bool result = await _trackCollectionService.UploadTrackBase64(track.UserId, track.GpxData, track.LogData);

            if (!result) return BadRequest("Error while uploading track."); ;

            return Ok(true);  
        }
    }
}
