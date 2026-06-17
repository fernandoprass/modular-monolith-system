using Myce.FluentValidator;
using Myce.Response;
using Shared.Application.Contracts;
using Shared.Domain;
using Shared.Domain.DTOs.Requests;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shared.Domain.Messages;
using System.Globalization;

namespace Shared.Application.Validators;

public class ParameterValidator : IParameterValidator
{
   private static void MemberKeyTemplate<T>(RuleBuilder<T, string> rb) where T : class
                     => rb.IsRequired().MinLength(2).IsAlphaNumeric();

   public Result ValidateCreate(ParameterCreateRequest request, bool keyExists)
   {
      var validator = new FluentValidator<ParameterCreateRequest>()
         .RuleFor(x => x.Module).ApplyTemplate(MemberKeyTemplate)
         .RuleFor(x => x.Group).ApplyTemplate(MemberKeyTemplate)
         .RuleFor(x => x.Name).ApplyTemplate(MemberKeyTemplate)
         .RuleFor(x => x.Description).IsRequired()
         .RuleFor(x => x.Type).IsRequired()
         .RuleFor(x => x.OverrideType).IsRequired().IsInEnum()
         .RuleFor(x => x.ValidationErrorCustomMessage)
            .If(x => !string.IsNullOrEmpty(x.ValidationRegex), x => x.IsRequired())
         .RuleFor(x => x.Value)
            .IsRequired()
            .If(x => !string.IsNullOrEmpty(x.ValidationRegex),
                x => x.Matches(request.ValidationRegex, new ParameterInvalidValueError(request?.ValidationErrorCustomMessage)))
         .RuleForValue(keyExists).IsFalse(new ParameterDuplicatedError(request.Module, request.Group, request.Name));

      var isValid = validator.Validate(request);
      if (!isValid) return Result.Failure(validator.Messages);

      return ValidateValueFormat(request.Value, request.Type);
   }

   public Result ValidateUpdate(bool parameterExists, bool keyExists, ParameterUpdateRequest request)
   {
      var validator = new FluentValidator<ParameterUpdateRequest>()
         .RuleFor(x => x.Module).ApplyTemplate(MemberKeyTemplate)
         .RuleFor(x => x.Group).ApplyTemplate(MemberKeyTemplate)
         .RuleFor(x => x.Name).ApplyTemplate(MemberKeyTemplate)
         .RuleFor(x => x.Description).IsRequired()
         .RuleFor(x => x.Type).IsRequired()
         .RuleFor(x => x.OverrideType).IsRequired().IsInEnum()
         .RuleFor(x => x.ValidationErrorCustomMessage)
            .If(x => !string.IsNullOrEmpty(x.ValidationRegex), x => x.IsRequired())
         .RuleFor(x => x.Value)
            .IsRequired()
            .If(x => !string.IsNullOrEmpty(x.ValidationRegex),
                x => x.Matches(request.ValidationRegex, new ParameterInvalidValueError(request.ValidationErrorCustomMessage)))
         .RuleForValue(parameterExists).IsTrue(new NotFoundError(SharedConst.Entity.Parameter))
         .RuleForValue(keyExists).IsFalse(new ParameterDuplicatedError(request.Module, request.Group, request.Name));

      var isValid = validator.Validate(request);
      if (!isValid) return Result.Failure(validator.Messages);

      return ValidateValueFormat(request.Value, request.Type);
   }

   public Result ValidateOwnerUpdate(Parameter? parameter, ParameterOwnerUpdateRequest request)
   {
      var validator = new FluentValidator<ParameterOwnerUpdateRequest>()
         .RuleForValue(parameter).IsNotNull(new NotFoundError(SharedConst.Entity.Parameter))
         .RuleForValue(parameter?.OverrideType).IsNotEqualTo(ParameterOverrideType.None, new ParameterNotOwnerEditableError())
         .RuleFor(x => x.Value)
            .IsRequired()
            .If(x => !string.IsNullOrEmpty(parameter?.ValidationRegex),
                x => x.Matches(parameter?.ValidationRegex, new ParameterInvalidValueError(parameter?.ValidationErrorCustomMessage)));

      var isValid = validator.Validate(request);
      if (!isValid) return Result.Failure(validator.Messages);

      return ValidateValueFormat(request.Value, parameter.Type);
   }

   private static Result ValidateValueFormat(string value, ParameterType type)
   {
      bool isValid = type switch
      {
         ParameterType.Boolean => bool.TryParse(value, out _),
         ParameterType.Integer => int.TryParse(value, out _),
         ParameterType.Decimal => decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _),
         ParameterType.DateTime => DateTime.TryParse(value, out _),
         ParameterType.Date => DateTime.TryParse(value, out _),
         ParameterType.Time => TimeSpan.TryParse(value, out _),
         _ => true
      };

      if (!isValid)
      {
         return Result.Failure(new ParameterInvalidValueFormatError(type.ToString()));
      }

      return Result.Success();
   }
}
