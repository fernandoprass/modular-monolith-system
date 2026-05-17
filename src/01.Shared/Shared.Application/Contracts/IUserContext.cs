namespace Shared.Application.Contracts;

public interface IUserContext
{
   bool IsAuthenticated { get; }
   bool IsSystemAdmin { get; }
   string? IpAddress { get; }
   string? UserAgent { get; }
   Guid UserId { get; }
   Guid UserOwnerId { get; }
   IEnumerable<string> Roles { get; }
}
