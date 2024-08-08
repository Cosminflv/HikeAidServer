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

        async Task<bool> ICollectionService<UserModel>.Create(UserModel model)
        {
            await _context.Users.AddAsync(model);
            _context.SaveChanges();
            return true;
        }

        async Task<bool> ICollectionService<UserModel>.Delete(int id)
        {
            UserModel? userToDelete = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

            if(userToDelete == null) return false;
            _context.Users.Remove(userToDelete);
            return true;
        }

        async Task<UserModel?> ICollectionService<UserModel>.Get(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
        }

        async Task<List<UserModel>?> ICollectionService<UserModel>.GetAll()
        {
           return await _context.Users.ToListAsync();
        }

        async Task<bool> ICollectionService<UserModel>.Update(int id, UserModel model)
        {
            UserModel? userToUpdate = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if(userToUpdate == null) return false;
            _context.Users.Update(userToUpdate);
            return true;
        }

        //public async Task<bool> AddUser(UserModel user)
        //{
        //    await _context.Users.AddAsync(user);

        //    _context.SaveChanges();

        //    return true;
        //}

        //public async Task<List<UserModel>> GetAllUsers()
        //{
        //    return await _context.Users.ToListAsync();
        //}

        //public async Task<UserModel?> GetUser(int id)
        //{
        //    UserModel? foundUser = await _context.Users.FirstOrDefaultAsync(UserModel => UserModel.Id == id);

        //    return foundUser;
        //}

        //public async Task<UserModel?> LogUser(string username, string password)
        //{
        //    List<UserModel> users = await GetAllUsers();

        //    UserModel? foundUser = users.FirstOrDefault(user => user.Name == username && user.Password == password);

        //    return foundUser;
        //}
    }
}
