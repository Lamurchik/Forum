using Forum.Model.DB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting.Internal;
using System.Reflection.Metadata.Ecma335;
using StackExchange.Redis;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Forms;

namespace Forum.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly ForumDBContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IDistributedCache _cache;
        private readonly string _redisKeyPost = "Post:";
        private readonly ILogger<PostController> _logger;
        public PostController(ForumDBContext context, IWebHostEnvironment hostingEnvironment, IDistributedCache cache, ILogger<PostController> logger)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _cache = cache;
            _logger = logger;
        }
        [HttpGet("GetPost")]
        public async Task<IActionResult> GetPost(int id)
        {
            var cachedPost = await _cache.GetStringAsync($"{_redisKeyPost}{id}");

            if (cachedPost != null)
            {
                var resRedis = JsonSerializer.Deserialize<Post>(cachedPost);
                Console.WriteLine("redis get post");
                return Ok(resRedis);

            }
            Post? post=null;
            await Task.Run(() => { post = _context.Posts.FirstOrDefault(i => i.PostId == id); });
            if (post != null)
            {
                var serializedPosts = JsonSerializer.Serialize(post);
                await _cache.SetStringAsync($"{_redisKeyPost}{id}", serializedPosts, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) // Настройте время хранения в кэше
                });


                return Ok(post);
            }
                return NotFound();
        }

       [Authorize(Roles = "User")]
        [HttpPost("CreatePost")]
        public async Task<IActionResult> CreatePost([FromForm] Post post,  IFormFile? titleImageFile) 
        {
            
            if (titleImageFile != null)
            {
                
                var webRootPath = _hostingEnvironment.WebRootPath;
                var imagesPath = System.IO.Path.Combine(webRootPath, "images");

                // Убедитесь, что папка существует
                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                }

                // Формируем полный путь к файлу
                var filePath = System.IO.Path.Combine(imagesPath, titleImageFile.FileName);

                // Сохраняем имя файла в свойство post
                post.PostFilePatch = titleImageFile.FileName;

                // Сохраняем файл на диск
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await titleImageFile.CopyToAsync(stream);
                }
            }
            else
             post.PostFilePatch = "";
            post.PostDate = DateTime.Now.ToUniversalTime();            
            await _context.Posts.AddAsync(post);
            _context.SaveChanges();

            await GetUserPosts(post.UserAuthorId, true);

            return Ok();
        }


        [HttpGet("GetPostImage")]
        public async Task<IActionResult> GetPostImage(int postId)
        {
            var fileName = (await _context.Posts.FirstOrDefaultAsync(Result => Result.PostId == postId))?.PostFilePatch;
            if(fileName == null)
                return NotFound();
            var filePath = System.IO.Path.Combine(System.IO.Path.Combine(Directory.GetCurrentDirectory(),"wwwroot", "images"), fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var mimeType = GetMimeType(filePath);
            return PhysicalFile(filePath, mimeType, System.IO.Path.GetFileName(filePath));
        }

        private string GetMimeType(string filePath)
        {
            var extension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream",
            };
        }


        [HttpGet("GetUserPosts")]
        public async Task<IActionResult> GetUserPosts(int userID, bool update=false) //нужен ли тут редис?
        {
            //редис 
            if(!update)
            {
                var cachedPost = await _cache.GetStringAsync($"{_redisKeyPost}All{userID}");

                if (cachedPost != null && cachedPost != "[]")
                {
                    var resRedis = JsonSerializer.Deserialize<List<Post>>(cachedPost);//list
                    Console.WriteLine("redis get post");
                    return Ok(resRedis);

                }
            }
            

            //бд
            List<Post>? res = null;
            await Task.Run(() => { res = _context.Posts.Where(i => i.UserAuthorId == userID)?.ToList(); });
            if (res != null)
            {
                var serializedPosts = JsonSerializer.Serialize(res);
                await _cache.SetStringAsync($"{_redisKeyPost}All{userID}", serializedPosts, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3) // Настройте время хранения в кэше
                });
                return Ok(res);

            }
                return NotFound();
        }

        [Authorize(Roles = "User")]
        [HttpPut("UpdatePost")]
        public async Task<IActionResult> UpdatePost(Post newPost)
        {
            var post = _context.Posts.FirstOrDefault(i => i.PostId == newPost.PostId);
            if(post != null) 
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (post.UserAuthorId != Convert.ToUInt32(userId))
                {
                    _logger.LogWarning("attempt to influence a post by an unauthorized person");
                    return Forbid("You are not authorized to update this post. It not you post");
                }

                post.PostSubtitle = newPost.PostSubtitle;
                post.PostTitle = newPost.PostTitle;
                post.PostBody = newPost.PostBody;
                await _context.SaveChangesAsync();

                await _cache.SetStringAsync($"{_redisKeyPost}{newPost.PostId}", JsonSerializer.Serialize(post));
                
                return NoContent();
            }
            return NotFound();          
        }

        [Authorize(Roles = "User")]
        [HttpDelete("DelatePost")]
        public async Task<IActionResult> DelatePost(int postId)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var post = await _context.Posts.FindAsync(postId);
            if(post != null)
            {
                if(post.UserAuthorId!=Convert.ToUInt32( userId) )
                {
                    _logger.LogWarning("attempt to influence a post by an unauthorized person");
                    return Forbid("You are not authorized to delete this post. It not you post");
                }


                _context.Remove(post);
                await _context.SaveChangesAsync();
                await _cache.RemoveAsync($"{_redisKeyPost}{postId}");
                return NoContent();
            }
            return NotFound();
        }

        [HttpGet("GetPostsDay")]
        public async Task<IActionResult> GetPostsDay(DateTime date)
        {


            var cachedPost = await _cache.GetStringAsync($"{_redisKeyPost}{date.ToString()}");

            if(cachedPost != null) 
            {
                var resRedis = JsonSerializer.Deserialize<List<Post>>(cachedPost);
                return Ok(resRedis);
            }




            var startOfDay = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            var endOfDay = DateTime.SpecifyKind(date.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

           
            var posts = await _context.Posts
                .Where(p => p.PostDate >= startOfDay && p.PostDate <= endOfDay)
                .ToListAsync();

            if (posts == null || !posts.Any())
            {
                return NotFound("No posts found for the specified date.");
            }

            var serializedPosts = JsonSerializer.Serialize(posts);
            _cache.SetString($"{_redisKeyPost}{date.ToString()}", serializedPosts, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1) // Настройте время хранения в кэше
            });

            return Ok(posts);
        }

    }  
}
