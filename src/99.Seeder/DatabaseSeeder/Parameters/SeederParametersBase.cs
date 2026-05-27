using Microsoft.EntityFrameworkCore;
using Shared.Domain;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shared.Infrastructure;

namespace DatabaseSeeder.Parameters;

public abstract class SeederParametersBase(SharedDbContext dbContext)
{
   protected async Task AddParameterAsync(
      string key,
      string title,
      string description,
      ParameterType type,
      string value,
      ParameterOverrideType overrideType,
      bool isVisible,
      CancellationToken cancellationToken)
   {
      var exists = await dbContext.Parameters.AnyAsync(parameter => parameter.Key == key, cancellationToken);

      if (exists) return;

      Console.WriteLine($"Parameter: {key}");

      var parameterKey = new ParameterKey(key);
      var parameter = Parameter.Create(
         parameterKey.Module,
         parameterKey.Group,
         parameterKey.Name,
         title,
         description,
         type,
         value,
         validationRegex: null,
         validationErrorCustomMessage: null,
         listItems: null,
         externalListEndpoint: null,
         overrideType,
         isVisible);

      await dbContext.Parameters.AddAsync(parameter, cancellationToken);
   }
}
