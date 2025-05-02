using Microsoft.EntityFrameworkCore;
using PacePalAPI.Models;
using PacePalAPI.Requests;

namespace PacePalAPI.Services.TrackService
{
    public class TourService : ITourCollectionService
    {
        private readonly PacePalContext _context;
        private readonly IWebHostEnvironment _environment;

        public TourService(PacePalContext context, IWebHostEnvironment environment)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public async Task<bool> Create(TourModel model)
        {
            await _context.Tours.AddAsync(model);
            await _context.SaveChangesAsync();
            return true;

        }

        public async Task<bool> Delete(int id)
        {
            TourModel? foundTour = await _context.Tours.FirstOrDefaultAsync(x => x.Id == id);

            if (foundTour == null) return false;

            _context.Tours.Remove(foundTour);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<TourModel?> Get(int id)
        {
            return await _context.Tours.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<TourModel>?> GetAll()
        {
            return await _context.Tours.ToListAsync();
        }

        public async Task<List<TourDto>?> GetUserRecordedTours(int userId)
        {
            UserModel? foundUser = await _context.Users
            .Include(u => u.RecordedTracks)
            .FirstOrDefaultAsync(u => u.Id == userId);

            if (foundUser == null) return null;

            List<TourModel> tracks = foundUser.RecordedTracks;
            List<TourDto> dtoList = new List<TourDto>();

            foreach (var track in tracks) {
                TourDto dto = new TourDto
                {
                    Id = track.Id,
                    AuthorId = track.AuthorId,
                    Name = track.Name,
                    Date = track.Date,
                    Distance = track.Distance,
                    Duration = track.Duration,
                    TotalUp = track.TotalUp,
                    TotalDown = track.TotalDown,
                    PreviewImageUrl = track.PreviewImageUrl
                };

                foreach (var coordinate in track.Coordinates)
                {
                    TourCoordinatesDto coordinatesDto = new TourCoordinatesDto
                    {
                        Latitude = coordinate.Latitude,
                        Longitude = coordinate.Longitude,
                        Speed = coordinate.Speed,
                        Altitude = coordinate.Altitude,
                        Timestamp = coordinate.Timestamp
                    };
                    dto.Coordinates.Add(coordinatesDto);
                }

                dtoList.Add(dto);
            }

            return dtoList;
        }

        public async Task<bool> Update(TourModel model)
        {
            TourModel? trackToUpdate = await _context.Tours.FirstOrDefaultAsync(x => x.Id == model.Id);

            if (trackToUpdate == null) return false;

            _context.Tours.Update(trackToUpdate);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UploadTour(TourDto tourToUpload)
        {
            TourModel tourModel = new TourModel
            {
                AuthorId = tourToUpload.AuthorId,
                Name = tourToUpload.Name,
                Date = tourToUpload.Date,
                Distance = tourToUpload.Distance,
                Duration = tourToUpload.Duration,
                TotalUp = tourToUpload.TotalUp,
                TotalDown = tourToUpload.TotalDown,
                PreviewImageUrl = tourToUpload.PreviewImageUrl
            };

            foreach (var coordinate in tourToUpload.Coordinates)
            {
                TourCoordinates coordinates = new TourCoordinates
                {
                    Latitude = coordinate.Latitude,
                    Longitude = coordinate.Longitude,
                    Speed = coordinate.Speed,
                    Altitude = coordinate.Altitude,
                    Timestamp = coordinate.Timestamp,
                    TourId = tourModel.Id
                };
                tourModel.Coordinates.Add(coordinates);
            }

            _context.Tours.Add(tourModel);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
