using Microsoft.EntityFrameworkCore;

namespace RealTimeChatSignalRLab.Models
{
    public class ChatDBContext : DbContext
    {
        public ChatDBContext(DbContextOptions<ChatDBContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Group>().ToTable("Group");
            modelBuilder.Entity<Message>().ToTable("Message");
            modelBuilder.Entity<UserGroup>().ToTable("UserGroup");

            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        }
    }
}
