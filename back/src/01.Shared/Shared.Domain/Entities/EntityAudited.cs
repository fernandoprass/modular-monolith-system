namespace Shared.Domain.Entities
{
   /// <summary>
   /// EntityAudited is an abstract base class that extends the Entity class to include auditing 
   /// properties such as CreatedAt, UpdatedAt, CreatedBy, and UpdatedBy. which are typically used 
   /// for tracking the lifecycle of entities. The class is designed to be inherited by specific 
   /// entity classes that will define their own properties and behaviors while 
   /// still benefiting from the common functionality provided by this base class.
   /// </summary>
   /// <typeparam name="T">The Type of the Entity</typeparam>
   /// </summary>
   /// <typeparam name="TId">The type of the entity's identifier.</typeparam>
   public abstract class EntityAudited<TId> : Entity<TId>
   {
      public DateTime CreatedAt { get; set; }
      public DateTime? UpdatedAt { get; set; }
      public Guid CreatedBy { get; set; }
      public Guid? UpdatedBy { get; set; }
   }

   /// <summary>
   /// The EntityAudited class is a specialization of the generic EntityAudited class for 
   /// entities that use Guid as their identifier type.
   /// </summary>
   public abstract class EntityAudited : EntityAudited<Guid> { }
}
