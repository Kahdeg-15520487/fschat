using Microsoft.EntityFrameworkCore;

namespace be.DAL
{
    public class ChatDataContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }

        public ChatDataContext(DbContextOptions<ChatDataContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=chat;Username=postgres;Password=postgres");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserGroup>()
                .HasIndex(ug => ug.UserID);

            modelBuilder.Entity<UserGroup>()
                .HasIndex(ug => ug.GroupID);
        }
    }
}
