using Courier.Domain.Enums;

namespace Courier.Domain.DTOs.Requests;

public record EmailTemplateUpdateRequest(
   string Key,
   EmailRetentionPolicy RetentionPolicy);
