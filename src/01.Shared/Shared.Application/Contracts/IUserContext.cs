namespace Shared.Application.Contracts;

public interface IUserContext
{
   bool IsAuthenticated { get; }
   bool IsSystemAdmin { get; }
   Guid UserId { get; }
   Guid UserOwnerId { get; }
}
