using Sentinel.Domain.Entities;
using Shared.Domain.Enums;

namespace Sentinel.Infrastructure.Tests;

public class SystemLogTests
{
   [Fact]
   public void CreateSetsStatus()
   {
      var systemLog = SystemLog.Create(
         DateTime.UtcNow,
         SystemLogLevel.Error,
         SystemLogStatus.Unauthorized,
         "Sentinel.Tests",
         "Unauthorized request",
         null,
         null,
         null,
         null,
         null,
         "{}");

      Assert.Equal(SystemLogStatus.Unauthorized, systemLog.Status);
   }
}
