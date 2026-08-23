using FluentAssertions;
using Shared.Application.Security;

namespace Shared.Application.Tests.Security;

public class HtmlSanitizerServiceTests
{
   private readonly HtmlSanitizerService _sanitizer = new();

   [Fact]
   public void Sanitize_ShouldRemoveDangerousElementsAndEventAttributes()
   {
      const string html = """<p onclick="evil()">Hi</p><script>alert(1)</script>""";

      var result = _sanitizer.Sanitize(html);

      result.Should().Contain("<p>Hi</p>");
      result.Should().NotContain("onclick");
      result.Should().NotContain("script");
   }

   [Theory]
   [InlineData("""<a href="javascript:alert(1)">Bad</a>""")]
   [InlineData("""<img src="//evil.test/a.png">""")]
   [InlineData("""<img src="data:image/svg+xml;base64,abc">""")]
   public void Sanitize_ShouldRemoveUnsafeUrls(string html)
   {
      var result = _sanitizer.Sanitize(html);

      result.Should().NotContain("javascript:");
      result.Should().NotContain("//evil.test");
      result.Should().NotContain("data:image");
   }

   [Theory]
   [InlineData("https://example.com/a.png")]
   [InlineData("http://example.com/a.png")]
   [InlineData("mailto:test@example.com")]
   [InlineData("tel:+353123456")]
   [InlineData("cid:image001.png")]
   [InlineData("/profile")]
   [InlineData("#top")]
   public void IsSafeUrl_ShouldAcceptAllowedUrls(string url)
   {
      _sanitizer.IsSafeUrl(url).Should().BeTrue();
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [InlineData("javascript:alert(1)")]
   [InlineData("//evil.test/a.png")]
   [InlineData("data:image/png;base64,abc")]
   public void IsSafeUrl_ShouldRejectUnsafeUrls(string url)
   {
      _sanitizer.IsSafeUrl(url).Should().BeFalse();
   }
}
