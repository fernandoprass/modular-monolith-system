using Shared.Domain.Interfaces;

namespace Sentinel.Domain.Interfaces;

public interface ISentinelUnitOfWork : IUnitOfWork
{
   IAuditLogRepository AuditLogs { get; }
   ISystemLogRepository SystemLogs { get; }
}
