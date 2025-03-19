using Microsoft.EntityFrameworkCore;
using TodoApp.Models;

namespace TodoApp.Database
{
    public class TodoDbContext : DbContext
    {
        public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
        {
        }

        public DbSet<TodoTask> Tasks { get; set; }
        public DbSet<TaskDependency> TaskDependencies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TaskDependency>()
                .HasOne(td => td.DependentTask)
                .WithMany(t => t.DependencyOf)
                .HasForeignKey(td => td.DependentTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskDependency>()
                .HasOne(td => td.DependencyTask)
                .WithMany(t => t.DependentOn)
                .HasForeignKey(td => td.DependencyTaskId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Prevent circular reference 
            modelBuilder.Entity<TaskDependency>()
                .HasIndex(td => new { td.DependentTaskId, td.DependencyTaskId })
                .IsUnique();
        }
    }
}