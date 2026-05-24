using Myce.Response;

namespace Courier.Application.Contracts;

public interface IEmailTemplateRenderer
{
   Result<string> Render(
      string template,
      IReadOnlyDictionary<string, string> values,
      bool htmlEncodeValues = false);
}
