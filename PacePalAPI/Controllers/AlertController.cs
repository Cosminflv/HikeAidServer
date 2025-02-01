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
    [Authorize]
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

        [HttpPost("AddAlert")]
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

                return Ok(result && hasUploaded);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
