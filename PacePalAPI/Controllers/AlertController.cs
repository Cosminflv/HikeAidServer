using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PacePalAPI.Controllers.Middleware;
using PacePalAPI.Converters;
using PacePalAPI.Extensions;
using PacePalAPI.Models;
using PacePalAPI.Requests;
using PacePalAPI.Services.AlertService;

namespace PacePalAPI.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AlertController : ControllerBase
    {
        private readonly MyWebSocketManager _webSocketManager;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IAlertCollectionService _alertCollectionService;

        public AlertController(MyWebSocketManager webSocketManager, IServiceScopeFactory serviceScopeFactory, IAlertCollectionService alertCollectionService)
        {
            _webSocketManager = webSocketManager ?? throw new ArgumentNullException(nameof(webSocketManager));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _alertCollectionService = alertCollectionService ?? throw new ArgumentNullException(nameof(alertCollectionService));
        }

        [HttpPost("addAlert/{alertId}")]
        public async Task<IActionResult> AddAlert([FromForm] AlertDto alertDto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest("Invalid model state.");

                int userId = HttpContextExtensions.GetUserId(HttpContext) ?? throw new UnauthorizedAccessException();
                Alert alert = AlertConverter.ToModel(alertDto, userId);

                bool result = await _alertCollectionService.AddAlert(alert);
                if (!result) return BadRequest("Error while adding alert.");

                bool hasUploaded = false;

                // Save image only if a file is provided
                if (alertDto.ImageFile != null)
                {
                    using var memoryStream = new MemoryStream();
                    await alertDto.ImageFile.CopyToAsync(memoryStream);
                    byte[] imageBytes = memoryStream.ToArray();

                    hasUploaded = await _alertCollectionService.UploadAlertImage(alert.Id, imageBytes);
                }

                //if (alertDto.ImageData != null)
                //{
                //    byte[] imageBytes = Convert.FromBase64String(alertDto.ImageData);
                //    hasUploaded = await _alertCollectionService.UploadAlertImage(alert.Id, imageBytes);
                //}

                return Ok(result && hasUploaded);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("GetAllAlerts")]
        public async Task<IActionResult> GetAllAlerts()
        {
            try
            {
                List<AlertDto> alerts = (await _alertCollectionService.GetAllAlerts())
                                        .AsParallel()
                                        .Select(a => AlertConverter.ToDto(a))
                                        .ToList();

                return Ok(alerts);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("{alertId}/image")]
        public async Task<IActionResult> GetAlertImage(int alertId)
        {
            try
            {
                byte[] imageData = await _alertCollectionService.GetAlertImageData(alertId);
                return File(imageData, "image/jpeg"); // Set correct MIME type
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
