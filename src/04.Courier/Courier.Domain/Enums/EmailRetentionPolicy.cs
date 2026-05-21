namespace Courier.Domain.Enums
{
   public enum EmailRetentionPolicy : byte
   {
      Transient = 1,     
      Operational = 2,   
      Standard = 3,      
      Extended = 4,      
      Compliance = 5,    
   }
}
