using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Moq;
using PacePalAPI.Models;
using PacePalAPI.Models.Enums;
using PacePalAPI.Requests;
using PacePalAPI.Services.TrackService;

namespace PacePalAPI.Tests
{
    [TestClass]
    public class TourServiceTests
    {
        private DbContextOptions<PacePalContext> _dbContextOptions;
        private Mock<IWebHostEnvironment> _environmentMock;

        [TestInitialize]
        public void Setup()
        {
            _dbContextOptions = new DbContextOptionsBuilder<PacePalContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _environmentMock = new Mock<IWebHostEnvironment>();
            _environmentMock.Setup(e => e.WebRootPath).Returns(Directory.GetCurrentDirectory());
        }

        [TestCleanup]
        public void Cleanup()
        {
            using var context = new PacePalContext(_dbContextOptions);
            context.Database.EnsureDeleted();
        }

        [TestMethod]
        public async Task Create_ShouldAddTour()
        {
            using var context = new PacePalContext(_dbContextOptions);
            var service = new TourService(context, _environmentMock.Object);
            var model = new TourModel
            {
                AuthorId = 1,
                Name = "Test Tour",
                Date = DateTime.UtcNow,
                Distance = 1000,
                Duration = 3600,
                TotalUp = 200,
                TotalDown = 100,
                PreviewImageUrl = "test.jpg"
            };

            var result = await service.Create(model);

            Assert.IsTrue(result);
            Assert.AreEqual(1, await context.Tours.CountAsync());
        }

        [TestMethod]
        public async Task Get_ShouldReturnTour_WhenExists()
        {
            using var context = new PacePalContext(_dbContextOptions);
            var model = new TourModel
            {
                AuthorId = 1,
                Name = "Test Tour",
                Date = DateTime.UtcNow,
                Distance = 1000,
                Duration = 3600,
                TotalUp = 200,
                TotalDown = 100,
                PreviewImageUrl = "test.jpg"
            };
            context.Tours.Add(model);
            await context.SaveChangesAsync();

            var service = new TourService(context, _environmentMock.Object);
            var result = await service.Get(model.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual("Test Tour", result.Name);
        }

        [TestMethod]
        public async Task Delete_ShouldRemoveTour_WhenExists()
        {
            using var context = new PacePalContext(_dbContextOptions);
            var model = new TourModel
            {
                AuthorId = 1,
                Name = "Test Tour",
                Date = DateTime.UtcNow,
                Distance = 1000,
                Duration = 3600,
                TotalUp = 200,
                TotalDown = 100,
                PreviewImageUrl = "test.jpg"
            };
            context.Tours.Add(model);
            await context.SaveChangesAsync();

            var service = new TourService(context, _environmentMock.Object);
            var result = await service.Delete(model.Id);

            Assert.IsTrue(result);
            Assert.AreEqual(0, await context.Tours.CountAsync());
        }

        [TestMethod]
        public async Task GetAll_ShouldReturnAllTours()
        {
            using var context = new PacePalContext(_dbContextOptions);
            context.Tours.AddRange(
                new TourModel
                {
                    AuthorId = 1,
                    Name = "Tour 1",
                    Date = DateTime.UtcNow,
                    Distance = 1000,
                    Duration = 1000,
                    TotalUp = 200,
                    TotalDown = 100,
                    PreviewImageUrl = "1.jpg"
                },
                new TourModel
                {
                    AuthorId = 2,
                    Name = "Tour 2",
                    Date = DateTime.UtcNow,
                    Distance = 2000,
                    Duration = 2000,
                    TotalUp = 300,
                    TotalDown = 150,
                    PreviewImageUrl = "2.jpg"
                });
            await context.SaveChangesAsync();

            var service = new TourService(context, _environmentMock.Object);
            var result = await service.GetAll();

            Assert.AreEqual(2, result?.Count);
        }

        [TestMethod]
        public async Task GetUserRecordedTours_ShouldReturnDtos()
        {
            using var context = new PacePalContext(_dbContextOptions);
            var user = new UserModel
            {
                Id = 1,
                ProfilePictureUrl = $"uploads\\profile_pictures\\pic.jpg",
                FirstName = "Test",
                LastName = "Test",
                PasswordHash = "Test",
                Username = "Test",
                Bio = "Test",
                Age = 20,
                Country = "Test",
                City = "Test",
                Weight = 70,
                Gender = EGender.Man,
                BirthDate = DateTime.Now,
                RecordedTracks = new List<TourModel>
                {
                    new TourModel
                    {
                        Name = "Tour X",
                        Date = DateTime.UtcNow,
                        Distance = 1200,
                        Duration = 3000,
                        TotalUp = 300,
                        TotalDown = 200,
                        PreviewImageUrl = "x.jpg",
                        Coordinates = new List<TourCoordinates>
                        {
                            new TourCoordinates
                            {
                                Latitude = 50,
                                Longitude = 14,
                                Altitude = 200,
                                Timestamp = DateTime.UtcNow
                            }
                        }
                    }
                }
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new TourService(context, _environmentMock.Object);
            var result = await service.GetUserRecordedTours(1);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Tour X", result[0].Name);
            Assert.AreEqual(1, result[0].Coordinates.Count);
        }

        [TestMethod]
        public async Task Update_ShouldReturnFalse_IfNotFound()
        {
            using var context = new PacePalContext(_dbContextOptions);
            var service = new TourService(context, _environmentMock.Object);

            var result = await service.Update(new TourModel { Id = 999 });

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task UploadTour_ShouldPersistTourWithCoordinates()
        {
            using var context = new PacePalContext(_dbContextOptions);
            var service = new TourService(context, _environmentMock.Object);

            var tourDto = new TourDto
            {
                AuthorId = 1,
                Name = "Tour Upload",
                Date = DateTime.UtcNow,
                Distance = 1000,
                Duration = 1000,
                TotalUp = 100,
                TotalDown = 50,
                PreviewImageUrl = "uploaded.jpg",
                Coordinates = new List<TourCoordinatesDto>
                {
                    new TourCoordinatesDto
                    {
                        Latitude = 10,
                        Longitude = 20,
                        Altitude = 100,
                        Timestamp = DateTime.UtcNow
                    }
                }
            };

            var result = await service.UploadTour(tourDto);

            Assert.IsTrue(result);
            Assert.AreEqual(1, await context.Tours.CountAsync());
            var tour = await context.Tours.Include(t => t.Coordinates).FirstAsync();
            Assert.AreEqual(1, tour.Coordinates.Count);
        }
    }
}
