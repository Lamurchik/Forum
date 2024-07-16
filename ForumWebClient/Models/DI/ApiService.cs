

using System.Net.Http.Headers;
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

        public async Task<string> GetPostImage64Async(int postId) //тест
        {
            var response = await _httpClient.GetAsync($"https://{_hostName}/api/Post/GetPostImage?postId={postId}");
            response.EnsureSuccessStatusCode();

            var imageBytes = await response.Content.ReadAsByteArrayAsync();
            var base64Image = Convert.ToBase64String(imageBytes);
            return base64Image;
        }





        public async Task<HttpResponseMessage> CreatePostAsync(Post post, IFormFile? titleImageFile, string jwtToken)
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
        public async Task<LogimInfo> LoginAsync(string userName, string password)
        {
            var url = $"https://{_hostName}/api/User/Login?loginName={userName}&password={password}";

            var parameters = new
            {
                userName,
                password
            };

            ///хуйня для даунов 
            ///




            var response = await _httpClient.PostAsJsonAsync(url, parameters);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var loginInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(jsonResponse);

            return new LogimInfo
            {
                JwtToken = loginInfo.token,
                UserId = loginInfo.userId
            };

        }

        public async Task RegisterAsync(string username, string password)
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

            var commentPayload = new
            {
                id = comment.Id, // Assuming the Comment model has an Id property
                text = comment.Text,
                parentCommentId = comment.ParentCommentId,
                userId = comment.UserId,
                postId = comment.PostId
            };

            // Сериализация объекта в JSON
            var jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(commentPayload);

            // Создание JSON-контента
            var jsonContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, uriBuilder.ToString())
            {
                Content = jsonContent
            };

            // Добавление заголовка авторизации
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            // Логирование для отладки
            Console.WriteLine("Request URI: " + uriBuilder.ToString());
            Console.WriteLine("Request Payload: " + jsonPayload);
            Console.WriteLine("Request Headers: " + request.Headers.ToString());

            // Отправка запроса
            var response = await _httpClient.SendAsync(request);

            // Логирование для отладки
            Console.WriteLine("Response Status Code: " + response.StatusCode);
            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response Content: " + responseContent);
            }

            return response;

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
