namespace IAM.Application.Contracts;

public interface IIamEmailNotifier
{
   Task NotifyAsync(
      string templateKey,
      Guid organizationId,
      Guid userId,
      string recipient,
      string feature,
      IReadOnlyDictionary<string, string>? values = null,
      CancellationToken cancellationToken = default);
}
