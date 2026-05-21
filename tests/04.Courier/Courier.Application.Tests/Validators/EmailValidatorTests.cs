using Courier.Application.Validators;
using Courier.Domain.DTOs.Requests;
using Courier.Domain.Messages;
using FluentAssertions;

namespace Courier.Application.Tests.Validators;

public class EmailValidatorTests
{
   private readonly EmailValidator _validator = new();

   [Theory]
   [InlineData("person@example.com", true)]
   [InlineData("", false)]
   [InlineData("not-email", false)]
   public void ValidateCreate_ShouldValidateRecipient(string recipient, bool expectedSuccess)
   {
      var request = new EmailCreateRequest(
         Guid.NewGuid(),
         Guid.NewGuid(),
         "IAM",
         "Users",
         "welcome",
         recipient,
         "Subject",
         "Body",
         false);

      var result = _validator.ValidateCreate(request);

      result.HasError.Should().Be(!expectedSuccess);
   }

   [Theory]
   [InlineData(1, 25, true)]
   [InlineData(0, 25, false)]
   [InlineData(1, 0, false)]
   public void ValidateSearch_ShouldValidatePaging(int pageNumber, int pageSize, bool expectedSuccess)
   {
      var request = new EmailSearchRequest(null, null, null, null, null, null, null, null, pageNumber, pageSize);

      var result = _validator.ValidateSearch(request);

      result.HasError.Should().Be(!expectedSuccess);
   }

   [Fact]
   public void ValidateSearch_ShouldReturnDateRangeError_WhenDateFromIsAfterDateTo()
   {
      var request = new EmailSearchRequest(
         null,
         null,
         null,
         null,
         null,
         null,
         DateTime.UtcNow,
         DateTime.UtcNow.AddDays(-1));

      var result = _validator.ValidateSearch(request);

      result.Messages.Should().Contain(m => m is EmailInvalidDateRangeError);
   }
}
