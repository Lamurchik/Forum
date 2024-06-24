using Forum.Model.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Forum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly ForumDBContext _context;
        public PostController(ForumDBContext context)   
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetPost(int id)
        {
            Post? p=null;
            await Task.Run(() => { p = _context.Posts.FirstOrDefault(i => i.PostId == id); });
            if (p != null ) return Ok(p);
            return NotFound();
        }

        [HttpPost]
        public async void CreatePost(Post post)
        {
            await _context.Posts.AddAsync(post);         
        }

    }
}
