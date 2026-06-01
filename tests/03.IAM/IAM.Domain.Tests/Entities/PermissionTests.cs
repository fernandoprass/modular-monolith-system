using FluentAssertions;
using IAM.Domain.Entities;

namespace IAM.Domain.Tests.Entities;

public class PermissionTests
{
   [Theory]
   [InlineData("IAM", "Users", "Create", "iam.users.create")]
   [InlineData("Sentinel", "AuditLogs", "View", "sentinel.auditlogs.view")]
   public void BuildCode_ShouldReturnLowercasePermissionCode(
      string module,
      string resource,
      string action,
      string expected)
   {
      var result = Permission.BuildCode(module, resource, action);

      result.Should().Be(expected);
   }
}
