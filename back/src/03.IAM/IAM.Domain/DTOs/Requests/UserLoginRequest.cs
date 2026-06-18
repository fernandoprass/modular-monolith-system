namespace IAM.Domain.DTOs.Requests;

public sealed record UserLoginRequest
(
    string Email,
     string Password
);