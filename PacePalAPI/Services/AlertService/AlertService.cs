using Microsoft.EntityFrameworkCore;
using PacePalAPI.Models;
using PacePalAPI.Services.UserService;

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

            if(foundAlert == null)
            {
                return false;
            }

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

        public Task<Alert?> Get(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Alert>?> GetAll()
        {
            throw new NotImplementedException();
        }

        public async Task<List<Alert>> GetAllAlerts()
        {
            return await _context.Alerts.ToListAsync();
        }

        public Task<bool> Update(int id, Alert model)
        {
            throw new NotImplementedException();
        }
    }
}
