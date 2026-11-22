using Shared.Application.Contracts;
using Shared.Domain;
using Shared.Domain.Entities;
using Shared.Domain.Events;
using Shared.Domain.Interfaces;
using System.Text.Json;

namespace Shared.Application.Services;

internal class LookupAuditLogger(IEventPublisher publisher, IUserContext userContext)
{
   public Task LogAsync(Lookup lookup, string action, CancellationToken cancellationToken = default) =>
      publisher.PublishAuditLogEventAsync(new AuditLogEvent
      {
         Module = SharedConst.System.ModuleName.ToLowerInvariant(), Feature = SharedConst.Logger.Feature.Lookups,
         Action = action, Description = $"Lookup {action}.", TargetId = lookup.Id,
         UserId = userContext.UserId, OrganizationId = lookup.OrganizationId ?? Guid.Empty,
         IpAddress = userContext.IpAddress, UserAgent = userContext.UserAgent,
         Metadata = JsonSerializer.Serialize(new { lookup.Type, lookup.Code, lookup.ParentId, lookup.OrganizationId })
      }, cancellationToken);
}
