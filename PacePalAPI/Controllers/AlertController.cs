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

        [HttpPost("addAlert")]
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

                if (alertDto.ImageFile != null)
                {
                    hasUploaded = await _alertCollectionService.UploadAlertImage(alert.Id, alertDto.ImageFile);
                }
                else
                {
                    hasUploaded = await _alertCollectionService.SetDefaultAlertImage(alert.Id);
                }


                //if (alertDto.ImageData != null)
                //{
                //    byte[] imageBytes = Convert.FromBase64String(alertDto.ImageData);
                //    hasUploaded = await _alertCollectionService.UploadAlertImage(alert.Id, imageBytes);
                //}

                var message = new
                {
                    alertId = alert.Id,
                    authorId = alert.AuthorId,
                    alertTitle = alert.Title,
                    alertDescription = alert.Description,
                    alertType = (int)alert.AlertType,
                    alertLatitude = alert.Latitude,
                    alertLongitude = alert.Longitude,
                    alertCreatedAt = alert.CreatedAt,
                    alertExpiresAt = alert.ExpiresAt,
                    alertIsActive = alert.IsActive,
                    confirmations = alert.ConfirmedUserIds.Count,
                };

                // Send message via SSE
                await EventsController.SendAlertToAll(message);
                return Ok(result && hasUploaded);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPost("{alertId}/confirmAlert")]
        public async Task<IActionResult> ConfirmAlert(int alertId)
        {
            try
            {
                int userId = HttpContextExtensions.GetUserId(HttpContext) ?? throw new UnauthorizedAccessException();

                bool result = await _alertCollectionService.ConfirmAlert(userId, alertId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{alertId}/confirmations")]
        [AllowAnonymous]
        public async Task<IActionResult> GetConfirmations(int alertId)
        {
            try
            {
                List<int> confirmations = await _alertCollectionService.GetConfirmations(alertId);

                return Ok(confirmations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("getAllAlerts")]
        public async Task<IActionResult> GetAllAlerts()
        {
            try
            {
                List<Alert> alerts = await _alertCollectionService.GetAllAlerts();   

                return Ok(alerts);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("{alertId}/image")]
        [Produces("image/jpeg")]
        [AllowAnonymous]
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
