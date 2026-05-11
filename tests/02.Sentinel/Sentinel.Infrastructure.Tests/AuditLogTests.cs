using Sentinel.Domain.Entities;
using Shared.Domain.Enums;

namespace Sentinel.Infrastructure.Tests;

public class AuditLogTests
{
   [Fact]
   public void CreateSetsPrivacyLevel()
   {
      var auditLog = AuditLog.Create(
         DateTime.UtcNow,
         "IAM",
         "users",
         "created",
         AuditPrivacyLevel.Confidential,
         "Created user",
         Guid.CreateVersion7(),
         Guid.CreateVersion7(),
         "User",
         Guid.CreateVersion7(),
         "127.0.0.1",
         "test-agent",
         "{}");

      Assert.Equal(AuditPrivacyLevel.Confidential, auditLog.PrivacyLevel);
   }
}
