using Forum.Model.DB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection.Metadata.Ecma335;

namespace Forum.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly ForumDBContext _context;
        public PostController(ForumDBContext context)   
        {
            _context = context;
        }
        [HttpGet("GetPost")]
        public async Task<IActionResult> GetPost(int id)
        {
            Post? post=null;
            await Task.Run(() => { post = _context.Posts.FirstOrDefault(i => i.PostId == id); });
            if (post != null ) return Ok(post);
            return NotFound();
        }

        [Authorize("User")]
        [HttpPost("CreatePost")]
        public async Task<IActionResult> CreatePost(Post post)//без картинки 
        {
            await _context.Posts.AddAsync(post);
            _context.SaveChanges();
            return NoContent();
        }
        [HttpGet("GetUserPosts")]
        public async Task<IActionResult> GetUserPosts(int userID)
        {
            List<Post>? res = null;
            await Task.Run(() => { res = _context.Users.FirstOrDefault(i => i.UserId == userID)?.Posts.ToList(); });
            if (res != null) return Ok(res);
            return NotFound();
        }
        [HttpGet("GetPostComents")]
        public async Task<IActionResult> GetPostComents(int postId)
        {
            List<Comment>? comments=null;
            await Task.Run(() => { comments = _context.Posts.FirstOrDefault(i => i.PostId == postId)?.Comments.ToList(); });
            if(comments != null) return Ok(comments);
            return NotFound();
        }

        [HttpPut("UpdatePost")]
        public async Task<IActionResult> UpdatePost(Post newPost)
        {
            var post = _context.Posts.FirstOrDefault(i => i.PostId == newPost.PostId);
            if(post != null) 
            {
                post.PostSubtitle = newPost.PostSubtitle;
                post.PostTitle = newPost.PostTitle;
                post.PostBody = newPost.PostBody;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            return NotFound();          
        }
        [HttpDelete("DelatePost")]
        public async Task<IActionResult> DelatePost(int postId)
        {
             var post = await _context.Posts.FindAsync(postId);
            if(post != null)
            {
                _context.Remove(post);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            return NotFound();
        }
    }  
}
