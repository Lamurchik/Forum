namespace ForumWebClient.Models
{
    public class PostPage
    {
        public Post Post { get; set; }

        public List<Comment> Comments { get; set; }


        public IFormFile? PostImage { get; set; }


        public string? Base64Image { get; set; }

        public Comment NewComment { get; set; }

    }
}
