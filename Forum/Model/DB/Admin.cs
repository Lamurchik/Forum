using System.ComponentModel.DataAnnotations;

namespace Forum.Model.DB
{
    public class Admin
    {
        [Key]
        public int AdminId { get; set; } // Уникальный идентификатор администратора

        [Required]
        [MaxLength(100)]
        public string AdminName { get; set; } // Имя администратора

        [Required]
        public string Password { get; set; } // Пароль администратора
    }
}

