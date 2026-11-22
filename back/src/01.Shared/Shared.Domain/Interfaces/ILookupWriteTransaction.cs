namespace Shared.Domain.Interfaces;

internal interface ILookupWriteTransaction : IAsyncDisposable
{
   Task CommitAsync(CancellationToken cancellationToken = default);
}
