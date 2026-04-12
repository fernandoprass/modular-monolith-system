namespace Shared.Domain.Entities
{
   /// <summary>
   /// Entity is an abstract base class that represents a generic entity in the domain model. It defines a single property, Id, 
   /// which is of a generic type TId. This allows for flexibility in defining the type of the identifier for different entities
   /// </summary>
   /// <typeparam name="TId"></typeparam>
   public abstract class Entity<TId>
   {
      public TId Id { get; set; }
   }

   /// <summary>
   /// The Entity class is a specialization of the generic Entity class for entities that use Guid as their identifier type.
   /// </summary>
   public abstract class Entity : Entity<Guid> { }
}
