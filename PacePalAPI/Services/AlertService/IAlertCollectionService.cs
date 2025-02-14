using PacePalAPI.Models;

namespace PacePalAPI.Services.AlertService
{
    public interface IAlertCollectionService : ICollectionService<Alert>
    {
        //TODO More complex methods than CRUD

        Task<bool> AddAlert(Alert alert);

        Task<List<Alert>> GetAllAlerts();

        Task<bool> ConfirmAlert(int userId, int alertId);

        Task<List<int>> GetConfirmations(int alertId);

        Task<bool> UploadAlertImage(int alertId, IFormFile imageData);

        Task<bool> SetDefaultAlertImage(int alertId);

        Task<byte[]> GetAlertImageData(int alertId);
    }
}
