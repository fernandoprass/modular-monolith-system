using Courier.Domain.DTOs.Requests;
using Myce.Response;

namespace Courier.Application.Contracts;

public interface IEmailValidator
{
   Result ValidateCreate(EmailCreateRequest request);
   Result ValidateSearch(EmailSearchRequest request);
}
