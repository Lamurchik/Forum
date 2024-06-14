using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Forum.Model.DB
{
    public class Post
    {
        [Key]
        public int PostId { get; set; } // Уникальный идентификатор поста

        [ForeignKey("User")]
        public int UserAuthorId { get; set; } // Идентификатор автора поста (ссылается на пользователя)

        [Required]
        [MaxLength(200)]
        public string PostTitle { get; set; } // Заголовок поста

        public string PostBody { get; set; } // Основное содержание поста
        public string PostSubtitle { get; set; } // Подзаголовок поста

        [Required]
        public DateTime PostDate { get; set; } // Дата публикации поста

        public string PostFilePatch { get; set; } // Путь к файлу поста (если есть)

        public User User { get; set; } // Связь с таблицей пользователей (автор поста)

        public ICollection<Comment> Comments { get; set; } = new List<Comment>(); // Коллекция комментариев к посту
    }
}

