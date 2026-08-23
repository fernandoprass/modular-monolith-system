namespace Shared.Application.Contracts;

public interface IHtmlSanitizer
{
   string Sanitize(string html);
   bool IsSafeUrl(string url);
}
