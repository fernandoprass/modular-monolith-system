using Courier.Application.Contracts;
using Courier.Domain;
using Courier.Domain.DTOs.Requests;
using Courier.Domain.DTOs.Responses;
using Courier.Domain.Entities;
using Courier.Domain.Interfaces.Repositories;
using Courier.Domain.Mappers;
using Courier.Domain.Messages;
using Myce.Response;
using Shared.Application.Contracts;
using Shared.Domain.Messages;
using System.Text.RegularExpressions;

namespace Courier.Application.Services;

public partial class EmailTemplateService(
   IEmailTemplateWriteRepository emailTemplateRepository,
   IEmailTemplateValidator emailTemplateValidator,
   IUserContext userContext) : IEmailTemplateService
{
   private readonly IEmailTemplateWriteRepository _emailTemplateRepository = emailTemplateRepository;
   private readonly IEmailTemplateValidator _emailTemplateValidator = emailTemplateValidator;
   private readonly IUserContext _userContext = userContext;

   public async Task<Result<PagedResultDto<EmailTemplateDto>>> GetAsync(EmailTemplateSearchRequest request, CancellationToken cancellationToken = default)
   {
      var validation = _emailTemplateValidator.ValidateSearch(request);

      if (validation.HasError)
      {
         return Result<PagedResultDto<EmailTemplateDto>>.Failure(validation.Messages);
      }

      var templates = await _emailTemplateRepository.GetAsync(request, cancellationToken);
      return Result<PagedResultDto<EmailTemplateDto>>.Success(templates);
   }

   public async Task<Result<EmailTemplateDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      var template = await _emailTemplateRepository.GetByIdAsync(id, cancellationToken);

      if (template == null)
      {
         return Result<EmailTemplateDto>.Failure(new NotFoundError(CourierConst.Entity.EmailTemplate));
      }

      return Result<EmailTemplateDto>.Success(template.ToEmailTemplateDto());
   }

   public async Task<Result<EmailTemplateDto>> CreateAsync(EmailTemplateCreateRequest request, CancellationToken cancellationToken = default)
   {
      var keyExists = await _emailTemplateRepository.KeyExistsAsync(request.Key, cancellationToken: cancellationToken);
      var validation = _emailTemplateValidator.ValidateCreate(request, keyExists);

      if (validation.HasError)
      {
         return Result<EmailTemplateDto>.Failure(validation.Messages);
      }

      var template = EmailTemplate.Create(request.Key, request.Name, request.RetentionPolicy, _userContext.UserId);

      await _emailTemplateRepository.AddAsync(template, cancellationToken);

      return Result<EmailTemplateDto>.Success(template.ToEmailTemplateDto());
   }

   public async Task<Result> UpdateAsync(Guid id, EmailTemplateUpdateRequest request, CancellationToken cancellationToken = default)
   {
      var template = await _emailTemplateRepository.GetByIdAsync(id, cancellationToken);
      var keyExists = await _emailTemplateRepository.KeyExistsAsync(request.Key, id, cancellationToken);
      var validation = _emailTemplateValidator.ValidateUpdate(request, template != null, keyExists);

      if (validation.HasError)
      {
         return Result.Failure(validation.Messages);
      }

      template!.Update(request.Key, request.Name, request.RetentionPolicy, _userContext.UserId);
      await _emailTemplateRepository.UpdateAsync(template, cancellationToken);

      return Result.Success(new SuccessInfo());
   }

   public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
   {
      var template = await _emailTemplateRepository.GetByIdAsync(id, cancellationToken);

      if (template == null)
      {
         return Result.Failure(new NotFoundError(CourierConst.Entity.EmailTemplate));
      }

      await _emailTemplateRepository.DeleteAsync(id, cancellationToken);
      return Result.Success(new SuccessInfo());
   }

   public async Task<Result> AddTranslationAsync(Guid id, EmailTemplateTranslationRequest request, CancellationToken cancellationToken = default)
   {
      var validation = _emailTemplateValidator.ValidateTranslation(request);

      if (validation.HasError)
      {
         return Result.Failure(validation.Messages);
      }

      var template = await _emailTemplateRepository.GetByIdAsync(id, cancellationToken);

      if (template == null)
      {
         return Result.Failure(new NotFoundError(CourierConst.Entity.EmailTemplate));
      }

      var sanitizedBody = SanitizeTemplateBody(request.Body);

      if (!template.AddTranslation(request.Language, request.Subject, sanitizedBody, _userContext.UserId))
      {
         return Result.Failure(new EmailTemplateTranslationAlreadyExistsError(request.Language));
      }

      await _emailTemplateRepository.UpdateAsync(template, cancellationToken);
      return Result.Success(new SuccessInfo());
   }

   public async Task<Result> UpdateTranslationAsync(Guid id, string language, EmailTemplateTranslationRequest request, CancellationToken cancellationToken = default)
   {
      var validation = _emailTemplateValidator.ValidateTranslation(request);

      if (validation.HasError)
      {
         return Result.Failure(validation.Messages);
      }

      var template = await _emailTemplateRepository.GetByIdAsync(id, cancellationToken);

      if (template == null)
      {
         return Result.Failure(new NotFoundError(CourierConst.Entity.EmailTemplate));
      }

      var sanitizedBody = SanitizeTemplateBody(request.Body);

      if (!template.UpdateTranslation(language, request.Subject, sanitizedBody, _userContext.UserId))
      {
         return Result.Failure(new EmailTemplateTranslationNotFoundError(language));
      }

      await _emailTemplateRepository.UpdateAsync(template, cancellationToken);
      return Result.Success(new SuccessInfo());
   }

   public async Task<Result> RemoveTranslationAsync(Guid id, string language, CancellationToken cancellationToken = default)
   {
      var template = await _emailTemplateRepository.GetByIdAsync(id, cancellationToken);

      if (template == null)
      {
         return Result.Failure(new NotFoundError(CourierConst.Entity.EmailTemplate));
      }

      if (!template.RemoveTranslation(language, _userContext.UserId))
      {
         return Result.Failure(new EmailTemplateTranslationNotFoundError(language));
      }

      await _emailTemplateRepository.UpdateAsync(template, cancellationToken);
      return Result.Success(new SuccessInfo());
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
