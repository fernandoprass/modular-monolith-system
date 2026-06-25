using Courier.Application.Contracts;
using Courier.Domain.DTOs.Requests;
using Myce.FluentValidator;
using Myce.Response;

namespace Courier.Application.Validators;

public class NotificationValidator : INotificationValidator
{
   public Result ValidateSearch(NotificationSearchRequest request)
   {
      var validator = new FluentValidator<NotificationSearchRequest>()
         .RuleFor(x => x.DateFrom).IsRequired()
         .RuleFor(x => x.DateTo).IsRequired().IsGreaterThanOrEqualTo(x => x.DateFrom)
         .RuleFor(x => x.PageNumber).IsGreaterThan(0)
         .RuleFor(x => x.PageSize).IsGreaterThan(0);

      var isValid = validator.Validate(request);

      return isValid ? Result.Success() : Result.Failure(validator.Messages);
   }
}
