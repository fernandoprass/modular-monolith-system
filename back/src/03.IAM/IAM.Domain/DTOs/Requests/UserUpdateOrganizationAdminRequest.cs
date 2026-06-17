namespace IAM.Domain.DTOs.Requests;

public sealed record UserUpdateOrganizationAdminRequest(
   bool IsOrganizationAdmin);
