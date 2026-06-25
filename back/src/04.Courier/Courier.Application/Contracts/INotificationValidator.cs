using Courier.Domain.DTOs.Requests;
using Myce.Response;

namespace Courier.Application.Contracts;

public interface INotificationValidator
{
   Result ValidateSearch(NotificationSearchRequest request);
}
