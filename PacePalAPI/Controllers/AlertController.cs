using Microsoft.AspNetCore.Mvc;
using PacePalAPI.Controllers.Middleware;
using PacePalAPI.Models;
using PacePalAPI.Models.Enums;
using PacePalAPI.Requests;
using PacePalAPI.Services.AlertService;

namespace PacePalAPI.Controllers
{
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
        public async Task<IActionResult> AddAlert(AlertDto alertDto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest("Invalid model state.");

                var alert = new Alert
                {
                    AuthorId = alertDto.AuthorId,
                    CreatedAt = alertDto.CreatedAt,
                    ExpiresAt = alertDto.ExpiresAt,
                    Title = alertDto.Title,
                    Description = alertDto.Description,
                    AlertType = EAlertTypeExtensions.FromString(alertDto.AlertType),
                    IsActive = alertDto.IsActive,
                    LocationCoords = new Coordinates
                    {
                        Latitude = alertDto.Latitude,
                        Longitude = alertDto.Longitude
                    }
                };

            bool result = await _alertCollectionService.AddAlert(alert);
            if (!result) return BadRequest("Error while adding alert.");
            return Ok(true);

            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
