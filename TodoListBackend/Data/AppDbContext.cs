using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TodoListBackend.Models;

namespace TodoListBackend.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public DbSet<TodoTask> TodoTasks { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Ensure Identity configuration is applied

            modelBuilder.Entity<TodoTask>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Optional: Cascade delete when user is removed
        }
    }
}
