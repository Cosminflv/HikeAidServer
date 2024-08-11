using Microsoft.AspNetCore.Mvc;
using PacePalAPI.Models;
using PacePalAPI.Requests;
using PacePalAPI.Services.SocialService;

[ApiController]
[Route("api/[controller]")]
public class SocialPostController : ControllerBase
{
    private readonly ISocialPostCollectionService _socialPostCollectionService;

    public SocialPostController(ISocialPostCollectionService socialPostService)
    {
        _socialPostCollectionService = socialPostService ?? throw new ArgumentNullException(nameof(socialPostService));
    }

    #region Get Endpoints

    // Get a specific post by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPost(int id)
    {
        var postFound = await _socialPostCollectionService.Get(id);

        if (postFound == null) return NotFound($"There is no post with id: {id}");

        return Ok(postFound);
    }

    // Get all posts
    [HttpGet]
    public async Task<IActionResult> GetPosts()
    {
        var posts = await _socialPostCollectionService.GetAll();

        if (posts == null || !posts.Any()) return NotFound("There are no posts available.");

        return Ok(posts);
    }

    // Get comments for a specific post
    [HttpGet("{postId}/comments")]
    public async Task<IActionResult> GetPostComments(int postId)
    {
        var comments = await _socialPostCollectionService.GetPostComments(postId);

        return Ok(comments);
    }

    // Get posts for a specific user
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserPosts(int userId)
    {
        var userPosts = await _socialPostCollectionService.GetUserPosts(userId);

        return Ok(userPosts);
    }

    #endregion

    #region Post Endpoints
    // Add a new post
    [HttpPost]
    public async Task<IActionResult> AddPost([FromBody] SocialPostModel socialPost)
    {
        var result = await _socialPostCollectionService.Create(socialPost);

        if (!result) return BadRequest("Error while adding post.");

        return Ok(result);
    }

    // Like a post
    [HttpPost("{postId}/like")]
    public async Task<IActionResult> LikePost(int postId, [FromBody] LikeDto likeDto)
    {
        var result = await _socialPostCollectionService.LikePost(postId, likeDto.UserId);

        if (!result) return BadRequest("Error while adding like to a post.");

        return Ok(result);
    }

    // Comment on a post
    [HttpPost("{postId}/comment")]
    public async Task<IActionResult> CommentPost(int postId, [FromBody] CommentDto comment)
    {
        var result = await _socialPostCollectionService.CommentPost(postId, comment.UserId, comment.Content, comment.TimeStamp);

        if (!result) return BadRequest("Error while adding comment to a post.");

        return Ok(result);
    }

    #endregion

    #region Put Endpoints
    // Update an existing post
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePost([FromBody] SocialPostDto updateInfo)
    {
        var result = await _socialPostCollectionService.UpdatePost(updateInfo.Id, updateInfo.Content, updateInfo.ImageUrl);

        if (!result) return BadRequest("Error while updating post.");

        return Ok(result);
    }

    #endregion

# region Delete Endpoints
    // Delete a comment by ID
    [HttpDelete("comments/{id}")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var hasDeleted = await _socialPostCollectionService.DeleteComment(id);

        if (!hasDeleted) return NotFound("Comment not found or already deleted.");

        return Ok(hasDeleted);
    }

    // Delete a post by ID
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(int id)
    {
        var hasDeleted = await _socialPostCollectionService.Delete(id);

        if (!hasDeleted) return NotFound("Post not found or already deleted.");

        return Ok(hasDeleted);
    }

    // Delete a like by ID
    [HttpDelete("likes/{id}")]
    public async Task<IActionResult> DeleteLike(int id)
    {
        var hasDeleted = await _socialPostCollectionService.RemoveLike(id);

        if (!hasDeleted) return NotFound("Like not found or already deleted.");

        return Ok(hasDeleted);
    }

    #endregion
}
