namespace IAM.Domain.DTOs.Requests;

public record RoleCreateRequest(
   string Name, 
   string Description, 
   bool IsDefault, 
   bool IsActive, 
   Guid? CustomerId);
