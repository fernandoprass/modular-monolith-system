using Courier.Domain.Enums;

namespace Courier.Domain.DTOs.Requests;

public record TemplateUpdateRequest(
   string Key,
   string Name,
   TemplateType Type,
   RetentionPolicy RetentionPolicy);
