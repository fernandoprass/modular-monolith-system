namespace Shared.Application.Contracts;

public interface IUserContext
{
   bool IsAuthenticated { get; }
   bool IsSystemAdmin { get; }
   bool IsSupportUser { get; }
   bool IsOrganizationAdmin { get; }
   string? IpAddress { get; }
   string? UserAgent { get; }
   string Language { get; }
   string? UserEmail { get; }
   Guid UserId { get; }
   Guid OrganizationId { get; }
   IEnumerable<string> Roles { get; }
}
