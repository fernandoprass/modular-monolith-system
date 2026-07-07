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
using Myce.Response.Messages;
using Shared.Application.Contracts;
using Shared.Application.Services;
using Shared.Domain.Enums;
using Shared.Domain.Interfaces;
using Shared.Domain.Messages;

namespace IAM.Application.Orchestrators;

public class RegisterOrchestrator(
   IOrganizationService organizationService,
   IOrganizationQueryRepository organizationQueryRepository,
   IParameterService parameterService,
   IUserContext userContext,
   IUserRepository userRepository,
   IUserService userService,
   IIamUnitOfWork iamUnitOfWork,
   IIamEventPublisher eventPublisher,
   IEventPublisher? sharedEventPublisher = null) : BaseService(userContext, sharedEventPublisher), IRegisterOrchestrator
{
   private readonly IOrganizationService _organizationService = organizationService;
   private readonly IOrganizationQueryRepository _organizationQueryRepository = organizationQueryRepository;
   private readonly IParameterService _parameterService = parameterService;
   private readonly IUserRepository _userRepository = userRepository;
   private readonly IUserService _userService = userService;
   private readonly IIamUnitOfWork _iamUnitOfWork = iamUnitOfWork;
   private readonly IIamEventPublisher _eventPublisher = eventPublisher;

   public async Task<Result<UserDto>> RegisterUserAsync(UserCreateRequest request, CancellationToken cancellationToken = default)
   {
      var organizationDto = await _organizationQueryRepository.GetByIdAsync(request.OrganizationId, cancellationToken);

      var organizationExists = organizationDto is not null;

      var result = await _userService.CreateUserAsync(request, organizationExists, cancellationToken);

      if (result.IsSuccess)
      {
         var organizationName = organizationDto?.Name ?? string.Empty;
         result.Data!.OrganizationName = organizationName;
         await _eventPublisher.NotifyAuditLogAsync(
            IamConst.Logger.Feature.Users,
            IamConst.Logger.Action.Create,
            AuditPrivacyLevel.High,
            RetentionPolicy.LongTerm,
            description: $"Created user {request.Email} at {organizationName}",
            targetId: result.Data.Id,
            metadata: new { request.Name, request.Email, request.OrganizationId },
            cancellationToken: cancellationToken);
      }

      return result;
   }

   public async Task<Result<OrganizationDto>> RegisterOrganizationAsync(OrganizationCreateRequest request, CancellationToken cancellationToken = default)
   {
      var organizationValidateResult = await _organizationService.ValidateCreateOrganizationAsync(request, cancellationToken);
      var userValidateResult = await _userService.ValidateUserForNewOrganizationAsync(request.User, cancellationToken);

      var result = Result.Merge(organizationValidateResult, userValidateResult);

      var organization = Organization.Create(
         request.Type,
         request.Type.Equals(OrganizationType.Company) ? request.Code : _organizationService.GetRandomCode(),
         request.Type.Equals(OrganizationType.Company) ? request.Name : request.User.Name,
         request.Description,
         request.DefaultLanguage
      );

      if (result.HasError)
      {
         return Result<OrganizationDto>.Failure(result.Messages.WithLanguage(_userContext.Language));
      }

      var user = User.CreateOrganizationAdmin(
       request.User.Name,
       request.User.Email,
       Argon2.Hash(request.User.Password),
       DateTime.UtcNow.AddDays(30),
       isOrganizationAdmin: true,
       organization.DefaultLanguage,
       organization.Id
      );

      user.AddRole(await _parameterService.GetGuidAsync(IamParam.Role.DefaultRoleIdForNewOrganization, cancellationToken), DateTime.UtcNow, null);

      organization.CreatedBy = user.Id;

      await _iamUnitOfWork.Organizations.AddAsync(organization, cancellationToken);
      await _iamUnitOfWork.Users.AddAsync(user, cancellationToken);
      await _iamUnitOfWork.SaveChangesAsync(cancellationToken);

      await _eventPublisher.NotifyAuditLogAsync(
         IamConst.Logger.Feature.Organizations,
         IamConst.Logger.Action.Create,
         AuditPrivacyLevel.Medium,
         RetentionPolicy.LongTerm,
         description: $"Created organization {organization.Name}",
         targetId: organization.Id,
         metadata: request,
         cancellationToken: cancellationToken);

      await _eventPublisher.NotifyUserAsync(
         IamConst.Templates.OrganizationWelcome,
         organization.Id,
         user.Id,
         user.Email,
         IamConst.Logger.Feature.Organizations,
         BuildOrganizationTemplateValues(organization, user),
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

         await _eventPublisher.NotifyAuditLogAsync(
            IamConst.Logger.Feature.Organizations,
            IamConst.Logger.Action.Delete,
            AuditPrivacyLevel.Medium,
            RetentionPolicy.Compliance,
            description: $"Deleted organization {organization.Name}",
            targetId: organization.Id,
            metadata: new { Id = id, Code = organization.Code, Name = organization.Name },
            cancellationToken: ct);

         foreach (var user in users)
         {
            await _eventPublisher.NotifyUserAsync(
               IamConst.Templates.OrganizationDelete,
               organization.Id,
               user.Id,
               user.Email,
               IamConst.Logger.Feature.Organizations,
               BuildOrganizationTemplateValues(organization, user),
               ct);
         }

         return Result.Success(new SuccessInfo());
      }, cancellationToken);
   }

   private static IReadOnlyDictionary<string, string> BuildOrganizationTemplateValues(Organization organization, User user)
   {
      return new Dictionary<string, string>
      {
         ["organization.name"] = organization.Name,
         ["organization.code"] = organization.Code,
         ["user.name"] = user.Name,
         ["user.email"] = user.Email
      };
   }
}
