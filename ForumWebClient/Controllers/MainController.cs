using ForumWebClient.Models;
using ForumWebClient.Models.DI;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ForumWebClient.Controllers
{
    public class MainController : Controller
    {
        private readonly ILogger<MainController> _logger;
        private readonly HttpClient _httpClient;
        private ApiService _apiService;

        public MainController(ILogger<MainController> logger, HttpClient httpClient, ApiService apiService)
        {
            _logger = logger;
            _httpClient = httpClient;
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            int userId = 1; // ”кажите реальный идентификатор пользовател€
            var posts = await _apiService.GetUserPostsAsync(userId);

            return View(posts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
