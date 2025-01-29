using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PacePalAPI.Models;
using PacePalAPI.Models.Enums;
using PacePalAPI.Services.UserService;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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

            _userService = new UserService(context, _environmentMock.Object);

            // Act
            var result = await _userService.AcceptFriendRequest(friendship.Id);

            // Assert
            Assert.IsTrue(result);
            var updatedFriendship = await context.Friendships.FirstOrDefaultAsync(f => f.Id == friendship.Id);
            Assert.AreEqual(EFriendshipState.Accepted, updatedFriendship.Status);
        }

        [TestMethod]
        public async Task SendFriendRequest_ShouldAddNewFriendshipRequest()
        {
            // Arrange
            using var context = new PacePalContext(_dbContextOptions);
            _userService = new UserService(context, _environmentMock.Object);

            // Act
            var result = await _userService.SendFriendRequest(3, 4);

            // Assert
            Assert.IsTrue(result > 0);
            var newFriendship = await context.Friendships.FirstOrDefaultAsync(f => f.RequesterId == 3 && f.ReceiverId == 4);
            Assert.IsNotNull(newFriendship);
            Assert.AreEqual(EFriendshipState.Pending, newFriendship.Status);
        }
    }
}
