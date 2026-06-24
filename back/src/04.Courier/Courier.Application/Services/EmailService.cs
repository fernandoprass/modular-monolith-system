using Courier.Application.Contracts;
using Courier.Domain;
using Courier.Domain.DTOs.Requests;
using Courier.Domain.DTOs.Responses;
using Courier.Domain.Entities;
using Courier.Domain.Interfaces.Repositories;
using Courier.Domain.Mappers;
using Myce.Response;
using Shared.Application.Contracts;
using Shared.Application.Services;
using Shared.Domain.DTOs.Responses;
using Shared.Domain.Messages;

namespace Courier.Application.Services;

public class EmailService(
   IEmailRepository emailRepository,
   ITemplateRepository templateRepository,
   IEmailValidator emailValidator,
   IUserContext userContext) : BaseService(userContext), IEmailService
{
   private readonly IEmailRepository _emailRepository = emailRepository;
   private readonly ITemplateRepository _templateRepository = templateRepository;
   private readonly IEmailValidator _emailValidator = emailValidator;

   public async Task<Result<PagedResultDto<EmailLiteDto>>> GetAsync(EmailSearchRequest request, CancellationToken cancellationToken = default)
   {
      var validation = _emailValidator.ValidateSearch(request);

      if (validation.HasError)
      {
         return Result<PagedResultDto<EmailLiteDto>>.Failure(validation.Messages);
      }

      var searchRequest = _userContext.IsSystemAdmin
         ? request
         : request with { OrganizationId = _userContext.OrganizationId };

      var emails = await _emailRepository.GetAsync(searchRequest, cancellationToken);
      return Result<PagedResultDto<EmailLiteDto>>.Success(emails);
   }

   public async Task<Result<EmailDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      var email = await _emailRepository.GetByIdAsync(id, cancellationToken);

      if (email == null)
      {
         return Result<EmailDto>.Failure(new NotFoundError(CourierConst.Entity.Email));
      }

      return await ExecuteIfUserOwnsAsync(email.OrganizationId, _ =>
         Task.FromResult(Result<EmailDto>.Success(email.ToEmailDto())), cancellationToken);
   }

   public async Task<Result<EmailCreateDto>> CreateAsync(EmailCreateRequest request, CancellationToken cancellationToken = default)
   {
      var validation = _emailValidator.ValidateCreate(request);

      if (validation.HasError)
      {
         return Result<EmailCreateDto>.Failure(validation.Messages);
      }

      var retentionPolicy = await _templateRepository.GetRetentionPolicyByModuleAndKeyAsync(
         request.Module,
         request.TemplateKey,
         cancellationToken);

      if (retentionPolicy == null)
      {
         return Result<EmailCreateDto>.Failure(new NotFoundError(CourierConst.Entity.Template));
      }

      var email = Email.Create(
         request.OrganizationId,
         request.UserId,
         request.Module,
         request.Feature,
         request.TemplateKey,
         request.Recipient,
         request.Subject,
         request.Body,
         request.IsHtml,
         retentionPolicy.Value);

      var id = await _emailRepository.AddAsync(email, cancellationToken);

      return Result<EmailCreateDto>.Success(new EmailCreateDto(id));
   }
}
