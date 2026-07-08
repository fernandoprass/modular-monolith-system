using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using IAM.Domain.Interfaces;
using IAM.Domain.QueryRepositories;
using IAM.Domain.Repositories;
using Myce.Response;
using Shared.Application.Contracts;
using Shared.Application.Services;
using Shared.Domain.DTOs.Responses;
using Shared.Domain.Enums;
using Shared.Domain.Interfaces;
using Shared.Domain.Messages;

namespace IAM.Application.Services;

public class OrganizationService(
    IOrganizationQueryRepository organizationQueryRepository,
    IOrganizationRepository organizationRepository,
    IOrganizationValidator organizationValidator,
    IIamUnitOfWork iamUnitOfWork,
    IUserContext userContext,
    IIamEventPublisher eventPublisher,
    IEventPublisher? sharedEventPublisher = null) : BaseService(userContext, sharedEventPublisher), IOrganizationService
{
   private readonly IOrganizationQueryRepository _organizationQueryRepository = organizationQueryRepository;
   private readonly IOrganizationRepository _organizationRepository = organizationRepository;
   private readonly IOrganizationValidator _organizationValidator = organizationValidator;
   private readonly IIamUnitOfWork _iamUnitOfWork = iamUnitOfWork;
   private readonly IIamEventPublisher _eventPublisher = eventPublisher;

   public async Task<Result> ValidateCreateOrganizationAsync(OrganizationCreateRequest request, CancellationToken cancellationToken = default)
   {
      var codeExists = await _organizationQueryRepository.ExistsByCodeAsync(request.Code, cancellationToken);

      var validation = _organizationValidator.ValidateCreate(request, codeExists);
      if (validation.HasError)
      {
         return Result.Failure(validation.Messages);
      }

      return Result.Success();
   }

   public async Task<Result<OrganizationDto?>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
   {
      return await ExecuteIfUserOwnsAsync(id, async (ct) =>
      { 
         var organizationDTo = await _organizationQueryRepository.GetByIdAsync(id, ct);
         return Result<OrganizationDto?>.Success(organizationDTo);
      }, cancellationToken);
   }

   public string GetRandomCode()
   {
      const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
      var random = new Random();
      return new string(Enumerable.Repeat(chars, IamConst.Organization.RandomCodeSize)
          .Select(s => s[random.Next(s.Length)]).ToArray());
   }

   public async Task<Result<PagedResultDto<OrganizationDto>>> GetAsync(OrganizationSearchRequest request, CancellationToken cancellationToken = default)
   {
      var searchRequest = request with
      {
         OrganizationId = _userContext.IsSystemAdmin ? request.OrganizationId : _userContext.OrganizationId
      };

      var organizations = await _organizationQueryRepository.GetAsync(searchRequest, cancellationToken);

      return Result<PagedResultDto<OrganizationDto>>.Success(organizations);
   }


   public async Task<Result<IEnumerable<OrganizationLookupDto>>> GetLookupAsync(OrganizationLookupRequest request, CancellationToken cancellationToken = default)
   {
      var searchRequest = request with
      {
         Id = _userContext.IsSystemAdmin ? request.Id : _userContext.OrganizationId
      };

      var organizations = await _organizationQueryRepository.GetLookupAsync(searchRequest, cancellationToken);

      return Result<IEnumerable<OrganizationLookupDto>>.Success(organizations);
   }

   public async Task<Result> UpdateAsync(Guid id, OrganizationUpdateRequest request, CancellationToken cancellationToken = default)
   {
      return await ExecuteIfUserOwnsAsync(id, async (ct) =>
      {
         var organization = await _organizationRepository.GetByIdAsync(id, ct);
         var organizationExists = organization is not null;

         var validation = _organizationValidator.ValidateUpdate(request, organizationExists);
         if (validation.HasError)
         {
            return Result.Failure(validation.Messages);
         }

         organization!.Update(request.Name, request.Description, request.IsActive, request.DefaultLanguage);

         var result = await CommitUpdateAsync(organization, ct);

         if (result.IsSuccess)
         {
            await _eventPublisher.NotifyAuditLogAsync(
               IamConst.Logger.Feature.Organizations,
               IamConst.Logger.Action.Update,
               AuditPrivacyLevel.Medium,
               RetentionPolicy.Compliance,
               $"Updated organization {organization.Id}",
               organization.Id,
               request,
               ct);
         }

         return result;
      }, cancellationToken);
   }

   public async Task<Result> UpdateCodeAsync(Guid id, OrganizationUpdateCodeRequest request, CancellationToken cancellationToken = default)
   {
      return await ExecuteIfUserOwnsAsync(id, async (ct) =>
      {
         var organization = await _organizationRepository.GetByCodeAsync(request.Code, ct);
         var newCodeExists = organization is not null;

         var validation = _organizationValidator.ValidateUpdateCode(request, newCodeExists);
         if (validation.HasError)
         {
            return Result.Failure(validation.Messages);
         }

         organization = await _organizationRepository.GetByIdAsync(id, ct);
         organization.Update(request.Code);

         var result = await CommitUpdateAsync(organization, ct);

         if (result.IsSuccess)
         {
            await _eventPublisher.NotifyAuditLogAsync(
               IamConst.Logger.Feature.Organizations,
               IamConst.Logger.Action.UpdateCode,
               AuditPrivacyLevel.Medium,
               RetentionPolicy.Compliance,
               $"Updated organization code {organization.Id}",
               organization.Id,
               request,
               ct);
         }

         return result;
      }, cancellationToken);
   }

   private async Task<Result> CommitUpdateAsync(Domain.Entities.Organization organization, CancellationToken cancellationToken)
   {
      _iamUnitOfWork.Organizations.Update(organization);
      await _iamUnitOfWork.SaveChangesAsync(cancellationToken);

      return Result.Success(new SuccessInfo());
   }
}
