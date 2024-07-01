using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Forum.Model.DB
{
    public class Comment
    {
        [Key]
        public int Id { get; set; } // Уникальный идентификатор комментария

        [Required]
        public string Text { get; set; } // Текст комментария

        public int? ParentCommentId { get; set; } // Идентификатор родительского комментария (если это ответ)

        [ForeignKey("User")]
        public int UserId { get; set; } // Идентификатор пользователя, оставившего комментарий

        [ForeignKey("Post")]
        public int PostId { get; set; } // Идентификатор поста, к которому относится комментарий


        [JsonIgnore]
        [Required]
        public DateTime CommentDate { get; set; } // Дата комментария

        [JsonIgnore]
        [Required]
        public TimeSpan CommentTime { get; set; } // Время комментария

        [JsonIgnore]
        [ForeignKey("ParentCommentId")]
        public Comment ParentComment { get; set; } // Связь с родительским комментарием (если это ответ)

        [JsonIgnore]
        public Post Post { get; set; } // Связь с постом

        [JsonIgnore]
        public User User { get; set; } // Связь с пользователем

        public ICollection<Comment> Replies { get; set; } = new List<Comment>(); // Коллекция ответов на комментарий
    }
}

