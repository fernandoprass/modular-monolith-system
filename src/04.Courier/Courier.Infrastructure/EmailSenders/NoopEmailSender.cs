using Courier.Application.Contracts;
using Courier.Domain.Entities;
using Microsoft.Extensions.Logging;
using Myce.Response;

namespace Courier.Infrastructure.EmailSenders;

public class NoopEmailSender(ILogger<NoopEmailSender> logger) : IEmailSender
{
   private readonly ILogger<NoopEmailSender> _logger = logger;

   public Task<Result> SendAsync(Email email, CancellationToken cancellationToken = default)
   {
      _logger.LogInformation("Noop email sender accepted email {EmailId} to {Recipient}", email.Id, email.Recipient);
      return Task.FromResult(Result.Success());
   }
}
