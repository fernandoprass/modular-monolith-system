using Courier.Application.Validators;
using Courier.Domain.DTOs.Requests;
using FluentAssertions;

namespace Courier.Application.Tests.Validators;

public class NotificationValidatorTests
{
   private readonly NotificationValidator _validator = new();

   [Theory]
   [InlineData(1, 25, true)]
   [InlineData(0, 25, false)]
   [InlineData(1, 0, false)]
   public void ValidateSearch_ShouldValidatePaging(int pageNumber, int pageSize, bool expectedSuccess)
   {
      var request = CreateRequest(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow, pageNumber, pageSize);

      var result = _validator.ValidateSearch(request);

      result.HasError.Should().Be(!expectedSuccess);
   }

   [Fact]
   public void ValidateSearch_ShouldFail_WhenDateFromIsAfterDateTo()
   {
      var request = CreateRequest(DateTime.UtcNow, DateTime.UtcNow.AddDays(-1));

      var result = _validator.ValidateSearch(request);

      result.HasError.Should().BeTrue();
   }

   private static NotificationSearchRequest CreateRequest(
      DateTime dateFrom,
      DateTime dateTo,
      int pageNumber = 1,
      int pageSize = 25)
   {
      return new NotificationSearchRequest(
         null,
         null,
         null,
         null,
         null,
         dateFrom,
         dateTo,
         pageNumber,
         pageSize);
   }
}
