using Courier.Domain.Entities;
using Myce.Response;

namespace Courier.Application.Contracts;

public interface IEmailSender
{
   Task<Result> SendAsync(Email email, CancellationToken cancellationToken = default);
}
