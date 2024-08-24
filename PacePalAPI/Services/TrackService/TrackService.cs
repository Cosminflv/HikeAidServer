using Microsoft.EntityFrameworkCore;
using PacePalAPI.Models;
using PacePalAPI.Requests;
using System.Runtime.ConstrainedExecution;

namespace PacePalAPI.Services.TrackService
{
    public class TrackService : ITrackCollectionService
    {
        private readonly PacePalContext _context;
        private readonly IWebHostEnvironment _environment;

        public TrackService(PacePalContext context, IWebHostEnvironment environment)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public async Task<bool> Create(TrackModel model)
        {
            await _context.RecordedTracks.AddAsync(model);
            await _context.SaveChangesAsync();
            return true;

        }

        public async Task<bool> Delete(int id)
        {
            TrackModel? trackToDelete = await _context.RecordedTracks.FirstOrDefaultAsync(x => x.Id == id);

            if (trackToDelete == null) return false;

            //TODO ALSO DELETE STORED FILES

            string gpxFilePath = Path.Combine(_environment.WebRootPath, trackToDelete.GpxFilePath);
            string logFilePath = Path.Combine(_environment.WebRootPath, trackToDelete.LogFilePath);

            if (System.IO.File.Exists(gpxFilePath) && System.IO.File.Exists(logFilePath))
            {
                System.IO.File.Delete(gpxFilePath);
                System.IO.File.Delete(logFilePath);
            }

            _context.RecordedTracks.Remove(trackToDelete);
            return true;
        }

        public async Task<TrackModel?> Get(int id)
        {
            return await _context.RecordedTracks.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<TrackModel>?> GetAll()
        {
            return await _context.RecordedTracks.ToListAsync();
        }

        public async Task<List<TrackDto>?> GetUserRecordedTracks(int userId)
        {
            UserModel? foundUser = await _context.Users
                        .Include(u => u.RecordedTracks)
                        .FirstOrDefaultAsync(u => u.Id == userId);

            if (foundUser == null) return null;

            List<TrackModel> tracks = foundUser.RecordedTracks;
            List<TrackDto> dtoList = new List<TrackDto>();

            foreach(var track in tracks)
            {
                string gpxFilePath = Path.Combine(_environment.WebRootPath, track.GpxFilePath);
                string logFilePath = Path.Combine(_environment.WebRootPath, track.LogFilePath);

                if(!System.IO.File.Exists(gpxFilePath) || !System.IO.File.Exists(logFilePath)) return null;

                byte[] gpxData =  await System.IO.File.ReadAllBytesAsync(gpxFilePath);
                byte[] logData =  await System.IO.File.ReadAllBytesAsync(logFilePath);

                dtoList.Add(new TrackDto {
                    UserId = track.UserId,
                    GpxData = gpxData,
                    LogData = logData,
                });
            }

            return dtoList;           
        }

        public async Task<bool> Update(int id, TrackModel model)
        {
            TrackModel? trackToUpdate = await _context.RecordedTracks.FirstOrDefaultAsync(x => x.Id == id);

            if (trackToUpdate == null) return false;

            _context.RecordedTracks.Update(trackToUpdate);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UploadTrackBase64(int userId, byte[] gpxFileData, byte[] logFileData)
        {
            UserModel? user = await _context.Users.FirstOrDefaultAsync(u => u.Id ==  userId);

            if (user == null) return false;

            var (gpxFilePath, gpxFileName, logFilePath, logFileName) = _createFilePath(userId);

            string gpxBase64String = Convert.ToBase64String(gpxFileData);
            string logBase64String = Convert.ToBase64String(logFileData);

            File.WriteAllText(gpxFilePath, gpxBase64String);
            File.WriteAllText(logFilePath, logBase64String);

            var gpxFileUrl = $"uploads\\tracks\\{gpxFileName}";
            var logFileUrl = $"uploads\\tracks_logs\\{logFileName}";

            TrackModel trackToAdd = new TrackModel
            {
                UserId = userId,
                GpxFilePath = gpxFileUrl,
                LogFilePath = logFileUrl,
            };

            await _context.RecordedTracks.AddAsync(trackToAdd);
            await _context.SaveChangesAsync();

            return true;
        }

        private (string, string, string, string) _createFilePath(int userId) 
        {
            string gpxFileName = $"{userId}_{Guid.NewGuid()}.base64";
            string logFileName = $"{userId}_{Guid.NewGuid()}.base64";

            string gpxUploadPath = Path.Combine(_environment.WebRootPath, "uploads", "tracks");
            string logUploadPath = Path.Combine(_environment.WebRootPath, "uploads", "tracks_logs");

            if(!Directory.Exists(gpxUploadPath)) Directory.CreateDirectory(gpxUploadPath);
            if(!Directory.Exists(logUploadPath)) Directory.CreateDirectory(logUploadPath);

            string gpxFilePath = Path.Combine(gpxUploadPath, gpxFileName);
            string logFilePath = Path.Combine(logUploadPath, logFileName);

            return (gpxFilePath, gpxFileName, logFilePath, logFileName);
        }
    }
}
