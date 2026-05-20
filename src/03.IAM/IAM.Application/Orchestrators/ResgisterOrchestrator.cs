using IAM.Application.Contracts;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using IAM.Domain.Entities;
using IAM.Domain.Enums;
using IAM.Domain.Interfaces;
using IAM.Domain.Mappers;
using IAM.Domain.QueryRepositories;
using IAM.Domain.Repositories;
using Isopoh.Cryptography.Argon2;
using Myce.Response;
using Shared.Application.Contracts;
using Shared.Application.Services;
using Shared.Domain.Enums;
using Shared.Domain.Messages;

namespace IAM.Application.Orchestrators;

public class ResgisterOrchestrator(
   IOrganizationService organizationService,
   IOrganizationQueryRepository organizationQueryRepository,
   IUserContext userContext,
   IUserRepository userRepository,
   IUserService userService,
   IIamUnitOfWork iamUnitOfWork,
   IIamAuditLogger auditLogger) : BaseService(userContext), IRegisterOrchestrator
{
   private readonly IOrganizationService _organizationService = organizationService;
   private readonly IOrganizationQueryRepository _organizationQueryRepository = organizationQueryRepository;
   private readonly IUserRepository _userRepository = userRepository;
   private readonly IUserService _userService = userService;
   private readonly IIamUnitOfWork _iamUnitOfWork = iamUnitOfWork;
   private readonly IIamAuditLogger _auditLogger = auditLogger;

   public async Task<Result<UserDto>> RegisterUserAsync(UserCreateRequest request, CancellationToken cancellationToken = default)
   {
      var organizationDto = await _organizationQueryRepository.GetByIdAsync(request.OrganizationId, cancellationToken);

      var organizationExists = organizationDto is not null;

      var result = await _userService.CreateUserAsync(request, organizationExists, cancellationToken);

      if (result.IsSuccess)
      {
         result.Data.OrganizationName = organizationDto.Name;
         await _auditLogger.LogAsync(
            IamConst.Logger.Feature.Users,
            IamConst.Logger.Action.Create,
            AuditPrivacyLevel.High,
            $"Created user {request.Email}",
            result.Data.Id,
            new { request.Name, request.Email, request.OrganizationId },
            cancellationToken);
      }

      return result;
   }

   public async Task<Result<OrganizationDto>> RegisterOrganizationAsync(OrganizationCreateRequest organizationCreate, CancellationToken cancellationToken = default)
   {
      var organizationValidateResult = await _organizationService.ValidateCreateOrganizationAsync(organizationCreate, cancellationToken);
      var userValidateResult = await _userService.ValidateUserForNewOrganizationAsync(organizationCreate.User, cancellationToken);

      var result = Result.Merge(organizationValidateResult, userValidateResult);

      var organization = Organization.Create(
         organizationCreate.Type,
         organizationCreate.Type.Equals(OrganizationType.Company) ? organizationCreate.Code : _organizationService.GetRandomCode(),
         organizationCreate.Type.Equals(OrganizationType.Company) ? organizationCreate.Name : organizationCreate.User.Name,
         organizationCreate.Description
      );

      if (result.HasError)
      {
         return Result<OrganizationDto>.Failure(result.Messages);
      }

      var user = User.Create(
       organizationCreate.User.Name,
       organizationCreate.User.Email,
       Argon2.Hash(organizationCreate.User.Password),
       DateTime.UtcNow.AddDays(30),
       organization.Id
      );

      organization.CreatedBy = user.Id;

      await _iamUnitOfWork.Organizations.AddAsync(organization, cancellationToken);
      await _iamUnitOfWork.Users.AddAsync(user, cancellationToken);
      await _iamUnitOfWork.SaveChangesAsync(cancellationToken);

      await _auditLogger.LogAsync(
         IamConst.Logger.Feature.Organizations,
         IamConst.Logger.Action.Create,
         AuditPrivacyLevel.Medium,
         $"Created organization {organization.Code}",
         organization.Id,
         organizationCreate,
         cancellationToken);

      return Result<OrganizationDto>.Success(organization.ToOrganizationDto());
   }

   public async Task<Result> DeleteOrganizationAsync(Guid id, CancellationToken cancellationToken = default)
   {
      return await ExecuteIfUserOwnsAsync(id, async (ct) =>
      {
         var organization = await _iamUnitOfWork.Organizations.GetByIdAsync(id, ct);
         if (organization == null)
         {
            return Result.Failure(new NotFoundError(IamConst.Entity.Organization));
         }

         await _iamUnitOfWork.Organizations.DeleteAsync(id, ct);

         var users = await _userRepository.GetByOrganizationIdAsync(id, ct);
         foreach (var u in users)
         {
            await _iamUnitOfWork.Users.DeleteAsync(u.Id, ct);
         }

         await _iamUnitOfWork.SaveChangesAsync(ct);

         await _auditLogger.LogAsync(
            IamConst.Logger.Feature.Organizations,
            IamConst.Logger.Action.Delete,
            AuditPrivacyLevel.Medium,
            $"Deleted organization {id}",
            id,
            new { OrganizationId = id },
            ct);

         return Result.Success(new SuccessInfo());
      }, cancellationToken);
   }
}
