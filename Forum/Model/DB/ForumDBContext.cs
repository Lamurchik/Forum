using Microsoft.EntityFrameworkCore;

namespace Forum.Model.DB
{
    public class ForumDBContext : DbContext
    {
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }

        public ForumDBContext(DbContextOptions<ForumDBContext> options) : base(options) { }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Здесь можно добавить дополнительные настройки, если необходимо
        }
    }
}

