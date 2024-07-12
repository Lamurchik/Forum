using Forum.Model.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.Design;
using System.Text.Json;

namespace Forum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ForumDBContext _context;
        private readonly IDistributedCache _cache;
        private readonly string _redisKeyComment = "Comment:";
        public CommentController(ForumDBContext context, IDistributedCache cache) 
        {
            _context = context;
            _cache = cache;
        }


        [HttpGet("GetPostComents")]
        public async Task<IActionResult> GetPostComents(int postId)//тут редис нужен 
        {

            var cachedComment = await _cache.GetStringAsync($"{_redisKeyComment}All{postId}");

            if (cachedComment != null)
            {
                var resRedis = JsonSerializer.Deserialize<List<Comment>>(cachedComment);
                Console.WriteLine("redis get comments");
                return Ok(resRedis);

            }



            List<Comment>? comments = null;
            await Task.Run(() => { comments = _context.Posts.FirstOrDefault(i => i.PostId == postId)?.Comments.ToList(); });
            if (comments != null)
            {
                var serializedPosts = JsonSerializer.Serialize(comments);
                await _cache.SetStringAsync($"{_redisKeyComment}All{postId}", serializedPosts, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2) // Настройте время хранения в кэше
                });
                Console.WriteLine("redis send comments");
                return Ok(comments);
            }
                
               
            return NotFound();
        }


        [HttpGet("GetComment")]
        public async Task<IActionResult> GetComment(int commentId)
        {
            var com = await _context.Comments.FirstOrDefaultAsync(c=> c.Id == commentId);
            if (com == null) return NotFound();
            return Ok(com);
        }






        [HttpPost("SendComment")]
        public async Task<IActionResult> SendComment(Comment comment)
        {
            comment.CommentDate = DateTime.UtcNow;
            comment.CommentTime = DateTime.Now.TimeOfDay;

            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("DelateComment")]
        public async Task<IActionResult> DelateComment(int commentId)//не удалил из кеша 
        {
            var DelCom =await _context.Comments.FindAsync(commentId);
            if(DelCom!=null)
            {
                 _context.Remove(DelCom);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }
        [HttpPut("UpdateComent")]
        public async Task<IActionResult> UpdateComment(int commentId, string updateText)
        {
            var updateCom = await _context.Comments.FindAsync(commentId);
            if(updateCom!=null)
            {
                updateCom.Text= updateText;
                await _context.SaveChangesAsync();
                return Ok();
            }

            return NotFound();

        }


    }
}
