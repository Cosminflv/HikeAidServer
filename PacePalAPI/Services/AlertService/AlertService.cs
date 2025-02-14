using Microsoft.EntityFrameworkCore;
using PacePalAPI.Models;

namespace PacePalAPI.Services.AlertService
{
    public class AlertService : IAlertCollectionService
    {
        private readonly PacePalContext _context;
        private readonly IWebHostEnvironment _environment;
        private static readonly object _fileLock = new object();

        public AlertService(PacePalContext context, IWebHostEnvironment environment)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }
        public async Task<bool> AddAlert(Alert alert)
        {
            await _context.Alerts.AddAsync(alert);
            int entries = await _context.SaveChangesAsync();
            return entries > 0;
        }

        public async Task<bool> ConfirmAlert(int userId, int alertId)
        {
            Alert? foundAlert = await _context.Alerts.FirstOrDefaultAsync((alert) => alert.Id == alertId);

            if (foundAlert == null)
            {
                return false;
            }
            List<int> confirmedUserIds = foundAlert.ConfirmedUserIds;
            if (confirmedUserIds.Contains(userId)) return false;

            foundAlert.ExpiresAt = foundAlert.ExpiresAt.AddDays(1);

            foundAlert.ConfirmedUserIds.Add(userId);
            _context.Alerts.Update(foundAlert);
            int entries = await _context.SaveChangesAsync();
            return entries > 0;
        }

        public Task<bool> Create(Alert model)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<Alert?> Get(int id)
        {
            return await _context.Alerts.FirstOrDefaultAsync((alert) => alert.Id == id);
        }

        public async Task<List<Alert>?> GetAll()
        {
            return await _context.Alerts.ToListAsync();
        }

        public async Task<List<Alert>> GetAllAlerts()
        {
            return await _context.Alerts
                    .Where(alert => alert.ExpiresAt >= DateTime.Today)
                    .ToListAsync();
        }

        public Task<bool> Update(Alert model)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UploadAlertImage(int alertId, IFormFile imageFile)
        {
            var foundAlert = await _context.Alerts.FindAsync(alertId);
            if (foundAlert == null)
                return false;

            // Generate file path and file name (ensure this method creates a unique path)
            (string filePath, string fileName) = CreateAlertImageFilePath(alertId);

            // Use asynchronous file writing with a FileStream
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            // Set the URL relative to your web root (use forward slashes for URLs)
            foundAlert.ImageUrl = $"uploads/alert_pictures/{fileName}";

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetDefaultAlertImage(int alertId)
        {
            var foundAlert = await _context.Alerts.FindAsync(alertId);
            if (foundAlert == null)
                return false;

            // Get the default image path from configuration or hard-coded value
            string defaultFilePath = Path.Combine(_environment.WebRootPath, "uploads", "alert_pictures", "default.base64");
            foundAlert.ImageUrl = defaultFilePath;

            await _context.SaveChangesAsync();
            return true;
        }


        private (string, string) CreateAlertImageFilePath(int alertId)
        {
            string fileName = $"{alertId}_{Guid.NewGuid()}.base64";

            string uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "alert_pictures");

            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            string filePath = Path.Combine(uploadPath, fileName);

            return (filePath, fileName);
        }

        public async Task<byte[]> GetAlertImageData(int alertId)
        {
            Alert alert = await _context.Alerts.FirstOrDefaultAsync(a => a.Id == alertId) ?? throw new InvalidOperationException();

            string filePath = Path.Combine(_environment.WebRootPath, alert.ImageUrl);

            if (!File.Exists(filePath)) throw new FileNotFoundException();

            return await System.IO.File.ReadAllBytesAsync(filePath);
        }

        public async Task<List<int>> GetConfirmations(int alertId)
        {
            Alert alert = await _context.Alerts.FirstOrDefaultAsync(a => a.Id == alertId) ?? throw new InvalidOperationException();

            return alert.ConfirmedUserIds;
        }
    }
}
