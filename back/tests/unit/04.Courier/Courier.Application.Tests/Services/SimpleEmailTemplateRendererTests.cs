using Courier.Application.Services;
using Courier.Domain.Messages;
using FluentAssertions;

namespace Courier.Application.Tests.Services;

public class SimpleEmailTemplateRendererTests
{
   private readonly SimpleEmailTemplateRenderer _renderer = new();

   [Fact]
   public void Render_ShouldReplacePlaceholders()
   {
      var result = _renderer.Render(
         "Hello {{ user.name }} from {{organization.name}}.",
         new Dictionary<string, string>
         {
            ["user.name"] = "Ana",
            ["organization.name"] = "Acme"
         });

      result.HasError.Should().BeFalse();
      result.Data.Should().Be("Hello Ana from Acme.");
   }

   [Fact]
   public void Render_ShouldHtmlEncodeValues_WhenRequested()
   {
      var result = _renderer.Render(
         "<p>Hello {{user.name}}</p>",
         new Dictionary<string, string>
         {
            ["user.name"] = "<b>Ana</b>"
         },
         htmlEncodeValues: true);

      result.HasError.Should().BeFalse();
      result.Data.Should().Be("<p>Hello &lt;b&gt;Ana&lt;/b&gt;</p>");
   }

   [Fact]
   public void Render_ShouldReturnMissingPlaceholderError()
   {
      var result = _renderer.Render("Hello {{user.name}}.", new Dictionary<string, string>());

      result.HasError.Should().BeTrue();
      result.Messages.Should().ContainSingle(message => message is EmailTemplatePlaceholderMissingError);
   }
}
