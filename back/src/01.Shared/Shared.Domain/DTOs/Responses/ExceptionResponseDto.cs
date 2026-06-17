namespace Shared.Domain.DTOs.Responses;

public class ExceptionResponseDto
{
   public int StatusCode { get; set; }
   public string Message { get; set; } = string.Empty;
   public string Details { get; set; } = string.Empty;
}
