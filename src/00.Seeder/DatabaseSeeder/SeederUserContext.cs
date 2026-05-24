using Shared.Application.Contracts;

namespace DatabaseSeeder;

public class SeederUserContext : IUserContext
{
   public bool IsAuthenticated => true;
   public bool IsSystemAdmin => true;
   public Guid UserId => Guid.Empty;
   public Guid UserOwnerId => Guid.Empty;
   public string? UserName => "DatabaseSeeder";
   public string Language => "en";
   public string? IpAddress => null;
   public string? UserAgent => "DatabaseSeeder";
   public IEnumerable<string> Roles => [];
}
