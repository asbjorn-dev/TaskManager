using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Data
{
    // makes session to the database 
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<Team> Teams => Set<Team>();
        public DbSet<TaskItem> TaskItems => Set<TaskItem>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Tag> Tags => Set<Tag>();
        // TODO: kig på at remove TaskTags DbSet tror det kan gøres unødvendigt
        public DbSet<TaskTag> TaskTags => Set<TaskTag>();



        // konfigurer join entity for many-to-many relationship between TaskItem and Tag
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Many-to-many: TaskItem ↔ Tag
            modelBuilder.Entity<TaskTag>()
                .HasKey(tt => new { tt.TaskItemId, tt.TagId });

            modelBuilder.Entity<TaskTag>()
                .HasOne(tt => tt.TaskItem)
                .WithMany(t => t.TaskTags)
                .HasForeignKey(tt => tt.TaskItemId);

            modelBuilder.Entity<TaskTag>()
                .HasOne(tt => tt.Tag)
                .WithMany(tag => tag.TaskTags)
                .HasForeignKey(tt => tt.TagId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
