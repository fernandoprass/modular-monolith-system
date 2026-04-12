using FluentAssertions;
using Myce.Response;
using NSubstitute;
using Shared.Application.Contracts;
using Shared.Application.Services;
using Shared.Domain.DTOs.Requests;
using Shared.Domain.DTOs.Responses;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shared.Domain.Interfaces;
using Shared.Domain.Messages;

namespace Shared.Application.Tests.Services;

public class ParameterServiceTests
{
   private readonly ISharedUnitOfWork _unitOfWorkMock;
   private readonly IUserContext _userContextMock;
   private readonly IParameterValidator _parameterValidatorMock;
   private readonly IParameterRepository _parameterRepositoryMock;
   private readonly IParameterOverrideRepository _parameterOverrideRepositoryMock;
   private readonly IParameterQueryRepository _parameterQueryRepositoryMock;
   private readonly ParameterService _parameterService;

   private readonly string _keyMock = "Module.Group.Key";

   public ParameterServiceTests()
   {
      _unitOfWorkMock = Substitute.For<ISharedUnitOfWork>();
      _userContextMock = Substitute.For<IUserContext>();
      _parameterValidatorMock = Substitute.For<IParameterValidator>();
      _parameterRepositoryMock = Substitute.For<IParameterRepository>();
      _parameterOverrideRepositoryMock = Substitute.For<IParameterOverrideRepository>();
      _parameterQueryRepositoryMock = Substitute.For<IParameterQueryRepository>();

      _userContextMock.UserOwnerId.Returns(Guid.NewGuid());
      _userContextMock.UserId.Returns(Guid.NewGuid());

      _parameterService = new ParameterService(
          _unitOfWorkMock,
          _userContextMock,
          _parameterValidatorMock,
          _parameterRepositoryMock,
          _parameterOverrideRepositoryMock,
          _parameterQueryRepositoryMock);
   }

   [Fact]
   public async Task GetByIdAsync_ShouldReturnNotFoundError_WhenParameterDoesNotExist()
   {
      var parameterId = Guid.NewGuid();
      _parameterQueryRepositoryMock.GetByIdAsync(parameterId, Arg.Any<CancellationToken>()).Returns((ParameterDto)null);

      var result = await _parameterService.GetByIdAsync(parameterId, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().ContainSingle(m => m is NotFoundError);
   }

   [Fact]
   public async Task CreateAsync_ShouldReturnValidationErrors_WhenValidatorFails()
   {
      var request = new ParameterCreateRequest("Module", "Group", "Name", "Title", "Desc", ParameterType.String, "Value", ParameterOverrideType.None, true);
      _parameterQueryRepositoryMock.GetByModuleGroupAndKeyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new ParameterDto());
      _parameterValidatorMock.ValidateCreate(request, true).Returns(Result.Failure(new ParameterDuplicatedError("M", "G", "K")));

      var result = await _parameterService.CreateAsync(request, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().Contain(m => m is ParameterDuplicatedError);
      await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task CreateAsync_ShouldSaveParameter_WhenRequestIsValid()
   {
      var request = new ParameterCreateRequest("Module", "Group", "Name", "Title", "Desc", ParameterType.String, "Value", ParameterOverrideType.None, true);
      _parameterQueryRepositoryMock.GetByModuleGroupAndKeyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((ParameterDto)null);
      _parameterValidatorMock.ValidateCreate(request, false).Returns(Result.Success());

      var result = await _parameterService.CreateAsync(request, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeTrue();
      await _unitOfWorkMock.Parameters.Received(1).AddAsync(Arg.Any<Parameter>(), Arg.Any<CancellationToken>());
      await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task SaveOverrideValueAsync_ShouldCreateNewOverride_WhenOverrideDoesNotExist()
   {
      var parameterId = Guid.NewGuid();
      var request = new ParameterOwnerUpdateRequest("NewValue");
      var parameter = new Parameter { OverrideType = ParameterOverrideType.UserOwnerId };

      _parameterRepositoryMock.GetByIdAsync(parameterId, Arg.Any<CancellationToken>()).Returns(parameter);
      _parameterValidatorMock.ValidateOwnerUpdate(parameter, request).Returns(Result.Success());
      _parameterOverrideRepositoryMock.GetByParameterIdAndOwnerIdAsync(parameterId, _userContextMock.UserOwnerId, Arg.Any<CancellationToken>()).Returns((ParameterOverride)null);

      var result = await _parameterService.SaveOverrideValueAsync(parameterId, request, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeTrue();
      await _unitOfWorkMock.ParameterOverrides.Received(1).AddAsync(Arg.Any<ParameterOverride>(), Arg.Any<CancellationToken>());
      await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task DeleteAsync_ShouldReturnNotFoundError_WhenParameterDoesNotExist()
   {
      var parameterId = Guid.NewGuid();
      _parameterRepositoryMock.GetByIdAsync(parameterId, Arg.Any<CancellationToken>()).Returns((Parameter)null);

      var result = await _parameterService.DeleteAsync(parameterId, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().ContainSingle(m => m is NotFoundError);
      await _unitOfWorkMock.Parameters.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
   }

   [Theory]
   [InlineData("0", 0)]
   [InlineData("123", 123)]
   [InlineData("-456", -456)]
   [InlineData("32767", 32767)]   // Int16.MaxValue
   [InlineData("-32768", -32768)] // Int16.MinValue
   public async Task GetShortIntAsync_ShouldReturnParsedValue_WhenValueIsValid(string value, short expectedValue)
   {
      var valueDto = new ParameterValueDto { Value = value };

      _parameterQueryRepositoryMock.GetValueAsync(_keyMock, _userContextMock.UserOwnerId, _userContextMock.UserId, Arg.Any<CancellationToken>()).Returns(valueDto);

      var result = await _parameterService.GetShortIntAsync(_keyMock, TestContext.Current.CancellationToken);

      result.Should().Be(expectedValue);
   }

   [Theory]
   [InlineData("not-a-short")]      // Alphanumeric string
   [InlineData("12.5")]             // Decimal value
   [InlineData("32768")]            // Above Int16 limit (Max + 1)
   [InlineData("-32769")]           // Below Int16 limit (Min - 1)
   [InlineData("2147483647")]       // Valid Int32 but invalid Int16 (Overflow)
   [InlineData("")]                 // Empty string
   [InlineData(" ")]                // White space
   public async Task GetShortIntAsync_ShouldThrowInvalidDataException_WhenValueIsInvalidForInt16(string invalidValue)
   {
      var valueDto = new ParameterValueDto { Value = invalidValue };

      _parameterQueryRepositoryMock.GetValueAsync(_keyMock, _userContextMock.UserOwnerId, _userContextMock.UserId, Arg.Any<CancellationToken>()).Returns(valueDto);

      Func<Task> act = async () => await _parameterService.GetShortIntAsync(_keyMock, TestContext.Current.CancellationToken);

      await act.Should().ThrowAsync<InvalidDataException>()
         .WithMessage($"*'{invalidValue}'*invalid*'{_keyMock}'*type Int16*");
   }

   [Theory]
   [InlineData("0", 0)]
   [InlineData("123", 123)]
   [InlineData("-456", -456)]
   [InlineData("2147483647", 2147483647)]   // Int32.MaxValue
   [InlineData("-2147483648", -2147483648)] // Int32.MinValue
   public async Task GetIntAsync_ShouldReturnParsedValue_WhenValueIsValid(string value, int expectedValue)
   {
      var valueDto = new ParameterValueDto { Value = value };

      _parameterQueryRepositoryMock.GetValueAsync(_keyMock, _userContextMock.UserOwnerId, _userContextMock.UserId, Arg.Any<CancellationToken>())
         .Returns(valueDto);

      var result = await _parameterService.GetIntAsync(_keyMock, TestContext.Current.CancellationToken);

      result.Should().Be(expectedValue);
   }

   [Theory]
   [InlineData("not-an-int")]
   [InlineData("12.5")]
   [InlineData("12,5")]
   [InlineData("2147483648")]    // Int32.MaxValue + 1
   [InlineData("-2147483649")]   // Int32.MinValue - 1
   [InlineData("")]
   [InlineData(" ")]
   public async Task GetIntAsync_ShouldThrowInvalidDataException_WhenValueIsInvalidForInt32(string invalidValue)
   {
      var valueDto = new ParameterValueDto { Value = invalidValue };

      _parameterQueryRepositoryMock.GetValueAsync(_keyMock, _userContextMock.UserOwnerId, _userContextMock.UserId, Arg.Any<CancellationToken>()).Returns(valueDto);

      Func<Task> act = async () => await _parameterService.GetIntAsync(_keyMock, TestContext.Current.CancellationToken);

      await act.Should().ThrowAsync<InvalidDataException>()
         .WithMessage($"*'{invalidValue}'*invalid*'{_keyMock}'*type Int32*");
   }

   [Theory]
   [InlineData("0", 0)]
   [InlineData("123", 123)]
   [InlineData("-456", -456)]
   [InlineData("9223372036854775807", 9223372036854775807)]   // Int64.MaxValue
   [InlineData("-9223372036854775808", -9223372036854775808)] // Int64.MinValue
   public async Task GetLongIntAsync_ShouldReturnParsedValue_WhenValueIsValid(string value, long expectedValue)
   {
      var valueDto = new ParameterValueDto { Value = value };

      _parameterQueryRepositoryMock.GetValueAsync(_keyMock, _userContextMock.UserOwnerId, _userContextMock.UserId, Arg.Any<CancellationToken>()).Returns(valueDto);

      var result = await _parameterService.GetLongIntAsync(_keyMock, TestContext.Current.CancellationToken);

      result.Should().Be(expectedValue);
   }

   [Theory]
   [InlineData("not-a-long")]                // Alphanumeric string
   [InlineData("12.5")]                      // Decimal value
   [InlineData("9223372036854775808")]       // Above Int64 limit (Max + 1)
   [InlineData("-9223372036854775809")]      // Below Int64 limit (Min - 1)
   [InlineData("")]                          // Empty string
   [InlineData(" ")]                         // White space
   public async Task GetLongIntAsync_ShouldThrowInvalidDataException_WhenValueIsInvalidForInt64(string invalidValue)
   {
      var valueDto = new ParameterValueDto { Value = invalidValue };

      _parameterQueryRepositoryMock.GetValueAsync(_keyMock, _userContextMock.UserOwnerId, _userContextMock.UserId, Arg.Any<CancellationToken>()).Returns(valueDto);

      Func<Task> act = async () => await _parameterService.GetLongIntAsync(_keyMock, TestContext.Current.CancellationToken);

      await act.Should().ThrowAsync<InvalidDataException>()
         .WithMessage($"*'{invalidValue}'*invalid*'{_keyMock}'*type Int64*");
   }

   [Theory]
   [InlineData("0", 0.0)]
   [InlineData("123", 123.0)]
   [InlineData("1234.0009", 1234.0009)]
   [InlineData("-456.78", -456.78)]
   [InlineData("1.7976931348623157E+308", double.MaxValue)] // double.MaxValue
   [InlineData("-1.7976931348623157E+308", double.MinValue)] // double.MinValue
   [InlineData("4.94065645841247E-324", 4.94065645841247E-324)] // double.Epsilon
   public async Task GetDoubleAsync_ShouldReturnParsedValue_WhenValueIsValid(string value, double expectedValue)
   {
      var valueDto = new ParameterValueDto { Value = value };

      _parameterQueryRepositoryMock.GetValueAsync(_keyMock, _userContextMock.UserOwnerId, _userContextMock.UserId, Arg.Any<CancellationToken>())
         .Returns(valueDto);

      var result = await _parameterService.GetDoubleAsync(_keyMock, TestContext.Current.CancellationToken);

      result.Should().Be(expectedValue);
   }

   [Theory]
   [InlineData("not-a-double")]          // Alphanumeric string
   [InlineData("")]                      // Empty string
   [InlineData(" ")]                     // White space
   public async Task GetDoubleAsync_ShouldThrowInvalidDataException_WhenValueIsInvalidForDouble(string invalidValue)
   {
      var valueDto = new ParameterValueDto { Value = invalidValue };

      _parameterQueryRepositoryMock.GetValueAsync(_keyMock, _userContextMock.UserOwnerId, _userContextMock.UserId, Arg.Any<CancellationToken>()).Returns(valueDto);

      Func<Task> act = async () => await _parameterService.GetDoubleAsync(_keyMock, TestContext.Current.CancellationToken);

      await act.Should().ThrowAsync<InvalidDataException>()
         .WithMessage($"*'{invalidValue}'*invalid*'{_keyMock}'*type Double*");
   }

   [Theory]
   [InlineData("0", 0.0)]
   [InlineData("123", 123.0)]
   [InlineData("15.50", 15.50)]
   [InlineData("-456.78", -456.78)]
   public async Task GetDecimalAsync_ShouldReturnParsedValue_WhenValueIsValid(string value, decimal expectedValue)
   {
      var valueDto = new ParameterValueDto { Value = value };

      _parameterQueryRepositoryMock.GetValueAsync(_keyMock, _userContextMock.UserOwnerId, _userContextMock.UserId, Arg.Any<CancellationToken>())
         .Returns(valueDto);

      var result = await _parameterService.GetDecimalAsync(_keyMock, TestContext.Current.CancellationToken);

      result.Should().Be(expectedValue);
   }

   [Theory]
   [InlineData("not-a-decimal")]       // Alphanumeric string
   [InlineData("79228162514264337593543950336")] // Above decimal limit (Overflow)
   [InlineData("")]                   // Empty string
   [InlineData(" ")]                  // White space
   public async Task GetDecimalAsync_ShouldThrowInvalidDataException_WhenValueIsInvalidForDecimal(string invalidValue)
   {
      var valueDto = new ParameterValueDto { Value = invalidValue };

      _parameterQueryRepositoryMock.GetValueAsync(_keyMock, _userContextMock.UserOwnerId, _userContextMock.UserId, Arg.Any<CancellationToken>()).Returns(valueDto);

      Func<Task> act = async () => await _parameterService.GetDecimalAsync(_keyMock, TestContext.Current.CancellationToken);

      await act.Should().ThrowAsync<InvalidDataException>()
         .WithMessage($"*'{invalidValue}'*invalid*'{_keyMock}'*type Decimal*");
   }

   [Theory]
   [InlineData("true", true)]
   [InlineData("false", false)]
   public async Task GetBoolAsync_ShouldReturnParsedValue_WhenKeyExists(string value, bool expected)
   {
      var valueDto = new ParameterValueDto { Value = value };

      _parameterQueryRepositoryMock.GetValueAsync(_keyMock, _userContextMock.UserOwnerId, _userContextMock.UserId, Arg.Any<CancellationToken>()).Returns(valueDto);

      var result = await _parameterService.GetBoolAsync(_keyMock, TestContext.Current.CancellationToken);

      result.Should().Be(expected);
   }

   [Theory]
   [InlineData("2023-12-01")]
   [InlineData("2023-12-01T15:30:00")]
   [InlineData("2023-12-01T15:30:00Z")]
   [InlineData("2023-12-01T15:30:00-03:00")]
   [InlineData("12/31/2023 23:59:59")] // Standard US format often accepted by InvariantCulture
   public async Task GetDateTimeAsync_ShouldReturnParsedValue_WhenValueIsValid(string value)
   {
      var valueDto = new ParameterValueDto { Value = value };

      _parameterQueryRepositoryMock.GetValueAsync(_keyMock, _userContextMock.UserOwnerId, _userContextMock.UserId, Arg.Any<CancellationToken>())
         .Returns(valueDto);

      var result = await _parameterService.GetDateTimeAsync(_keyMock, TestContext.Current.CancellationToken);

      result.Should().Be(DateTime.Parse(value, System.Globalization.CultureInfo.InvariantCulture));
   }

   [Theory]
   [InlineData("not-a-date")]          // Alphanumeric string
   [InlineData("2023-13-01")]          // Invalid month
   [InlineData("2023-12-32")]          // Invalid day
   [InlineData("2023-12-01 25:00:00")] // Invalid hour
   [InlineData("")]                    // Empty string
   [InlineData(" ")]                   // White space
   public async Task GetDateTimeAsync_ShouldThrowInvalidDataException_WhenValueIsInvalidForDateTime(string invalidValue)
   {
      var valueDto = new ParameterValueDto { Value = invalidValue };

      _parameterQueryRepositoryMock.GetValueAsync(_keyMock, _userContextMock.UserOwnerId, _userContextMock.UserId, Arg.Any<CancellationToken>()).Returns(valueDto);

      Func<Task> act = async () => await _parameterService.GetDateTimeAsync(_keyMock, TestContext.Current.CancellationToken);

      await act.Should().ThrowAsync<InvalidDataException>()
         .WithMessage($"*'{invalidValue}'*invalid*'{_keyMock}'*type DateTime*");
   }

   [Fact]
   public async Task GetStringAsync_ShouldReturnEmptyString_WhenValueIsNull()
   {
      var valueDto = new ParameterValueDto { Value = null };
      _parameterQueryRepositoryMock.GetValueAsync(_keyMock, _userContextMock.UserOwnerId, _userContextMock.UserId, Arg.Any<CancellationToken>()).Returns(valueDto);

      var result = await _parameterService.GetStringAsync(_keyMock, TestContext.Current.CancellationToken);

      result.Should().BeEmpty();
   }
}
