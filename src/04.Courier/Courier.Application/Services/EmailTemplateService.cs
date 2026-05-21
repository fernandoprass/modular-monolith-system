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

namespace Courier.Application.Services;

public class EmailTemplateService(
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

      var template = EmailTemplate.Create(request.Key, request.RetentionPolicy, _userContext.UserId);

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

      template!.Update(request.Key, request.RetentionPolicy, _userContext.UserId);
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

      if (!template.AddTranslation(request.Language, request.Name, request.Subject, request.Body, _userContext.UserId))
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

      if (!template.UpdateTranslation(language, request.Name, request.Subject, request.Body, _userContext.UserId))
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
}
