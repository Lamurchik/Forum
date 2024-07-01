using Forum.Model.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;

namespace Forum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ForumDBContext _context;
        public CommentController(ForumDBContext context) 
        {
            _context = context;
        }

        [HttpGet("GetComment")]
        public async Task<IActionResult> GetComment(int comentId)
        {
            var com = await _context.Comments.FirstOrDefaultAsync(c=> c.Id == comentId);
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
        public async Task<IActionResult> DelateComment(int commentId)
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
