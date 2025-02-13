using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Moq;
using PacePalAPI.Models;
using PacePalAPI.Models.Enums;
using PacePalAPI.Services.AlertService;

namespace PacePalAPI.Tests
{
    [TestClass]
    public class AlertServiceTests
    {
        private DbContextOptions<PacePalContext> _dbContextOptions;
        private Mock<IWebHostEnvironment> _environmentMock;
        private string _webRootPath;

        [TestInitialize]
        public void Setup()
        {
            _dbContextOptions = new DbContextOptionsBuilder<PacePalContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _webRootPath = Directory.GetCurrentDirectory();
            _environmentMock = new Mock<IWebHostEnvironment>();
            _environmentMock.Setup(e => e.WebRootPath).Returns(_webRootPath);
        }

        [TestCleanup]
        public void Cleanup()
        {
            using var context = new PacePalContext(_dbContextOptions);
            context.Database.EnsureDeleted();

            // Cleanup alert picture files that may have been created during tests
            string alertPicturesPath = Path.Combine(_webRootPath, "uploads", "alert_pictures");
            if (Directory.Exists(alertPicturesPath))
            {
                Directory.Delete(alertPicturesPath, true);
            }
        }

        [TestMethod]
        public async Task AddAlert_ShouldReturnTrue_WhenAlertIsAdded()
        {
            // Arrange
            using var context = new PacePalContext(_dbContextOptions);
            var alertService = new AlertService(context, _environmentMock.Object);
            var alert = new Alert
            {
                // Initialize properties as needed
                Id = 1,
                ImageUrl = "",
                ConfirmedUserIds = new List<int>(),
                AuthorId = 1,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddDays(1),
                Title = "Test Alert",
                Description = "This is a test alert",
                AlertType = EAlertType.Other,
                IsActive = true,
                Latitude = 0.0,
                Longitude = 0.0,
                ConfirmedUsers = new List<UserModel>()

            };

            // Act
            var result = await alertService.AddAlert(alert);

            // Assert
            Assert.IsTrue(result);
            var savedAlert = await context.Alerts.FirstOrDefaultAsync(a => a.Id == alert.Id);
            Assert.IsNotNull(savedAlert);
        }

        [TestMethod]
        public async Task ConfirmAlert_ShouldReturnFalse_WhenAlertDoesNotExist()
        {
            // Arrange
            using var context = new PacePalContext(_dbContextOptions);
            var alertService = new AlertService(context, _environmentMock.Object);
            int nonExistingAlertId = 999;
            int userId = 1;

            // Act
            var result = await alertService.ConfirmAlert(userId, nonExistingAlertId);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task ConfirmAlert_ShouldAddUserId_WhenAlertExists()
        {
            // Arrange
            using var context = new PacePalContext(_dbContextOptions);
            var alert = new Alert
            {
                // Initialize properties as needed
                Id = 1,
                ImageUrl = "",
                ConfirmedUserIds = new List<int>(),
                AuthorId = 1,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddDays(1),
                Title = "Test Alert",
                Description = "This is a test alert",
                AlertType = EAlertType.Other,
                IsActive = true,
                Latitude = 0.0,
                Longitude = 0.0,
                ConfirmedUsers = new List<UserModel>()
            };
            context.Alerts.Add(alert);
            await context.SaveChangesAsync();

            var alertService = new AlertService(context, _environmentMock.Object);
            int userId = 42;

            // Act
            var result = await alertService.ConfirmAlert(userId, alert.Id);

            // Assert
            Assert.IsTrue(result);
            var updatedAlert = await context.Alerts.FirstOrDefaultAsync(a => a.Id == alert.Id);
            Assert.IsTrue(updatedAlert.ConfirmedUserIds.Contains(userId));
        }

        [TestMethod]
        public async Task GetAllAlerts_ShouldReturnAllAlerts()
        {
            // Arrange
            using var context = new PacePalContext(_dbContextOptions);
            var alerts = new List<Alert>
            {
                new Alert {                 // Initialize properties as needed
                Id = 1,
                ImageUrl = "",
                ConfirmedUserIds = new List<int>(),
                AuthorId = 1,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddDays(1),
                Title = "Test Alert",
                Description = "This is a test alert",
                AlertType = EAlertType.Other,
                IsActive = true,
                Latitude = 0.0,
                Longitude = 0.0,
                ConfirmedUsers = new List<UserModel>() },
                new Alert {                 // Initialize properties as needed
                Id = 2,
                ImageUrl = "",
                ConfirmedUserIds = new List<int>(),
                AuthorId = 1,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddDays(1),
                Title = "Test Alert",
                Description = "This is a test alert",
                AlertType = EAlertType.Other,
                IsActive = true,
                Latitude = 0.0,
                Longitude = 0.0,
                ConfirmedUsers = new List<UserModel>() }
            };
            context.Alerts.AddRange(alerts);
            await context.SaveChangesAsync();

            var alertService = new AlertService(context, _environmentMock.Object);

            // Act
            var result1 = await alertService.GetAll();
            var result2 = await alertService.GetAllAlerts();

            // Assert
            Assert.IsNotNull(result1);
            Assert.AreEqual(2, result1.Count);
            Assert.AreEqual(2, result2.Count);
        }

        [TestMethod]
        public async Task UploadAlertImage_ShouldReturnFalse_WhenAlertDoesNotExist()
        {
            // Arrange
            using var context = new PacePalContext(_dbContextOptions);
            var alertService = new AlertService(context, _environmentMock.Object);
            int nonExistingAlertId = 999;
            var imageData = new byte[] { 0x01, 0x02 };

            // Act
            var result = await alertService.UploadAlertImage(nonExistingAlertId, imageData);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task UploadAlertImage_ShouldSetDefaultImage_WhenImageDataEmpty()
        {
            // Arrange
            using var context = new PacePalContext(_dbContextOptions);
            var alert = new Alert
            {
                // Initialize properties as needed
                Id = 1,
                ImageUrl = "",
                ConfirmedUserIds = new List<int>(),
                AuthorId = 1,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddDays(1),
                Title = "Test Alert",
                Description = "This is a test alert",
                AlertType = EAlertType.Other,
                IsActive = true,
                Latitude = 0.0,
                Longitude = 0.0,
                ConfirmedUsers = new List<UserModel>()
            };
            context.Alerts.Add(alert);
            await context.SaveChangesAsync();

            var alertService = new AlertService(context, _environmentMock.Object);
            var emptyImageData = new byte[0];

            // Act
            var result = await alertService.UploadAlertImage(alert.Id, emptyImageData);

            // Assert
            Assert.IsTrue(result);
            var updatedAlert = await context.Alerts.FirstOrDefaultAsync(a => a.Id == alert.Id);
            string expectedDefaultPath = Path.Combine(_webRootPath, "uploads\\alert_pictures\\default.base64");
            Assert.AreEqual(expectedDefaultPath, updatedAlert.ImageUrl);
        }

        [TestMethod]
        public async Task UploadAlertImage_ShouldUploadImage_WhenImageDataProvided()
        {
            // Arrange
            using var context = new PacePalContext(_dbContextOptions);
            var alert = new Alert
            {
                // Initialize properties as needed
                Id = 1,
                ImageUrl = "",
                ConfirmedUserIds = new List<int>(),
                AuthorId = 1,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddDays(1),
                Title = "Test Alert",
                Description = "This is a test alert",
                AlertType = EAlertType.Other,
                IsActive = true,
                Latitude = 0.0,
                Longitude = 0.0,
                ConfirmedUsers = new List<UserModel>()
            };
            context.Alerts.Add(alert);
            await context.SaveChangesAsync();

            var alertService = new AlertService(context, _environmentMock.Object);
            var imageData = new byte[] { 0x01, 0x02, 0x03 };

            // Act
            var result = await alertService.UploadAlertImage(alert.Id, imageData);

            // Assert
            Assert.IsTrue(result);
            var updatedAlert = await context.Alerts.FirstOrDefaultAsync(a => a.Id == alert.Id);
            Assert.IsNotNull(updatedAlert.ImageUrl);
            Assert.IsTrue(updatedAlert.ImageUrl.StartsWith("uploads\\alert_pictures\\"));

            // Verify that the file was created and contains the correct data.
            string filePath = Path.Combine(_webRootPath, updatedAlert.ImageUrl);
            Assert.IsTrue(File.Exists(filePath));
            var fileData = File.ReadAllBytes(filePath);
            CollectionAssert.AreEqual(imageData, fileData);
        }

        [TestMethod]
        public async Task GetAlertImageData_ShouldReturnImageData_WhenFileExists()
        {
            // Arrange
            using var context = new PacePalContext(_dbContextOptions);
            // First, create an alert and a dummy image file.
            var alert = new Alert
            {
                // Initialize properties as needed
                Id = 1,
                ImageUrl = "uploads\\alert_pictures\\testImage.base64",
                ConfirmedUserIds = new List<int>(),
                AuthorId = 1,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddDays(1),
                Title = "Test Alert",
                Description = "This is a test alert",
                AlertType = EAlertType.Other,
                IsActive = true,
                Latitude = 0.0,
                Longitude = 0.0,
                ConfirmedUsers = new List<UserModel>(),
            };
            context.Alerts.Add(alert);
            await context.SaveChangesAsync();

            // Ensure the directory exists
            string uploadPath = Path.Combine(_webRootPath, "uploads", "alert_pictures");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }
            string filePath = Path.Combine(_webRootPath, alert.ImageUrl);
            var expectedData = new byte[] { 0xAA, 0xBB, 0xCC };
            File.WriteAllBytes(filePath, expectedData);

            var alertService = new AlertService(context, _environmentMock.Object);

            // Act
            var result = await alertService.GetAlertImageData(alert.Id);

            // Assert
            CollectionAssert.AreEqual(expectedData, result);
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public async Task GetAlertImageData_ShouldThrowFileNotFoundException_WhenFileDoesNotExist()
        {
            // Arrange
            using var context = new PacePalContext(_dbContextOptions);
            var alert = new Alert
            {
                Id = 1,
                ImageUrl = "uploads\\alert_pictures\\nonExisting.base64",
                ConfirmedUserIds = new List<int>(),
                AuthorId = 1,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddDays(1),
                Title = "Test Alert",
                Description = "This is a test alert",
                AlertType = EAlertType.Other,
                IsActive = true,
                Latitude = 0.0,
                Longitude = 0.0,
                ConfirmedUsers = new List<UserModel>(),
            };
            context.Alerts.Add(alert);
            await context.SaveChangesAsync();

            var alertService = new AlertService(context, _environmentMock.Object);

            // Act
            await alertService.GetAlertImageData(alert.Id);

            // Assert is handled by ExpectedException
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task GetAlertImageData_ShouldThrowInvalidOperationException_WhenAlertNotFound()
        {
            // Arrange
            using var context = new PacePalContext(_dbContextOptions);
            var alertService = new AlertService(context, _environmentMock.Object);
            int nonExistingAlertId = 999;

            // Act
            await alertService.GetAlertImageData(nonExistingAlertId);

            // Assert is handled by ExpectedException
        }

        [TestMethod]
        public async Task GetConfirmations_ShouldReturnNonEmptyListAfterUserConfirmed()
        {
            // Arrange

            var alert = new Alert
            {
                // Initialize properties as needed
                Id = 1,
                ImageUrl = "uploads\\alert_pictures\\testImage.base64",
                ConfirmedUserIds = new List<int>(),
                AuthorId = 1,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddDays(1),
                Title = "Test Alert",
                Description = "This is a test alert",
                AlertType = EAlertType.Other,
                IsActive = true,
                Latitude = 0.0,
                Longitude = 0.0,
                ConfirmedUsers = new List<UserModel>(),
            };
            using var context = new PacePalContext(_dbContextOptions);
            context.Alerts.Add(alert);
            await context.SaveChangesAsync();

            var alertService = new AlertService(context, _environmentMock.Object);

            await alertService.ConfirmAlert(3, 1);

            List<int> confirmations = await alertService.GetConfirmations(1);

            Assert.IsTrue(confirmations.Count == 1);
            Assert.IsTrue(confirmations.First() == 3);
        }

        [TestMethod]
        public async Task SetConfirmations_ShouldNotConfirmWithSameIdTwice()
        {
            // Arrange

            var alert = new Alert
            {
                // Initialize properties as needed
                Id = 1,
                ImageUrl = "uploads\\alert_pictures\\testImage.base64",
                ConfirmedUserIds = new List<int>(),
                AuthorId = 1,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddDays(1),
                Title = "Test Alert",
                Description = "This is a test alert",
                AlertType = EAlertType.Other,
                IsActive = true,
                Latitude = 0.0,
                Longitude = 0.0,
                ConfirmedUsers = new List<UserModel>(),
            };
            using var context = new PacePalContext(_dbContextOptions);
            context.Alerts.Add(alert);
            await context.SaveChangesAsync();

            var alertService = new AlertService(context, _environmentMock.Object);

            await alertService.ConfirmAlert(3, 1);
            await alertService.ConfirmAlert(3, 1);

            List<int> confirmations = await alertService.GetConfirmations(1);

            Assert.IsTrue(confirmations.Count == 1);
            Assert.IsTrue(confirmations.First() == 3);
        }

        [TestMethod]
        public async Task SetConfirmations_ShouldExtendExpireDateWhenConfirmed()
        {
            // Arrange

            DateTime expiringDate = new DateTime(2023, 10, 5);

            var alert = new Alert
            {
                // Initialize properties as needed
                Id = 1,
                ImageUrl = "uploads\\alert_pictures\\testImage.base64",
                ConfirmedUserIds = new List<int>(),
                AuthorId = 1,
                CreatedAt = DateTime.Now,
                ExpiresAt = expiringDate,
                Title = "Test Alert",
                Description = "This is a test alert",
                AlertType = EAlertType.Other,
                IsActive = true,
                Latitude = 0.0,
                Longitude = 0.0,
                ConfirmedUsers = new List<UserModel>(),
            };
            using var context = new PacePalContext(_dbContextOptions);
            context.Alerts.Add(alert);
            await context.SaveChangesAsync();

            var alertService = new AlertService(context, _environmentMock.Object);

            await alertService.ConfirmAlert(3, 1);

            Alert? retrievedAlert = await alertService.Get(1);
            Assert.IsTrue(retrievedAlert != null);
            Assert.IsTrue(retrievedAlert.ExpiresAt.Day > expiringDate.Day);
        }
    }
}
