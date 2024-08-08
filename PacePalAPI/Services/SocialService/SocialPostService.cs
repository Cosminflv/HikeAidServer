using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using PacePalAPI.Models;

namespace PacePalAPI.Services.SocialService
{
    public class SocialPostService : ISocialPostCollectionService
    {
        private readonly PacePalContext _context;
        public SocialPostService(PacePalContext context) 
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        async public Task<bool> Create(SocialPostModel model)
        {
            UserModel? userPosting = await _context.Users.FirstOrDefaultAsync(u => u.Id == model.UserId);

            if (userPosting == null) return false;

            model.User = userPosting;

            _context.SocialPosts.Add(model);
            _context.SaveChanges();

            return true;
        }

        public Task<bool> Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Task<SocialPostModel?> Get(int id)
        {
            throw new NotImplementedException();
        }

        async public Task<List<SocialPostModel>?> GetAll()
        {
            return await _context.SocialPosts.ToListAsync();
        }

        public Task<bool> Update(int id, SocialPostModel model)
        {
            throw new NotImplementedException();
        }
    }
}
