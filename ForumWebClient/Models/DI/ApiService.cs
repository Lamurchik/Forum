using System.Text.Json;

namespace ForumWebClient.Models.DI
{
    public class ApiService
    {
        private string _jwtToken;

        private string _hostName;
        private readonly HttpClient _httpClient;
        
        public ApiService(HttpClient httpClient, string hostName)
        {
            _httpClient = httpClient;
            _hostName = hostName;
        }


        #region post
        public async Task<List<Post>> GetUserPostsAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"https://{_hostName}/api/Post/GetUserPosts?userID={userId}");
            response.EnsureSuccessStatusCode();

            var posts = await response.Content.ReadFromJsonAsync<List<Post>>();
            
            return posts;
        }
        #endregion




    }
}
