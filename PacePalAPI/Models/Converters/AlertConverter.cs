using PacePalAPI.Models;
using PacePalAPI.Requests;
using PacePalAPI.Models.Enums;

namespace PacePalAPI.Converters
{
    public static class AlertConverter
    {
        public static AlertDto ToDto(Alert alert)
        {
            return new AlertDto
            {
                CreatedAt = alert.CreatedAt,
                ExpiresAt = alert.ExpiresAt,
                Title = alert.Title,
                Description = alert.Description,
                AlertType = alert.AlertType.ToString(),
                IsActive = alert.IsActive,
                Latitude = alert.Latitude,
                Longitude = alert.Longitude
            };
        }

        public static Alert ToModel(AlertDto alertDto, int authorId)
        {
            return new Alert
            {
                AuthorId = authorId,
                CreatedAt = alertDto.CreatedAt,
                ExpiresAt = alertDto.ExpiresAt,
                Title = alertDto.Title,
                Description = alertDto.Description,
                AlertType = EAlertTypeExtensions.FromString(alertDto.AlertType),
                IsActive = alertDto.IsActive,
                Latitude = alertDto.Latitude,
                Longitude = alertDto.Longitude,
                ConfirmedUsers = new List<UserModel>(), // Initial empty list
                ConfirmedUserIds = new List<int>(),
                ImageUrl = ""
            };
        }
    }
}
