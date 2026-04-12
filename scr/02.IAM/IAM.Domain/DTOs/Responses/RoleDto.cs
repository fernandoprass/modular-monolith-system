namespace IAM.Domain.DTOs.Responses;

public record RoleDto(
   Guid Id, 
   string Name, 
   Guid? CustomerId, 
   bool IsDefault, 
   IEnumerable<PermissionDto> Features);
