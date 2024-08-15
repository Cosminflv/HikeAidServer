using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
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

        public async Task<bool> LikePost(int postId, int userId)
        {
            SocialPostModel? postToLike = await _context.SocialPosts.FirstOrDefaultAsync(p => p.Id == postId);

            if (postToLike == null) return false;

            LikeModel likeModel = new LikeModel();
            likeModel.PostId = postId;
            likeModel.UserId = userId;


            postToLike.Likes.Add(likeModel);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Create(SocialPostModel model)
        {
            UserModel? userPosting = await _context.Users.FirstOrDefaultAsync(u => u.Id == model.UserId);

            if (userPosting == null) return false;

            //model.User = userPosting;

            _context.SocialPosts.Add(model);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<SocialPostModel?> Get(int id)
        {
            return await _context.SocialPosts.FirstOrDefaultAsync(post => post.Id == id);
        }

        public async Task<List<SocialPostModel>?> GetAll()
        {
            return await _context.SocialPosts
                .Include(post => post.User)
                .Include(post => post.Likes)
                .ToListAsync();
        }

        public async Task<bool> Update(int id, string content, string imageUrl)
        {
            SocialPostModel? postToUpdate = await _context.SocialPosts.FirstOrDefaultAsync(post => post.Id == id);

            if (postToUpdate == null) return false;

            postToUpdate.Content = content;
            postToUpdate.ImageUrl = imageUrl;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<SocialPostModel>> GetUserPosts(int userId)
        {
            return await _context.SocialPosts.Where(post => post.UserId == userId).ToListAsync();
        }

        public async Task<bool> Delete(int idToDelete)
        {
            // Fetch and delete comments first
            var commentsToDelete = await _context.Comments
                .Where(comment => comment.PostId == idToDelete)
                .ToListAsync();

            _context.Comments.RemoveRange(commentsToDelete);
            await _context.SaveChangesAsync();

            var likesToDelete = await _context.Likes
                    .Where(like => like.PostId == idToDelete)
                    .ToListAsync();

            _context.Likes.RemoveRange(likesToDelete);
            await _context.SaveChangesAsync();

            int rowsAffected = await _context.SocialPosts
                                 .Where(post => post.Id == idToDelete)
                                 .ExecuteDeleteAsync();

            return rowsAffected > 0; // Returns true if a row was deleted, false otherwise
        }

        public async Task<bool> CommentPost(int postId, int userId, string content, DateTime timeStamp)
        {
            SocialPostModel? postToComment = await _context.SocialPosts.FirstOrDefaultAsync(p => p.Id == postId);
            CommentModel commentToAdd = new CommentModel();

            commentToAdd.PostId = postId;
            commentToAdd.UserId = userId;
            commentToAdd.Content = content;
            commentToAdd.TimeStamp = timeStamp;

            if (postToComment == null) return false;

            postToComment.Comments.Add(commentToAdd);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<CommentModel>> GetPostComments(int postId)
        {
            return await _context.SocialPosts
                     .Where(p => p.Id == postId)
                     .SelectMany(p => p.Comments)
                     .ToListAsync();
        }

        public async Task<bool> DeleteComment(int commentId)
        {
            int rowsAffected = await _context.SocialPosts
                     .SelectMany(p => p.Comments)
                     .Where(p => p.Id == commentId)
                     .ExecuteDeleteAsync();

            return rowsAffected > 0; // Returns true if a row was deleted, false otherwise
        }

        public async Task<bool> RemoveLike(int likeId)
        {
            int rowsAffected = await _context.SocialPosts
                .SelectMany(p => p.Likes)
                .Where(like => like.Id == likeId)
                .ExecuteDeleteAsync();

            return rowsAffected > 0;
        }

        public async Task<bool> UpdatePost(int id, string content, string imageUrl)
        {
            SocialPostModel? postToUpdate = await _context.SocialPosts.FirstOrDefaultAsync(p => p.Id == id);

            if (postToUpdate == null) return false;

            postToUpdate.Content = content;
            postToUpdate.ImageUrl = imageUrl;

            await _context.SaveChangesAsync();
            return true;
        }

        public Task<bool> Update(int id, SocialPostModel model)
        {
            throw new NotImplementedException();
        }
    }
}
