using Microsoft.AspNetCore.Mvc;
using PacePalAPI.Models;
using PacePalAPI.Services.SocialService;
using PacePalAPI.Services.UserService;

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

        [HttpGet]
        public async Task<IActionResult> GetPosts()
        {
            List<SocialPostModel>? posts = await _socialPostCollectionService.GetAll();

            if (posts == null) return BadRequest("There are no posts available.");

            return Ok(posts);
        }


    }
}
