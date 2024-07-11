namespace ForumWebClient.Models
{
    public class Comment
    {
        
        public int Id { get; set; } // Уникальный идентификатор комментария    
        public string Text { get; set; } // Текст комментария

        public int? ParentCommentId { get; set; } // Идентификатор родительского комментария (если это ответ)  
        public int UserId { get; set; } // Идентификатор пользователя, оставившего комментарий     
        public int PostId { get; set; } // Идентификатор поста, к которому относится комментарий

    }
}
