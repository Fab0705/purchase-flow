namespace backend.Common.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(Guid UserId, string Email, string Role, Guid? LocationId);
        string GenerateRefreshToken();
    }
}