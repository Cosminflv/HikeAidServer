using PacePalAPI.Models;

namespace PacePalAPI.Services.AlertService
{
    public interface IAlertCollectionService : ICollectionService<Alert>
    {
        //TODO More complex methods than CRUD

        Task<bool> AddAlert(Alert alert);

        Task<List<Alert>> GetAllAlerts();

        Task<bool> ConfirmAlert(int userId, int alertId);

        Task<bool> UploadAlertImage(int alertId, byte[] imageData);
    }
}
