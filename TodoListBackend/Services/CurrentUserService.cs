using System.Security.Claims;

namespace TodoListBackend.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user == null)
                {
                    return null;
                }

                // Try retrieving user ID from standard claim types
                return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? user.FindFirst("sub")?.Value; // Fallback to "sub" (subject claim)
            }
        }

        public string? UserName
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                return user?.FindFirst(ClaimTypes.Name)?.Value;
            }
        }

        public string? GetCurrentUserId()
        {
            var userId = UserId;
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("User ID is NULL! Ensure the user is authenticated and claims are set.");
                return null;
            }

            Console.WriteLine($"Retrieved User ID: {userId}");
            return userId;
        }
    }
}
