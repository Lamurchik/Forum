using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ForumWebClient.Models
{
    public class Post
    {   
        public int PostId { get; set; } // Уникальный идентификатор поста     
        public int UserAuthorId { get; set; } // Идентификатор автора поста (ссылается на пользователя)
        public string PostTitle { get; set; } // Заголовок поста
        public string PostBody { get; set; } // Основное содержание поста
        public string PostSubtitle { get; set; } // Подзаголовок поста
    }
}
