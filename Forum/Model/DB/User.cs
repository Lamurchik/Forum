using System.ComponentModel.DataAnnotations;

namespace Forum.Model.DB
{
    public class User
    {
        [Key]
        public int UserId { get; set; } // Уникальный идентификатор пользователя

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } // Имя пользователя

        [Required]
        public string Password { get; set; } // Пароль пользователя

        public ICollection<Post> Posts { get; set; } = new List<Post>(); // Коллекция постов пользователя

        public ICollection<Comment> Comments { get; set; } = new List<Comment>(); // Коллекция комментариев пользователя
    }
}

