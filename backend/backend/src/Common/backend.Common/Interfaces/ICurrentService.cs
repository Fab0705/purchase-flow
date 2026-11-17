namespace backend.Common.Interfaces
{
    public interface ICurrentService
    {
        Guid UserId { get; }
        string Role { get; }
        string? Email { get; }
        bool IsAuthenticated { get; }

        bool IsAdmin { get; }
        bool IsInRole(string role);
    }
}