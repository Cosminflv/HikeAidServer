﻿using PacePalAPI.Models;


namespace PacePalAPI.Services.UserService
{
    public interface IUserCollectionService : ICollectionService<UserModel>
    {
        //TODO More complex methods than CRUD

        Task<List<double>> PredictUserPosition(int userId);

        Task<bool> AddProgressCoordinates(int userId, List<Coordinate> coordinates);

        Task<bool> ConfirmHike(int userId, List<Coordinate> trackCoordinates);

        Task<ConfirmedCurrentHike> GetActiveHike(int userId);

        Task<bool> UpdateHikeProgress(ConfirmedCurrentHike hike);

        Task<int> SendFriendRequest(int requesterId, int recieverId);

        Task<bool> AcceptFriendRequest(int requestId);

        Task<bool> DeclineFriendRequest(int requestId);

        Task<List<FriendshipModel>?> GetFriendshipRequests(int receiverId);

        Task<EFriendshipStatus> GetFriendshipStatus(int user1, int user2);

        Task<int> NumberOfFriends(int userId);

        Task<bool> UploadProfilePicture(int userId, byte[] imageData);

        Task<bool> DeleteProfilePicture(int userId);

        Task<byte[]> GetProfilePicture(int userId);

        Task<byte[]> GetDefaultUserPicture();
    }
}
