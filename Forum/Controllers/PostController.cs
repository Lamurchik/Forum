﻿using Forum.Model.DB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting.Internal;
using System.Reflection.Metadata.Ecma335;
using StackExchange.Redis;

namespace Forum.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly ForumDBContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public PostController(ForumDBContext context, IWebHostEnvironment hostingEnvironment)   
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }
        [HttpGet("GetPost")]
        public async Task<IActionResult> GetPost(int id)
        {
            Post? post=null;
            await Task.Run(() => { post = _context.Posts.FirstOrDefault(i => i.PostId == id); });
            if (post != null ) return Ok(post);
            return NotFound();
        }

        [Authorize(Roles = "User")]
        [HttpPost("CreatePost")]
        public async Task<IActionResult> CreatePost(Post post)//, IFormFile? titleImageFile
        {
            /*
            if (titleImageFile != null)
            {
                var webRootPath = _hostingEnvironment.WebRootPath;
                var imagesPath = Path.Combine(webRootPath, "images");

                // Убедитесь, что папка существует
                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                }

                // Формируем полный путь к файлу
                var filePath = Path.Combine(imagesPath, titleImageFile.FileName);

                // Сохраняем имя файла в свойство post
                post.PostFilePatch = titleImageFile.FileName;

                // Сохраняем файл на диск
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await titleImageFile.CopyToAsync(stream);
                }
            }
            */
            post.PostFilePatch = "";
            post.PostDate = DateTime.Now.ToUniversalTime();
            
            await _context.Posts.AddAsync(post);
            _context.SaveChanges();
            return Ok();
        }


        [HttpGet("GetUserPosts")]
        public async Task<IActionResult> GetUserPosts(int userID)
        {
            //редис 
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
            IDatabase db = redis.GetDatabase();



            //бд
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
