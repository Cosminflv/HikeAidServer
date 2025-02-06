using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Moq;
using PacePalAPI.Models;
using PacePalAPI.Models.Enums;
using PacePalAPI.Services.UserService;

namespace PacePalAPI.Tests
{
    [TestClass]
    public class UserServiceTests
    {
        private UserService _userService;
        private DbContextOptions<PacePalContext> _dbContextOptions;
        private Mock<IWebHostEnvironment> _environmentMock;

        [TestInitialize]
        public void Setup()
        {
            _dbContextOptions = new DbContextOptionsBuilder<PacePalContext>()
                .UseInMemoryDatabase(databaseName: "PacePalTestDb")
                .Options;

            _environmentMock = new Mock<IWebHostEnvironment>();
            _environmentMock.Setup(e => e.WebRootPath).Returns(Directory.GetCurrentDirectory());

            var context = new PacePalContext(_dbContextOptions);
            _userService = new UserService(context, _environmentMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            using var context = new PacePalContext(_dbContextOptions);
            context.Database.EnsureDeleted(); // Delete the in-memory database after each test
        }

        [TestMethod]
        public async Task AcceptFriendRequest_ShouldUpdateFriendshipStatus()
        {
            // Arrange
            using var context = new PacePalContext(_dbContextOptions);
            var friendship = new FriendshipModel
            {
                Id = 1,
                RequesterId = 1,
                ReceiverId = 2,
                Status = EFriendshipState.Pending
            };
            context.Friendships.Add(friendship);
            await context.SaveChangesAsync();

            var userService = new UserService(context, _environmentMock.Object);

            // Act
            var result = await userService.AcceptFriendRequest(friendship.Id);

            // Assert
            Assert.IsTrue(result);
            var updatedFriendship = await context.Friendships.FirstOrDefaultAsync(f => f.Id == friendship.Id);
            Assert.AreEqual(EFriendshipState.Accepted, updatedFriendship.Status);
        }

        [TestMethod]
        public async Task DeclineFriendRequest_ShouldUpdateFriendshipStatus()
        {
            // Arrange
            using var context = new PacePalContext(_dbContextOptions);
            var friendship = new FriendshipModel
            {
                Id = 1,
                RequesterId = 1,
                ReceiverId = 2,
                Status = EFriendshipState.Pending
            };
            context.Friendships.Add(friendship);
            await context.SaveChangesAsync();

            var userService = new UserService(context, _environmentMock.Object);

            // Act
            var result = await userService.DeclineFriendRequest(friendship.Id);

            // Assert
            Assert.IsTrue(result);
            var updatedFriendship = await context.Friendships.FirstOrDefaultAsync(f => f.Id == friendship.Id);
            Assert.AreEqual(EFriendshipState.Declined, updatedFriendship.Status);
        }

        [TestMethod]
        public async Task DeleteProfilePicture_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            // Arrange
            using var context = new PacePalContext(_dbContextOptions);
            var userService = new UserService(context, _environmentMock.Object);

            // Act
            var result = await userService.DeleteProfilePicture(1);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task DeleteProfilePicture_ShouldReturnTrue_WhenProfilePictureIsDeleted()
        {
            // Arrange
            using var context = new PacePalContext(_dbContextOptions);


            var (filePath, fileName) = CreateUserImageFilePath(1);

            string base64String = Convert.ToBase64String(new byte[] { 0x01, 0x02 });

            File.WriteAllText(filePath, base64String);

            var user = new UserModel
            {
                Id = 1,
                ProfilePictureUrl = $"uploads\\profile_pictures\\{fileName}",
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
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userService = new UserService(context, _environmentMock.Object);

            // Act
            var result = await userService.DeleteProfilePicture(user.Id);

            // Assert
            Assert.IsTrue(result);
            var updatedUser = await context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            Assert.AreEqual("uploads\\profile_pictures\\default.base64", updatedUser.ProfilePictureUrl);
        }

        [TestMethod]
        public async Task GetDefaultUserPicture_ShouldReturnDefaultImage()
        {
            // Arrange
            using var context = new PacePalContext(_dbContextOptions);
            var defaultImageContent = "defaultImageContent";
            var defaultImagePath = Path.Combine(_environmentMock.Object.WebRootPath, "uploads\\profile_pictures\\default.base64");
            Directory.CreateDirectory(Path.GetDirectoryName(defaultImagePath));
            File.WriteAllText(defaultImagePath, defaultImageContent);

            var userService = new UserService(context, _environmentMock.Object);

            // Act
            var result = await userService.GetDefaultUserPicture();

            // Assert
            Assert.AreEqual(result.Length != 0, true);
        }

        [TestMethod]
        public async Task GetFriendshipRequests_ShouldReturnListOfRequests()
        {
            // Arrange
            using var context = new PacePalContext(_dbContextOptions);
            var friendshipRequests = new List<FriendshipModel>
            {
                new FriendshipModel { Id = 1, RequesterId = 1, ReceiverId = 2, Status = EFriendshipState.Pending },
                new FriendshipModel { Id = 2, RequesterId = 3, ReceiverId = 2, Status = EFriendshipState.Pending }
            };
            context.Friendships.AddRange(friendshipRequests);
            await context.SaveChangesAsync();

            var userService = new UserService(context, _environmentMock.Object);

            // Act
            var result = await userService.GetFriendshipRequests(2);

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public async Task SendFriendRequest_ShouldReturnNegativeOne_WhenFriendshipAlreadyExists()
        {
            // Arrange
            using var context = new PacePalContext(_dbContextOptions);
            var existingFriendship = new FriendshipModel
            {
                Id = 1,
                RequesterId = 1,
                ReceiverId = 2,
                Status = EFriendshipState.Pending
            };
            context.Friendships.Add(existingFriendship);
            await context.SaveChangesAsync();

            var userService = new UserService(context, _environmentMock.Object);

            // Act
            var result = await userService.SendFriendRequest(1, 2);

            // Assert
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public async Task SendFriendRequest_ShouldReturnNewFriendshipId_WhenFriendshipIsCreated()
        {
            // Arrange
            using var context = new PacePalContext(_dbContextOptions);
            var userService = new UserService(context, _environmentMock.Object);

            // Act
            var result = await userService.SendFriendRequest(1, 2);

            // Assert
            Assert.IsTrue(result > 0);
            var newFriendship = await context.Friendships.FirstOrDefaultAsync(f => f.RequesterId == 1 && f.ReceiverId == 2);
            Assert.IsNotNull(newFriendship);
            Assert.AreEqual(EFriendshipState.Pending, newFriendship.Status);
        }

        [TestMethod]
        public async Task NumberOfFriends_ShouldReturnCorrectCount()
        {
            // Arrange
            using var context = new PacePalContext(_dbContextOptions);
            var friendships = new List<FriendshipModel>
            {
                new FriendshipModel { Id = 1, RequesterId = 1, ReceiverId = 2, Status = EFriendshipState.Accepted },
                new FriendshipModel { Id = 2, RequesterId = 3, ReceiverId = 1, Status = EFriendshipState.Accepted },
                new FriendshipModel { Id = 3, RequesterId = 4, ReceiverId = 5, Status = EFriendshipState.Pending }
            };
            context.Friendships.AddRange(friendships);
            await context.SaveChangesAsync();

            var userService = new UserService(context, _environmentMock.Object);

            // Act
            var result = await userService.NumberOfFriends(1);

            // Assert
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public async Task UploadProfilePicture_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            // Arrange
            using var context = new PacePalContext(_dbContextOptions);
            var userService = new UserService(context, _environmentMock.Object);

            // Act
            var result = await userService.UploadProfilePicture(1, new byte[] { 0x01, 0x02 });

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task UploadProfilePicture_ShouldReturnTrue_WhenProfilePictureIsUploaded()
        {
            // Arrange
            using var context = new PacePalContext(_dbContextOptions);
            var user = new UserModel
            {
                Id = 1,
                ProfilePictureUrl = "uploads\\profile_pictures\\test.base64",
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
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userService = new UserService(context, _environmentMock.Object);

            // Act
            var result = await userService.UploadProfilePicture(user.Id, new byte[] { 0x01, 0x02 });

            // Assert
            Assert.IsTrue(result);
            var updatedUser = await context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            Assert.IsTrue(updatedUser.ProfilePictureUrl.StartsWith("uploads\\profile_pictures\\"));
        }

        [TestMethod]
        public async Task GetFriendshipStatus_ShouldReturnNone_WhenNoFriendshipExists()
        {
            // Arrange
            using var context = new PacePalContext(_dbContextOptions);
            var userService = new UserService(context, _environmentMock.Object);

            // Act
            var result = await userService.GetFriendshipStatus(1, 2);

            // Assert
            Assert.AreEqual(EFriendshipStatus.None, result);
        }

        [TestMethod]
        public async Task GetFriendshipStatus_ShouldReturnFriends_WhenFriendshipIsAccepted()
        {
            // Arrange
            using var context = new PacePalContext(_dbContextOptions);
            var friendship = new FriendshipModel
            {
                Id = 1,
                RequesterId = 1,
                ReceiverId = 2,
                Status = EFriendshipState.Accepted
            };
            context.Friendships.Add(friendship);
            await context.SaveChangesAsync();

            var userService = new UserService(context, _environmentMock.Object);

            // Act
            var result = await userService.GetFriendshipStatus(1, 2);

            // Assert
            Assert.AreEqual(EFriendshipStatus.Friends, result);
        }

        private (string, string) CreateUserImageFilePath(int userId)
        {
            string fileName = $"{userId}_{Guid.NewGuid()}.base64";

            string uploadPath = Path.Combine(_environmentMock.Object.WebRootPath, "uploads", "profile_pictures");

            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            string filePath = Path.Combine(uploadPath, fileName);

            return (filePath, fileName);
        }
    }
}