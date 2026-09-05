using FluentAssertions;
using IAM.Application.Contracts;
using IAM.Application.Orchestrators;
using IAM.Domain;
using IAM.Domain.DTOs.Requests;
using IAM.Domain.DTOs.Responses;
using IAM.Domain.Entities;
using IAM.Domain.Enums;
using IAM.Domain.Interfaces;
using IAM.Domain.Messages;
using IAM.Domain.QueryRepositories;
using IAM.Domain.Repositories;
using Myce.Response;
using NSubstitute;
using Shared.Application.Contracts;
using Shared.Domain;
using Shared.Domain.Enums;
using Shared.Domain.Messages;

namespace IAM.Application.Tests.Orchestrators;

public class RegisterOrchestratorTests
{
   private readonly IOrganizationService _organizationService;
   private readonly IOrganizationQueryRepository _organizationQueryRepository;
   private readonly IParameterService _parameterService;
   private readonly IUserContext _userContext;
   private readonly IUserRepository _userRepository;
   private readonly IUserService _userService;
   private readonly IIamUnitOfWork _unitOfWork;
   private readonly IOrganizationRepository _organizationRepository;
   private readonly IIamEventPublisher _eventPublisher;
   private readonly RegisterOrchestrator _orchestrator;

   public RegisterOrchestratorTests()
   {
      _organizationService = Substitute.For<IOrganizationService>();
      _organizationQueryRepository = Substitute.For<IOrganizationQueryRepository>();
      _parameterService = Substitute.For<IParameterService>();
      _userContext = Substitute.For<IUserContext>();
      _userRepository = Substitute.For<IUserRepository>();
      _userService = Substitute.For<IUserService>();
      _unitOfWork = Substitute.For<IIamUnitOfWork>();
      _organizationRepository = Substitute.For<IOrganizationRepository>();
      _eventPublisher = Substitute.For<IIamEventPublisher>();

      _unitOfWork.Organizations.Returns(_organizationRepository);
      _unitOfWork.Users.Returns(_userRepository);

      _orchestrator = new RegisterOrchestrator(
         _organizationService,
         _organizationQueryRepository,
         _parameterService,
         _userContext,
         _userRepository,
         _userService,
         _unitOfWork,
         _eventPublisher);
   }

   [Fact]
   public async Task RegisterUserAsync_ShouldSetOrganizationNameAndAudit_WhenUserIsCreated()
   {
      var organizationId = Guid.NewGuid();
      var userId = Guid.NewGuid();
      var request = CreateUserRequest(organizationId);
      var userDto = new UserDto { Id = userId, Email = request.Email, OrganizationId = organizationId };
      var organizationDto = new OrganizationDto(
         organizationId,
         OrganizationType.Company,
         "ORG",
         "Organization Name",
         "description",
         LanguageOptions.English,
         true);

      _organizationQueryRepository.GetByIdAsync(organizationId, Arg.Any<CancellationToken>())
         .Returns(organizationDto);
      _userService.CreateUserAsync(request, true, Arg.Any<CancellationToken>())
         .Returns(Result<UserDto>.Success(userDto));

      var result = await _orchestrator.RegisterUserAsync(request, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeTrue();
      result.Data!.OrganizationName.Should().Be(organizationDto.Name);
      await _eventPublisher.Received(1).NotifyAuditLogAsync(
         IamConst.Logger.Feature.Users,
         IamConst.Logger.Action.Create,
         AuditPrivacyLevel.High,
         RetentionPolicy.LongTerm,
         Arg.Any<string>(),
         userId,
         Arg.Any<object>(),
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task RegisterUserAsync_ShouldNotAudit_WhenUserCreationFails()
   {
      var organizationId = Guid.NewGuid();
      var request = CreateUserRequest(organizationId);

      _organizationQueryRepository.GetByIdAsync(organizationId, Arg.Any<CancellationToken>())
         .Returns((OrganizationDto?)null);
      _userService.CreateUserAsync(request, false, Arg.Any<CancellationToken>())
         .Returns(Result<UserDto>.Failure(new NotFoundError(IamConst.Entity.Organization)));

      var result = await _orchestrator.RegisterUserAsync(request, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeFalse();
      await _eventPublisher.DidNotReceive().NotifyAuditLogAsync(
         Arg.Any<string>(),
         Arg.Any<string>(),
         Arg.Any<AuditPrivacyLevel>(),
         Arg.Any<RetentionPolicy>(),
         Arg.Any<string>(),
         Arg.Any<Guid?>(),
         Arg.Any<object?>(),
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task RegisterOrganizationAsync_ShouldReturnErrorsAndNotSave_WhenValidationFails()
   {
      var request = CreateOrganizationRequest();

      _organizationService.ValidateCreateOrganizationAsync(request, Arg.Any<CancellationToken>())
         .Returns(Result.Failure(OrganizationErrorMessages.DuplicateCode(request.Code)));
      _userService.ValidateUserForNewOrganizationAsync(request.User, Arg.Any<CancellationToken>())
         .Returns(Result.Success());

      var result = await _orchestrator.RegisterOrganizationAsync(request, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeFalse();
      await _organizationRepository.DidNotReceive().AddAsync(Arg.Any<Organization>(), Arg.Any<CancellationToken>());
      await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
      await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task RegisterOrganizationAsync_ShouldSaveOrganizationAndUserAndPublishEvents_WhenValid()
   {
      var roleId = Guid.NewGuid();
      var request = CreateOrganizationRequest();

      _organizationService.ValidateCreateOrganizationAsync(request, Arg.Any<CancellationToken>())
         .Returns(Result.Success());
      _userService.ValidateUserForNewOrganizationAsync(request.User, Arg.Any<CancellationToken>())
         .Returns(Result.Success());
      _parameterService.GetGuidAsync(IamParam.Role.DefaultRoleIdForNewOrganization, Arg.Any<CancellationToken>())
         .Returns(roleId);

      var result = await _orchestrator.RegisterOrganizationAsync(request, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeTrue();
      result.Data!.Name.Should().Be(request.Name);
      await _organizationRepository.Received(1).AddAsync(
         Arg.Is<Organization>(o => o.Name == request.Name && o.Code == request.Code),
         Arg.Any<CancellationToken>());
      await _userRepository.Received(1).AddAsync(
         Arg.Is<User>(u =>
            u.Email == request.User.Email.ToLowerInvariant().Trim() &&
            u.OrganizationId == result.Data.Id &&
            u.UserRoles.Any(r => r.RoleId == roleId)),
         Arg.Any<CancellationToken>());
      await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
      await _eventPublisher.Received(1).NotifyAuditLogAsync(
         IamConst.Logger.Feature.Organizations,
         IamConst.Logger.Action.Create,
         AuditPrivacyLevel.Medium,
         RetentionPolicy.LongTerm,
         Arg.Any<string>(),
         result.Data.Id,
         request,
         Arg.Any<CancellationToken>());
      await _eventPublisher.Received(1).NotifyUserAsync(
         IamConst.Templates.OrganizationWelcome,
         result.Data.Id,
         Arg.Any<Guid>(),
         request.User.Email.ToLowerInvariant().Trim(),
         IamConst.Logger.Feature.Organizations,
         Arg.Any<IReadOnlyDictionary<string, string>>(),
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task DeleteOrganizationAsync_ShouldReturnNotFound_WhenOrganizationDoesNotExist()
   {
      var organizationId = Guid.NewGuid();
      _userContext.OrganizationId.Returns(organizationId);
      _organizationRepository.GetByIdAsync(organizationId, Arg.Any<CancellationToken>())
         .Returns((Organization?)null);

      var result = await _orchestrator.DeleteOrganizationAsync(organizationId, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeFalse();
      result.Messages.Should().ContainSingle(m => m is NotFoundError);
      await _organizationRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
      await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task DeleteOrganizationAsync_ShouldDeleteOrganizationAndUsersAndPublishEvents_WhenOrganizationExists()
   {
      var organization = Organization.Create(
         OrganizationType.Company,
         "ORG",
         "Organization Name",
         "description",
         LanguageOptions.English);
      var firstUser = User.Create("First User", "first@test.com", "hash", DateTime.UtcNow, LanguageOptions.English, organization.Id);
      var secondUser = User.Create("Second User", "second@test.com", "hash", DateTime.UtcNow, LanguageOptions.English, organization.Id);

      _userContext.OrganizationId.Returns(organization.Id);
      _organizationRepository.GetByIdAsync(organization.Id, Arg.Any<CancellationToken>())
         .Returns(organization);
      _userRepository.GetByOrganizationIdAsync(organization.Id, Arg.Any<CancellationToken>())
         .Returns([firstUser, secondUser]);

      var result = await _orchestrator.DeleteOrganizationAsync(organization.Id, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeTrue();
      await _organizationRepository.Received(1).DeleteAsync(organization.Id, Arg.Any<CancellationToken>());
      await _userRepository.Received(1).DeleteAsync(firstUser.Id, Arg.Any<CancellationToken>());
      await _userRepository.Received(1).DeleteAsync(secondUser.Id, Arg.Any<CancellationToken>());
      await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
      await _eventPublisher.Received(1).NotifyAuditLogAsync(
         IamConst.Logger.Feature.Organizations,
         IamConst.Logger.Action.Delete,
         AuditPrivacyLevel.Medium,
         RetentionPolicy.Compliance,
         Arg.Any<string>(),
         organization.Id,
         Arg.Any<object>(),
         Arg.Any<CancellationToken>());
      await _eventPublisher.Received(2).NotifyUserAsync(
         IamConst.Templates.OrganizationDelete,
         organization.Id,
         Arg.Any<Guid>(),
         Arg.Any<string>(),
         IamConst.Logger.Feature.Organizations,
         Arg.Any<IReadOnlyDictionary<string, string>>(),
         Arg.Any<CancellationToken>());
   }

   private static UserCreateRequest CreateUserRequest(Guid organizationId)
   {
      return new UserCreateRequest(
         "Test User",
         "test@example.com",
         "Strong#Pass123",
         LanguageOptions.English,
         organizationId);
   }

   private static OrganizationCreateRequest CreateOrganizationRequest()
   {
      return new OrganizationCreateRequest(
         OrganizationType.Company,
         "Organization Name",
         "ORG",
         "description",
         LanguageOptions.PortugueseBrazil,
         new OrganizationUserCreateRequest("Admin User", "admin@example.com", "Strong#Pass123"));
   }
}
