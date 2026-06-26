using Asp.Versioning;
using Courier.Application.Contracts;
using Courier.Domain;
using Courier.Domain.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Infrastructure.Authorization;

namespace Courier.API.Controllers;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/notifications")]
[Authorize]
public class NotificationController(INotificationService notificationService) : BaseController
{
   private readonly INotificationService _notificationService = notificationService;

   [HttpGet("")]
   [RequirePermission(CourierPermission.Notifications.Read)]
   public async Task<IActionResult> GetByParams(
      [FromQuery] NotificationSearchRequest request,
      CancellationToken cancellationToken)
   {
      var result = await _notificationService.GetAsync(request, cancellationToken);
      return OkOrNotFound(result);
   }

   [HttpGet("unread-count")]
   [RequirePermission(CourierPermission.Notifications.Read)]
   public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
   {
      var result = await _notificationService.GetUnreadCountAsync(cancellationToken);
      return result.HasError ? BadRequest(result) : Ok(result);
   }

   [HttpPatch("{id:guid}/read")]
   [RequirePermission(CourierPermission.Notifications.Write)]
   public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
   {
      var result = await _notificationService.MarkAsReadAsync(id, cancellationToken);
      return result.HasError ? BadRequest(result) : Ok(result);
   }

   [HttpDelete("{id:guid}")]
   [RequirePermission(CourierPermission.Notifications.Write)]
   public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
   {
      var result = await _notificationService.DeleteAsync(id, cancellationToken);
      return result.HasError ? BadRequest(result) : Ok(result);
   }
}
