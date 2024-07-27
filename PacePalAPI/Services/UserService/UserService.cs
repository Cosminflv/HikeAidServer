using Microsoft.EntityFrameworkCore;
using PacePalAPI.Models;
using PacePalAPI.Utils;

namespace PacePalAPI.Services.UserService
{
    public class UserService : IUserCollectionService
    {
        private readonly PacePalContext _context;

        public UserService(PacePalContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<bool> AddUser(UserModel user)
        {
            await _context.Users.AddAsync(user);

            _context.SaveChanges();

            return true;
        }

        public async Task<List<UserModel>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<UserModel?> GetUser(int id)
        {
            UserModel? foundUser = await _context.Users.FirstOrDefaultAsync(UserModel => UserModel.Id == id);

            return foundUser;
        }

        public async Task<UserModel?> LogUser(string username, string password)
        {
            List<UserModel> users = await GetAllUsers();

            UserModel? foundUser = users.FirstOrDefault(user => user.Name == username && user.Password == password);

            return foundUser;
        }
    }
}
