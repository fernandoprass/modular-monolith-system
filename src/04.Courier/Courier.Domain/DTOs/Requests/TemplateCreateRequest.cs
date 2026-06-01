using Courier.Domain.Enums;
using Shared.Domain.Enums;

namespace Courier.Domain.DTOs.Requests;

public record TemplateCreateRequest(
   string Key,
   string Name,
   TemplateType Type,
   RetentionPolicy RetentionPolicy);
