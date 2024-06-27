using Forum.Model.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        public async Task<IActionResult> GetComment(int comentId)
        {
            return Ok();
        }

    }
}
