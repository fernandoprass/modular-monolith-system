namespace IAM.Domain.DTOs.Requests;

public sealed record UserUpdatePasswordRequest(
   string PasswordOld,
   string PasswordNew
);