namespace TodoListBackend.Services
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? UserName { get; }
        string? GetCurrentUserId();
    }
}
