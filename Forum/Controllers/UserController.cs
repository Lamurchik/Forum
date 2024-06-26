using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpPost]
        public async void Login(string loginName, string password)
        {

        }
        public async void Logout()
        {

        }
        public async void Register(string loginName, string password)
        {

        }

    }
}
