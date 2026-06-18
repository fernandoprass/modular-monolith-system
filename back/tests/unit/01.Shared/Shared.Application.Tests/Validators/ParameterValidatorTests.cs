using FluentAssertions;
using Shared.Application.Validators;
using Shared.Domain.DTOs.Requests;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shared.Domain.Messages;

namespace Shared.Application.Tests.Validators;

public class ParameterValidatorTests
{
   private readonly ParameterValidator _validator;

   public ParameterValidatorTests()
   {
      _validator = new ParameterValidator();
   }

   [Fact]
   public void ValidateCreate_WhenAllDataIsValid_ShouldBeSuccess()
   {
      var request = new ParameterCreateRequest("Sys", "Security", "MaxLoginAttempts", "Max allowed logins", "Parameter Description", ParameterType.Integer, "5", ParameterOverrideType.None, true);

      var result = _validator.ValidateCreate(request, keyExists: false);

      result.IsSuccess.Should().BeTrue();
   }

   [Fact]
   public void ValidateCreate_WhenKeyAlreadyExists_ShouldHaveError()
   {
      var request = new ParameterCreateRequest("Sys", "Security", "MaxLoginAttempts", "Max allowed logins", "Parameter Description", ParameterType.Decimal, "12.345", ParameterOverrideType.None, true);

      var result = _validator.ValidateCreate(request, keyExists: true);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(m => m is ParameterDuplicatedError);
   }

   [Fact]
   public void ValidateCreate_WhenRegexDoesNotMatch_ShouldHaveError()
   {
      var request = new ParameterCreateRequest("Sys", "Network", "IPAddress", "Server IP", "Parameter Description", ParameterType.String, "123.123", ParameterOverrideType.None, false, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$", "Invalid IP format.", null, null);

      var result = _validator.ValidateCreate(request, keyExists: false);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(m => m is ParameterInvalidValueError);
   }

   [Fact]
   public void ValidateUpdate_WhenAllDataIsValid_ShouldBeSuccess()
   {
      var request = new ParameterUpdateRequest("Sys", "Security", "MaxLoginAttempts", "Max allowed logins", "Parameter Description", ParameterType.String, "the string", ParameterOverrideType.None, true);

      var result = _validator.ValidateUpdate(parameterExists: true, keyExists: false, request);

      result.IsSuccess.Should().BeTrue();
   }

   [Fact]
   public void ValidateUpdate_WhenParameterDoesNotExist_ShouldHaveError()
   {
      var request = new ParameterUpdateRequest("Sys", "Security", "MaxLoginAttempts", "Max allowed logins", "Parameter Description", ParameterType.Boolean, "true", ParameterOverrideType.None, true);

      var result = _validator.ValidateUpdate(parameterExists: false, keyExists: false, request);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(m => m is NotFoundError);
   }

   [Fact]
   public void ValidateUpdate_WhenKeyAlreadyExists_ShouldHaveError()
   {
      var request = new ParameterUpdateRequest("Sys", "Security", "MaxLoginAttempts", "Max allowed logins", "Parameter Description", ParameterType.Date, "1978-02-22", ParameterOverrideType.None, true);

      var result = _validator.ValidateUpdate(parameterExists: true, keyExists: true, request);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(m => m is ParameterDuplicatedError);
   }

   [Fact]
   public void ValidateOwnerUpdate_WhenAllDataIsValid_ShouldBeSuccess()
   {
      var parameter = new Parameter { OverrideType = ParameterOverrideType.UserId, ValidationRegex = null };
      var request = new ParameterOwnerUpdateRequest("NewValidValue");

      var result = _validator.ValidateOwnerUpdate(parameter, request);

      result.IsSuccess.Should().BeTrue();
   }

   [Fact]
   public void ValidateOwnerUpdate_WhenParameterIsNull_ShouldHaveError()
   {
      var request = new ParameterOwnerUpdateRequest("NewValue");

      var result = _validator.ValidateOwnerUpdate(null, request);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(m => m is NotFoundError);
   }

   [Fact]
   public void ValidateOwnerUpdate_WhenOverrideTypeIsNone_ShouldHaveError()
   {
      var parameter = new Parameter { OverrideType = ParameterOverrideType.None };
      var request = new ParameterOwnerUpdateRequest("NewValue");

      var result = _validator.ValidateOwnerUpdate(parameter, request);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(m => m is ParameterNotOwnerEditableError);
   }

   [Fact]
   public void ValidateOwnerUpdate_WhenRegexDoesNotMatch_ShouldHaveError()
   {
      var parameter = new Parameter { OverrideType = ParameterOverrideType.OrganizationId, ValidationRegex = "^[0-9]+$", ValidationErrorCustomMessage = "Must be numbers" };
      var request = new ParameterOwnerUpdateRequest("NotANumber");

      var result = _validator.ValidateOwnerUpdate(parameter, request);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(m => m is ParameterInvalidValueError);
   }

   [Theory]
   [InlineData("A", "ValidGroup", "ValidName", true, false, false)]
   [InlineData("ValidModule", "B", "ValidName", true, false, false)]
   [InlineData("ValidModule", "ValidGroup", "C", true, false, false)]
   [InlineData("ValidModule", "ValidGroup", "ValidName", false, false, false)]
   [InlineData("ValidModule", "ValidGroup", "ValidName", true, true, false)]
   [InlineData("ValidModule", "ValidGroup", "ValidName", true, false, true)]
   public void ValidateUpdate_WhenApplyingTemplateValidation_ShouldReturnExpectedResult(
       string module,
       string group,
       string name,
       bool parameterExists,
       bool keyExists,
       bool expectedSuccess)
   {
      var request = new ParameterUpdateRequest(module, group, name, "title", "Valid description", ParameterType.String, "SomeValue", ParameterOverrideType.None, true);

      var result = _validator.ValidateUpdate(parameterExists, keyExists, request);

      result.IsSuccess.Should().Be(expectedSuccess);
   }

   [Theory]
   [InlineData(ParameterType.Boolean, "1", false)]
   [InlineData(ParameterType.Boolean, "true", true)]
   [InlineData(ParameterType.Boolean, "false", true)]
   [InlineData(ParameterType.Boolean, "not-a-bool", false)]
   [InlineData(ParameterType.Integer, "123", true)]
   [InlineData(ParameterType.Integer, "12.3", false)]
   [InlineData(ParameterType.Decimal, "123.45", true)]
   [InlineData(ParameterType.Decimal, "abc", false)]
   [InlineData(ParameterType.DateTime, "2023-12-01T15:30:00", true)]
   [InlineData(ParameterType.DateTime, "2023-12-01T15:30:00Z", true)]
   [InlineData(ParameterType.DateTime, "2023-12-01T15:30:00-03:00", true)]
   [InlineData(ParameterType.DateTime, "2023-12-01T15:30:00.000-05:00", true)]
   [InlineData(ParameterType.DateTime, "invalid-date", false)]
   [InlineData(ParameterType.Date, "2026-04-08", true)]
   [InlineData(ParameterType.Date, "22/02/1978", true)]
   [InlineData(ParameterType.Date, "invalid-date", false)]
   [InlineData(ParameterType.DateTime, "12:30 PM", true)]
   [InlineData(ParameterType.Time, "15:30:00", true)]
   [InlineData(ParameterType.Time, "invalid-time", false)]
   [InlineData(ParameterType.String, "Any string value", true)]
   public void ValidateValueFormat_WhenReceivingVariousTypes_ShouldReturnExpectedResult(
       ParameterType type,
       string value,
       bool expectedSuccess)
   {
      var request = new ParameterCreateRequest("Sys", "Group", "Name", "TestParam", "Test description", type, value, ParameterOverrideType.None, true);

      var result = _validator.ValidateCreate(request, keyExists: false);

      result.IsSuccess.Should().Be(expectedSuccess);

      if (!expectedSuccess)
      {
         result.Messages.Should().Contain(m => m is ParameterInvalidValueFormatError);
      }
   }
}