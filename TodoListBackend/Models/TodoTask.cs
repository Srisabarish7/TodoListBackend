using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoListBackend.Models
{
    public class TodoTask
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime ReminderDate { get; set; }
        public string? Priority { get; set; }

        // Foreign Key should be string to match IdentityUser.Id
        [ForeignKey("User")]
        public string UserId { get; set; }

        public virtual User User { get; set; }
    }
}
