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

namespace Courier.Application.Services;

public class TemplateService(
   ITemplateWriteRepository templateRepository,
   ITemplateValidator templateValidator,
   IUserContext userContext,
   IHtmlSanitizer htmlSanitizer) : ITemplateService
{
   private readonly ITemplateWriteRepository _templateRepository = templateRepository;
   private readonly ITemplateValidator _templateValidator = templateValidator;
   private readonly IUserContext _userContext = userContext;
   private readonly IHtmlSanitizer _htmlSanitizer = htmlSanitizer;

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

   private TemplateTranslation ToTranslation(TemplateTranslationRequest request)
   {
      var email = request.Email == null
         ? null
         : TemplateTranslationEmail.Create(
            request.Email.Subject,
            _htmlSanitizer.Sanitize(request.Email.Body));
      var notification = request.Notification == null
         ? null
         : TemplateTranslationNotification.Create(
            request.Notification.Title,
            request.Notification.Message,
            request.Notification.ActionLink);

      return TemplateTranslation.Create(request.Language, request.Name, email, notification);
   }
}
