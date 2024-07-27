using PacePalAPI.Models;
using PacePalAPI.Utils;

namespace PacePalAPI.Services.UserService
{
    public interface ICollectionService<UserModel>
    {
        public Task<bool> AddUser(UserModel user);

        public Task<List<UserModel>> GetAllUsers();

        public Task<UserModel?> GetUser(int id);

        public Task<UserModel?> LogUser(string username, string password);

    }
}
