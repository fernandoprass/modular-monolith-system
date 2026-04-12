namespace Shared.Domain.Entities;

public class ParameterOverride : EntityAudited
{
   public string Value { get; set; }
   public Guid OwnerId { get; set; }
   public Guid ParameterId { get; set; }

   public static ParameterOverride Create(Guid parameterId, Guid ownerId, string value)
   {
      return new ParameterOverride
      {
         Id = Guid.CreateVersion7(),
         ParameterId = parameterId,
         OwnerId = ownerId,
         Value = value
      };
   }

   public void Update(string value)
   {
         Value = value;
   }
}
