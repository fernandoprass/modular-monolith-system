using Courier.Application.Contracts;
using Courier.Domain;
using Courier.Domain.DTOs.Requests;
using Courier.Domain.DTOs.Responses;
using Courier.Domain.Entities;
using Courier.Domain.Interfaces.Repositories;
using Courier.Domain.Mappers;
using Courier.Domain.Messages;
using Courier.Domain.ValueObjects;
using Myce.Response;
using Shared.Application.Contracts;
using Shared.Domain.DTOs.Responses;
using Shared.Domain.Messages;
using System.Text.RegularExpressions;

namespace Courier.Application.Services;

public partial class TemplateService(
   ITemplateWriteRepository templateRepository,
   ITemplateValidator templateValidator,
   IUserContext userContext) : ITemplateService
{
   private readonly ITemplateWriteRepository _templateRepository = templateRepository;
   private readonly ITemplateValidator _templateValidator = templateValidator;
   private readonly IUserContext _userContext = userContext;

   public async Task<Result<PagedResultDto<TemplateLiteDto>>> GetAsync(
      TemplateSearchRequest request,
      CancellationToken cancellationToken = default)
   {
      var validation = _templateValidator.ValidateSearch(request);

      if (validation.HasError)
      {
         return Result<PagedResultDto<TemplateLiteDto>>.Failure(validation.Messages);
      }

      var templates = await _templateRepository.GetAsync(request, cancellationToken);
      return Result<PagedResultDto<TemplateLiteDto>>.Success(templates);
   }

   public async Task<Result<TemplateDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      var template = await _templateRepository.GetByIdAsync(id, cancellationToken);

      if (template == null)
      {
         return Result<TemplateDto>.Failure(new NotFoundError(CourierConst.Entity.Template));
      }

      return Result<TemplateDto>.Success(template.ToTemplateDto());
   }

   public async Task<Result<TemplateDto>> CreateAsync(
      TemplateCreateRequest request,
      CancellationToken cancellationToken = default)
   {
      var keyExists = await _templateRepository.KeyExistsAsync(
         request.Module,
         request.Key,
         cancellationToken: cancellationToken);
      var validation = _templateValidator.ValidateCreate(request, keyExists);

      if (validation.HasError)
      {
         return Result<TemplateDto>.Failure(validation.Messages);
      }

      var template = Template.Create(
         request.Module,
         request.Key,
         request.IsAllowingOptOut,
         request.Severity,
         request.RetentionPolicy,
         _userContext.UserId);

      await _templateRepository.AddAsync(template, cancellationToken);

      return Result<TemplateDto>.Success(template.ToTemplateDto());
   }

   public async Task<Result> UpdateAsync(
      Guid id,
      TemplateUpdateRequest request,
      CancellationToken cancellationToken = default)
   {
      var template = await _templateRepository.GetByIdAsync(id, cancellationToken);
      var keyExists = await _templateRepository.KeyExistsAsync(
         request.Module,
         request.Key,
         id,
         cancellationToken);
      var validation = _templateValidator.ValidateUpdate(request, template != null, keyExists);

      if (validation.HasError)
      {
         return Result.Failure(validation.Messages);
      }

      template!.Update(
         request.Module,
         request.Key,
         request.IsAllowingOptOut,
         request.Severity,
         request.RetentionPolicy,
         _userContext.UserId);
      await _templateRepository.UpdateAsync(template, cancellationToken);

      return Result.Success(new SuccessInfo());
   }

   public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
   {
      var template = await _templateRepository.GetByIdAsync(id, cancellationToken);

      if (template == null)
      {
         return Result.Failure(new NotFoundError(CourierConst.Entity.Template));
      }

      await _templateRepository.DeleteAsync(id, cancellationToken);
      return Result.Success(new SuccessInfo());
   }

   public async Task<Result> AddTranslationAsync(
      Guid id,
      TemplateTranslationRequest request,
      CancellationToken cancellationToken = default)
   {
      var template = await _templateRepository.GetByIdAsync(id, cancellationToken);
      var validation = _templateValidator.ValidateTranslation(request, template != null);

      if (validation.HasError)
      {
         return Result.Failure(validation.Messages);
      }

      var translation = ToTranslation(request);

      if (!template!.AddTranslation(translation, _userContext.UserId))
      {
         return Result.Failure(new TemplateTranslationAlreadyExistsError(request.Language));
      }

      await _templateRepository.UpdateAsync(template, cancellationToken);
      return Result.Success(new SuccessInfo());
   }

   public async Task<Result> UpdateTranslationAsync(
      Guid id,
      string language,
      TemplateTranslationRequest request,
      CancellationToken cancellationToken = default)
   {
      var normalizedRequest = request with { Language = language };
      var template = await _templateRepository.GetByIdAsync(id, cancellationToken);
      var validation = _templateValidator.ValidateTranslation(normalizedRequest, template != null);

      if (validation.HasError)
      {
         return Result.Failure(validation.Messages);
      }

      var translation = ToTranslation(normalizedRequest);

      if (!template!.UpdateTranslation(language, translation, _userContext.UserId))
      {
         return Result.Failure(new TemplateTranslationNotFoundError(language));
      }

      await _templateRepository.UpdateAsync(template, cancellationToken);
      return Result.Success(new SuccessInfo());
   }

   public async Task<Result> RemoveTranslationAsync(
      Guid id,
      string language,
      CancellationToken cancellationToken = default)
   {
      var template = await _templateRepository.GetByIdAsync(id, cancellationToken);

      if (template == null)
      {
         return Result.Failure(new NotFoundError(CourierConst.Entity.Template));
      }

      if (!template.RemoveTranslation(language, _userContext.UserId))
      {
         return Result.Failure(new TemplateTranslationNotFoundError(language));
      }

      await _templateRepository.UpdateAsync(template, cancellationToken);
      return Result.Success(new SuccessInfo());
   }

   private static TemplateTranslation ToTranslation(TemplateTranslationRequest request)
   {
      var email = request.Email == null
         ? null
         : TemplateTranslationEmail.Create(
            request.Email.Subject,
            SanitizeTemplateBody(request.Email.Body));
      var notification = request.Notification == null
         ? null
         : TemplateTranslationNotification.Create(
            request.Notification.Title,
            request.Notification.Message,
            request.Notification.ActionLink);

      return TemplateTranslation.Create(request.Language, request.Name, email, notification);
   }

   private static string SanitizeTemplateBody(string body)
   {
      var sanitized = DangerousElementRegex().Replace(body, string.Empty);
      sanitized = EventAttributeRegex().Replace(sanitized, string.Empty);
      sanitized = UnsafeUrlAttributeRegex().Replace(sanitized, match =>
      {
         var attribute = match.Groups["attribute"].Value;
         var quote = match.Groups["quote"].Value;
         var url = match.Groups["url"].Value.Trim();

         return IsSafeUrl(url)
            ? $"{attribute}={quote}{url}{quote}"
            : string.Empty;
      });

      return sanitized;
   }

   private static bool IsSafeUrl(string url)
   {
      if (string.IsNullOrWhiteSpace(url))
      {
         return false;
      }

      var normalizedUrl = url.Trim().ToLowerInvariant();

      return normalizedUrl.StartsWith("http://", StringComparison.Ordinal)
         || normalizedUrl.StartsWith("https://", StringComparison.Ordinal)
         || normalizedUrl.StartsWith("mailto:", StringComparison.Ordinal)
         || normalizedUrl.StartsWith("tel:", StringComparison.Ordinal)
         || normalizedUrl.StartsWith("cid:", StringComparison.Ordinal)
         || normalizedUrl.StartsWith("/", StringComparison.Ordinal)
         || normalizedUrl.StartsWith("#", StringComparison.Ordinal)
         || normalizedUrl.StartsWith("data:image/", StringComparison.Ordinal);
   }

   [GeneratedRegex(@"<\s*(script|style|iframe|object|embed|svg|math|form|input|button|textarea|select|link|meta|base)\b[^>]*>.*?<\s*/\s*\1\s*>|<\s*(script|style|iframe|object|embed|svg|math|form|input|button|textarea|select|link|meta|base)\b[^>]*\/?>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
   private static partial Regex DangerousElementRegex();

   [GeneratedRegex(@"\s+on[a-z]+\s*=\s*(""[^""]*""|'[^']*'|[^\s>]+)", RegexOptions.IgnoreCase)]
   private static partial Regex EventAttributeRegex();

   [GeneratedRegex(@"\s+(?<attribute>href|src|action|formaction|xlink:href)\s*=\s*(?<quote>[""'])(?<url>.*?)(\k<quote>)", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
   private static partial Regex UnsafeUrlAttributeRegex();
}
