﻿using System.Net.Http.Headers;
using System.Text;
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
        public async Task<Post> GetPostAsync(int postId)
        {
            var response = await _httpClient.GetAsync($"https://{_hostName}/api/Post/GetPost?id={postId}");
            response.EnsureSuccessStatusCode();
            var post = await response.Content.ReadFromJsonAsync<Post>();
            return post;
        }

        public async Task<IFormFile> GetPostImageAsync(int postId) //тест
        {
            var response = await _httpClient.GetAsync($"https://{_hostName}/api/Post/GetPostImage?postId={postId}");
            response.EnsureSuccessStatusCode();

            var img = await response.Content.ReadFromJsonAsync<IFormFile>();

            return img;
        }


        public async Task<HttpResponseMessage> CreatePostAsync(Post post, IFormFile titleImageFile, string jwtToken)
        {
            var uriBuilder = new UriBuilder($"https://{_hostName}/api/Post/CreatePost");

            using (var form = new MultipartFormDataContent())
            {
                form.Add(new StringContent(post.PostId.ToString()), "PostId");
                form.Add(new StringContent(post.UserAuthorId.ToString()), "UserAuthorId");
                form.Add(new StringContent(post.PostTitle), "PostTitle");
                form.Add(new StringContent(post.PostBody), "PostBody");
                form.Add(new StringContent(post.PostSubtitle), "PostSubtitle");

                if (titleImageFile != null)
                {
                    var stream = new MemoryStream();
                    await titleImageFile.CopyToAsync(stream);
                    stream.Position = 0;

                    var fileContent = new StreamContent(stream);
                    fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        Name = "titleImageFile",
                        FileName = titleImageFile.FileName
                    };
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(titleImageFile.ContentType);

                    form.Add(fileContent);
                }

                var request = new HttpRequestMessage(HttpMethod.Post, uriBuilder.ToString())
                {
                    Content = form
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                return await _httpClient.SendAsync(request);
            }
        }

        public async Task<HttpResponseMessage> UpdatePostAsync(Post newPost, string jwtToken)
        {
            var uriBuilder = new UriBuilder($"https://{_hostName}/api/Post/UpdatePost");

            var request = new HttpRequestMessage(HttpMethod.Put, uriBuilder.ToString())
            {
                Content = JsonContent.Create(newPost)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            return await _httpClient.SendAsync(request);
        }


        #endregion


        #region login
        public async Task<LogimInfo> Login(string username, string password)
        {
            var url = $"https://{_hostName}/api/User/Login?loginName={username}&password={password}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var loginfo = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);

            return new LogimInfo { JwtToken = loginfo.token, UserId = loginfo.userId };
        }

        public async Task Register(string username, string password)
        {
            var url = $"https://{_hostName}/api/User/Register?loginName={username}&{password}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
        }
        #endregion



        #region comment


        public async Task<List<Comment>> GetPostCommentAsyn(int postId)
        {
            var url = $"https://{_hostName}/api/Comment/GetPostComents?postId={postId}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();


            var result = await response.Content.ReadFromJsonAsync<List<Comment>>();

            return result;

        }

        public async Task<Comment> GetCommentAsync(int commentId)
        {
            var url = $"https://{_hostName}/api/Comment/GetComment?commentId={commentId}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<Comment>();

            return result;

        }

        public async Task<HttpResponseMessage> SendCommentAsync(Comment comment, string jwtToken)
        {
            var uriBuilder = new UriBuilder($"https://{_hostName}/api/Comment/SendComment");

            var request = new HttpRequestMessage(HttpMethod.Post, uriBuilder.ToString())
            {
                Content = JsonContent.Create(comment)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            return await _httpClient.SendAsync(request);
        }

        public async Task<HttpResponseMessage> UpdateCommentAsync(int commentId, string updateText, string jwtToken)
        {
            var uriBuilder = new UriBuilder($"https://{_hostName}/api/Comment/UpdateComment");

            var content = new StringContent(JsonSerializer.Serialize(new { commentId, updateText }), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Put, uriBuilder.ToString())
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            return await _httpClient.SendAsync(request);
        }



        #endregion


    }




    public class LogimInfo
    { 
        public string JwtToken {  get; set; }
        public int UserId { get; set; }

    }

}
