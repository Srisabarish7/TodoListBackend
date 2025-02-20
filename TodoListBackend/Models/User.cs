using Microsoft.AspNetCore.Identity;

namespace TodoListBackend.Models
{
    public class User : IdentityUser
    {
        public string? FullName { get; set; }
    }
}
