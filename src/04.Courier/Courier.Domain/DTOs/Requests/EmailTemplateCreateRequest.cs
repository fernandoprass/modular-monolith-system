using Courier.Domain.Enums;

namespace Courier.Domain.DTOs.Requests;

public record EmailTemplateCreateRequest(
   string Key,
   EmailRetentionPolicy RetentionPolicy);
