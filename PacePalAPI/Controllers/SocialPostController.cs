using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using PacePalAPI.Models;
using PacePalAPI.Requests;
using PacePalAPI.Services.SocialService;
using PacePalAPI.Services.UserService;
using System.Net.Security;

namespace PacePalAPI.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class SocialPostController : ControllerBase
    {
        ISocialPostCollectionService _socialPostCollectionService;

        public SocialPostController(ISocialPostCollectionService socialPostService)
        {
            _socialPostCollectionService = socialPostService ?? throw new ArgumentNullException(nameof(SocialPostService));
        }

        [HttpGet("getPost/id")]
        public async Task<IActionResult> GetPost(int id)
        {
            SocialPostModel? postFound = await _socialPostCollectionService.Get(id);

            if (postFound == null) return BadRequest($"There is no post with id: {id}");

            return Ok(postFound);
        }

        [HttpGet("getPosts/")]
        public async Task<IActionResult> GetPosts()
        {
            List<SocialPostModel>? posts = await _socialPostCollectionService.GetAll();

            if (posts == null) return BadRequest("There are no posts available.");

            return Ok(posts);
        }

        [HttpGet("getPostComments/id")]
        public async Task<IActionResult> GetPostComments(int postId)
        {
            List<CommentModel> comments = await _socialPostCollectionService.GetPostComments(postId);

            return Ok(comments);
        }

        [HttpGet("getUserPosts/")]
        public async Task<List<SocialPostModel>> GetUserPosts(int userId)
        {
            return await _socialPostCollectionService.GetUserPosts(userId);
        }

        [HttpPost("addPost/")]
        public async Task<IActionResult> AddPost([FromBody] SocialPostModel socialPost)
        {
            bool result = await _socialPostCollectionService.Create(socialPost);

            if (!result) return BadRequest("Error while adding post.");

            return Ok(result);
        }

        [HttpPost("updatePost/")]
        public async Task<IActionResult> UpdatePost([FromBody] UpdateSocialPostRequest updateInfo)
        {
            bool result = await _socialPostCollectionService.Update(updateInfo.id, updateInfo.SocialPost);

            if (!result) return BadRequest("Error while updating post.");

            return Ok(result);
        }

        [HttpPost("likePost/")]
        public async Task<IActionResult> LikePost([FromBody] LikeDto likeDto)
        {
            bool result = await _socialPostCollectionService.LikePost(likeDto.PostId, likeDto.UserId);

            if (!result) return BadRequest("Error while adding like to a post.");

            return Ok(result);
        }

        [HttpPost("commentPost/")]
        public async Task<IActionResult> CommentPost([FromBody] CommentDto comment)
        {
            bool result = await _socialPostCollectionService.CommentPost(comment.PostId, comment.UserId, comment.Content, comment.TimeStamp);

            if (!result) return BadRequest("Error while adding comment to a post.");

            return Ok(result);
        }

        [HttpDelete("deleteComment/id")]
        public async Task<IActionResult> DeleteComment([FromBody] int idToDelete)
        {
            bool hasDeleted = await _socialPostCollectionService.DeleteComment(idToDelete);

            return Ok(hasDeleted);
        }

        [HttpDelete("deletePost/id")]
        public async Task<IActionResult> DeletePost([FromBody] int idToDelete)
        {
            bool hasDeleted = await _socialPostCollectionService.Delete(idToDelete);

            return Ok(hasDeleted);
        }

        [HttpDelete("deleteLike/id")]
        public async Task<IActionResult> DeleteLike([FromBody] int idToDelete)
        {
            bool hasDeleted = await _socialPostCollectionService.RemoveLike(idToDelete);

            return Ok(hasDeleted);
        }
    }
}
