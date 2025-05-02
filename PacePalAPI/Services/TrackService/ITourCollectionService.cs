using PacePalAPI.Models;
using PacePalAPI.Requests;

namespace PacePalAPI.Services.TrackService
{
    public interface ITourCollectionService : ICollectionService<TourModel>
    {
        //TODO More complex actions that CRUD
        Task<List<TourDto>?> GetUserRecordedTours(int userId);

        Task<bool> UploadTour(TourDto tour);
    }
}
