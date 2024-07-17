using ForumWebClient.Models;
using ForumWebClient.Models.DI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ForumWebClient.Controllers
{
    public class MainController : Controller
    {
        private readonly ILogger<MainController> _logger;
        private readonly HttpClient _httpClient;
        private ApiService _apiService;
        private string jwtToken;
        private int userId;
        //где хранить 

        public MainController(ILogger<MainController> logger, HttpClient httpClient, ApiService apiService)
        {
            _logger = logger;
            _httpClient = httpClient;
            _apiService = apiService;
        }

        //[Route("Index")]
        public async Task<IActionResult> Index()//домашн€€ страница
        {
            int? userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
                return RedirectToAction("Login");

            var posts = await _apiService.GetUserPostsAsync(userId ?? 1);

            return View(posts);
        }
        //[Route("Login")]
        [HttpGet("Login")]
        public async Task<IActionResult> Login() 
        {
            return View();
        }


        [HttpPost("Login")]
        public async Task<IActionResult> Login(string userName, string password)
        {
            try
            {
                var result = await _apiService.LoginAsync(userName, password);


                HttpContext.Session.SetString("jwtToken", result.JwtToken);
                HttpContext.Session.SetInt32("userId", result.UserId);

                
                return RedirectToAction("Index");
            }
            catch (Exception ex) 
            {
                return RedirectToAction("Erorr"); 
            }
            return await Login();
        }



        [HttpGet("Register")]
        public async Task<IActionResult> Register()
        {
            return View();
        }



        [HttpPost("Register")]
        public async Task<IActionResult> Register(string userName, string password)
        {
            try
            {
                await _apiService.RegisterAsync(userName, password);

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Erorr");
            }
            return await Login();
        }


        [Route("PostPage")]
        public async Task<IActionResult> PostPage(int id)
        {
            var post =await _apiService.GetPostAsync(id);
            var comments =await _apiService.GetPostCommentAsyn(id);
            IFormFile? img;
            try 
            {  img = await _apiService.GetPostImageAsync(id); }
            catch (Exception ex) { img = null; Console.WriteLine(ex.Message); }


            string base64Image = "";
            try { base64Image = await _apiService.GetPostImage64Async(id); }
            catch { }


   



            var postPage = new PostPage() { Comments = comments, Post = post, PostImage=img, Base64Image= base64Image };
            return View(postPage);
        }

        [HttpPost("SendComent")]
        public async Task<IActionResult> SendComent(int postId, string text, string returnUrl)//хуйн€ ебана€ не работает 
        {
            if (HttpContext.Session.GetString("jwtToken") == null || HttpContext.Session.GetString("jwtToken") == "")
                return RedirectToAction("Login");

            var comment = new Comment() { ParentCommentId = null, PostId = postId, Text = text, UserId = HttpContext.Session.GetInt32("userId")?? 0 };

            var res = await _apiService.SendCommentAsync(comment, HttpContext.Session.GetString("jwtToken"));

            return RedirectToAction("PostPage", new { id = postId}); 
        }

        [HttpGet("CreatePost")]
        public async Task<IActionResult> CreatePost()
        {
            if(HttpContext.Session.GetString("jwtToken") == null)
                return RedirectToAction("Login");
            return View();
        }

        [HttpPost("CreatePost")]
        public async Task<IActionResult> CreatePost([FromForm] Post post, IFormFile postImage)
        {
            if (ModelState.IsValid)
            {
                if (postImage != null && postImage.Length > 0)
                {
                    int? userId = HttpContext.Session.GetInt32("userId") ;
                    if(userId!=null  )
                    {
                        post.UserAuthorId = userId ?? 0;

                        if (HttpContext.Session.GetString("jwtToken") != null)
                        await _apiService.CreatePostAsync(post, postImage, HttpContext.Session.GetString("jwtToken") ?? "");
                    }
                    else
                    {
                        return RedirectToAction("Login");
                    }

                }
                

               

                return RedirectToAction("Index"); // или любой другой метод, который вы хотите вызвать после создани€ поста
            }
            return View(post);
        }

        [Route("PostDay")]
        public async Task<IActionResult> PostDay()
        {
            var posts =await  _apiService.GetPostsDayAsync(DateTime.UtcNow.ToString("yyyy-MM-dd"));
            return View(posts);

        }




        [Route("Erorr")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
