using Microsoft.AspNetCore.Mvc;
using Sentinel.API.Controllers;
using Sentinel.Domain;
using Shared.Infrastructure.Authorization;

namespace Sentinel.API.Tests;

public class LogControllerTests
{
   [Fact]
   public void GetAuditLogsByParamsUsesAuditLogPermission()
   {
      var method = typeof(LogController).GetMethod(nameof(LogController.GetAuditLogsByParams));

      var attribute = Assert.Single(method!.GetCustomAttributes(typeof(RequirePermissionAttribute), inherit: false).Cast<RequirePermissionAttribute>());

      Assert.Equal(SentinelPermission.AuditLogs.List, attribute.Permission);
   }

   [Fact]
   public void GetSystemLogsByParamsUsesSystemLogPermission()
   {
      var method = typeof(LogController).GetMethod(nameof(LogController.GetSystemLogsByParams));

      var attribute = Assert.Single(method!.GetCustomAttributes(typeof(RequirePermissionAttribute), inherit: false).Cast<RequirePermissionAttribute>());

      Assert.Equal(SentinelPermission.SystemLogs.List, attribute.Permission);
   }

   [Fact]
   public void GetAuditLogByIdUsesAuditLogViewPermission()
   {
      var method = typeof(LogController).GetMethod(nameof(LogController.GetAuditLogById));

      var attribute = Assert.Single(method!.GetCustomAttributes(typeof(RequirePermissionAttribute), inherit: false).Cast<RequirePermissionAttribute>());

      Assert.Equal(SentinelPermission.AuditLogs.View, attribute.Permission);
   }

   [Fact]
   public void GetSystemLogByIdUsesSystemLogViewPermission()
   {
      var method = typeof(LogController).GetMethod(nameof(LogController.GetSystemLogById));

      var attribute = Assert.Single(method!.GetCustomAttributes(typeof(RequirePermissionAttribute), inherit: false).Cast<RequirePermissionAttribute>());

      Assert.Equal(SentinelPermission.SystemLogs.View, attribute.Permission);
   }

   [Fact]
   public void EndpointsUseExpectedRoutes()
   {
      var auditMethod = typeof(LogController).GetMethod(nameof(LogController.GetAuditLogsByParams));
      var systemMethod = typeof(LogController).GetMethod(nameof(LogController.GetSystemLogsByParams));
      var auditByIdMethod = typeof(LogController).GetMethod(nameof(LogController.GetAuditLogById));
      var systemByIdMethod = typeof(LogController).GetMethod(nameof(LogController.GetSystemLogById));

      var auditRoute = Assert.Single(auditMethod!.GetCustomAttributes(typeof(HttpGetAttribute), inherit: false).Cast<HttpGetAttribute>());
      var systemRoute = Assert.Single(systemMethod!.GetCustomAttributes(typeof(HttpGetAttribute), inherit: false).Cast<HttpGetAttribute>());
      var auditByIdRoute = Assert.Single(auditByIdMethod!.GetCustomAttributes(typeof(HttpGetAttribute), inherit: false).Cast<HttpGetAttribute>());
      var systemByIdRoute = Assert.Single(systemByIdMethod!.GetCustomAttributes(typeof(HttpGetAttribute), inherit: false).Cast<HttpGetAttribute>());

      Assert.Equal("audit", auditRoute.Template);
      Assert.Equal("system", systemRoute.Template);
      Assert.Equal("audit/{id:guid}", auditByIdRoute.Template);
      Assert.Equal("system/{id:guid}", systemByIdRoute.Template);
   }
}
