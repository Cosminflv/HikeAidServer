using PacePalAPI.Models;
using PacePalAPI.Requests;

namespace PacePalAPI.Services.TrackService
{
    public interface ITrackCollectionService : ICollectionService<TrackModel>
    {
        //TODO More complex actions that CRUD

        Task<List<TrackDto>?> GetUserRecordedTracks(int userId);

        Task<bool> UploadTrackBase64(int userId, byte[] gpxFileData, byte[] logFileData);
    }
}
