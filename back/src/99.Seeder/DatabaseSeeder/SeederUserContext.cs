using Shared.Application.Contracts;

namespace DatabaseSeeder;

public class SeederUserContext : IUserContext
{
   public bool IsAuthenticated => true;
   public bool IsSystemAdmin => true;
   public bool IsSupportUser => false;
   public bool IsOrganizationAdmin => false;
   public Guid UserId => Guid.Empty;
   public Guid OrganizationId => Guid.Empty;
   public string? UserName => "DatabaseSeeder";
   public string Language => "en";
   public string? IpAddress => null;
   public string? UserAgent => "DatabaseSeeder";
   public string? UserEmail => "user@seeder.com";
   public IEnumerable<string> Roles => [];
}
