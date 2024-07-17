using Forum.Model.DB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.Design;
using System.Security.Claims;
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
        private readonly ILogger<CommentController> _logger;
        public CommentController(ForumDBContext context, IDistributedCache cache, ILogger<CommentController> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }


        [HttpGet("GetPostComents")]
        public async Task<IActionResult> GetPostComents(int postId,bool updateCashe= false)//тут редис нужен 
        {

            if(!updateCashe) 
            {
                var cachedComment = await _cache.GetStringAsync($"{_redisKeyComment}All{postId}");

                if (cachedComment != null)
                {
                    var resRedis = JsonSerializer.Deserialize<List<Comment>>(cachedComment);
                    Console.WriteLine("redis get comments");
                    return Ok(resRedis);

                }
            }
            



            List<Comment>? comments = null;
            comments  =await _context.Comments.Where(c=> c.PostId == postId).ToListAsync();
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

            await _cache.SetStringAsync($"{_redisKeyComment}{commentId}", JsonSerializer.Serialize(com), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2) // Настройте время хранения в кэше
            });

            return Ok(com);
        }





        [Authorize(Roles = "User")]
        [HttpPost("SendComment")]
        public async Task<IActionResult> SendComment(Comment comment)
        {
            comment.CommentDate = DateTime.UtcNow;
            comment.CommentTime = DateTime.Now.TimeOfDay;

            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            await GetPostComents(comment.PostId, true);
            return Ok();
        }

        [Authorize(Roles = "User")]
        [HttpDelete("DelateComment")]
        public async Task<IActionResult> DelateComment(int commentId)//не удалил из кеша 
        {
            var delCom =await _context.Comments.FindAsync(commentId);
            if(delCom!=null)
            {

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (delCom.UserId != Convert.ToUInt32(userId))
                {
                    _logger.LogWarning("attempt to influence a comment by an unauthorized person");
                    return Forbid("You are not authorized to update this post. It not you post");
                }

                _context.Remove(delCom);
                await _context.SaveChangesAsync();
                await _cache.RemoveAsync($"{_redisKeyComment}{commentId}");
                return Ok();
            }
            return NotFound();
        }
        [Authorize(Roles = "User")]
        [HttpPut("UpdateComent")]
        public async Task<IActionResult> UpdateComment(int commentId, string updateText)
        {
            var updateCom = await _context.Comments.FindAsync(commentId);
            if(updateCom!=null)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (updateCom.UserId != Convert.ToUInt32(userId))
                {
                    _logger.LogWarning("attempt to influence a comment by an unauthorized person");
                    return Forbid("You are not authorized to update this post. It not you post");
                }

                updateCom.Text= updateText;
                await _context.SaveChangesAsync();
                var updateSerilzed = JsonSerializer.Serialize(updateCom);
                await _cache.SetStringAsync($"{_redisKeyComment}{commentId}",updateSerilzed , new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2) // Настройте время хранения в кэше
                });
                return Ok();
            }

            return NotFound();

        }




    }
}
