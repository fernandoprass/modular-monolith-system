using Courier.Domain;
using Shared.Domain.Enums;
using Shared.Infrastructure;

namespace DatabaseSeeder.Parameters;

public class SeederParametersCourier(SharedDbContext dbContext) : SeederParametersBase(dbContext)
{
   public async Task SeedAsync(CancellationToken cancellationToken = default)
   {
      await AddParameterAsync(
         CourierParam.EmailDelivery.MaxRetries,
         "Maximum Email Delivery Retries",
         "The maximum number of email delivery attempts before an email is marked as failed.",
         ParameterType.Integer,
         value: "3",
         ParameterOverrideType.None,
         isVisible: true,
         cancellationToken);
   }
}
