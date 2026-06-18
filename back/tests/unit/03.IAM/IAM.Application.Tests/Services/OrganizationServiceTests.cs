using IAM.Application.Contracts;
using IAM.Application.Services;
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
using Shared.Domain.DTOs.Responses;
using Shared.Domain.Enums;
using Shared.Domain.Messages;

namespace IAM.Application.Tests.Services;

public class OrganizationServiceTests
{
   private readonly IOrganizationQueryRepository _organizationQueryRepository;
   private readonly IOrganizationRepository _organizationRepository;
   private readonly IOrganizationValidator _organizationValidator;
   private readonly IIamUnitOfWork _unitOfWork;
   private readonly IUserContext _userContext;
   private readonly IIamEventPublisher _eventPublisher;
   private readonly OrganizationService _service;

   public OrganizationServiceTests()
   {
      _organizationQueryRepository = Substitute.For<IOrganizationQueryRepository>();
      _organizationRepository = Substitute.For<IOrganizationRepository>();
      _organizationValidator = Substitute.For<IOrganizationValidator>();
      _unitOfWork = Substitute.For<IIamUnitOfWork>();
      _userContext = Substitute.For<IUserContext>();
      _eventPublisher = Substitute.For<IIamEventPublisher>();

      _service = new OrganizationService(
          _organizationQueryRepository,
          _organizationRepository,
          _organizationValidator,
          _unitOfWork,
          _userContext,
          _eventPublisher);
   }

   [Fact]
   public async Task ValidateCreateOrganizationAsync_WhenValidatorSucceeds_ReturnsSuccess()
   {
      var request = GetOrganizationCreateRequest(OrganizationType.Company, "Organization Name", "Code1");

      _organizationQueryRepository.ExistsByCodeAsync(request.Code, Arg.Any<CancellationToken>()).Returns(false);
      _organizationValidator.ValidateCreate(request, false).Returns(Result.Success());

      var result = await _service.ValidateCreateOrganizationAsync(request, TestContext.Current.CancellationToken);

      Assert.True(result.IsSuccess);
   }

   [Fact]
   public async Task ValidateCreateOrganizationAsync_WhenValidatorFails_ReturnsFailure()
   {
      var request = GetOrganizationCreateRequest(OrganizationType.Company, "Organization Name", "Code1");

      _organizationQueryRepository.ExistsByCodeAsync(request.Code, Arg.Any<CancellationToken>()).Returns(true);
      _organizationValidator.ValidateCreate(request, true).Returns(Result.Failure(new OrganizationDuplicateCodeError(request.Code)));

      var result = await _service.ValidateCreateOrganizationAsync(request, TestContext.Current.CancellationToken);

      Assert.False(result.IsSuccess);
   }

   [Fact]
   public async Task GetByIdAsync_WhenOrganizationExists_ReturnsOrganizationDto()
   {
      var id = Guid.NewGuid();
      var expected = new OrganizationDto(Id: id, Type: OrganizationType.Company, Code: "ABC", Name: "Test", Description: null, LanguageOptions.English, IsActive: true);

      _userContext.OrganizationId.Returns(id);
      _organizationQueryRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(expected);

      var result = await _service.GetByIdAsync(id, TestContext.Current.CancellationToken);

      Assert.True(result.IsSuccess);
      Assert.Equal(expected, result.Data);
   }

   [Fact]
   public void GetRandomCode_WhenCalled_ReturnsStringWithCorrectSize()
   {
      var result = _service.GetRandomCode();

      Assert.NotNull(result);
      Assert.False(string.IsNullOrWhiteSpace(result));
   }

   [Fact]
   public async Task GetAsync_WhenUserIsSystemAdmin_ShouldUseRequestedFilters()
   {
      var organizationId = Guid.NewGuid();
      var request = new OrganizationSearchRequest(Code: "ABC", Name: "SearchName", OrganizationId: organizationId);
      var expected = new PagedResultDto<OrganizationDto>(
         [new(Id: organizationId, Type: OrganizationType.Company, Code: "ABC", Name: request.Name!, Description: null, LanguageOptions.English, IsActive: true)],
         1,
         25,
         1,
         1);

      _userContext.IsSystemAdmin.Returns(true);
      _organizationQueryRepository.GetAsync(request, Arg.Any<CancellationToken>()).Returns(expected);

      var result = await _service.GetAsync(request, TestContext.Current.CancellationToken);

      Assert.True(result.IsSuccess);
      Assert.Equal(expected, result.Data);
      await _organizationQueryRepository.Received(1).GetAsync(request, Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task GetAsync_WhenUserIsNotSystemAdmin_ShouldForceOrganizationId()
   {
      var organizationId = Guid.NewGuid();
      var requestedOrganizationId = Guid.NewGuid();
      var request = new OrganizationSearchRequest(Code: "ABC", Name: "SearchName", OrganizationId: requestedOrganizationId);
      var expectedRequest = request with { OrganizationId = organizationId };
      var expected = new PagedResultDto<OrganizationDto>([], 1, 25, 0, 0);

      _userContext.IsSystemAdmin.Returns(false);
      _userContext.OrganizationId.Returns(organizationId);
      _organizationQueryRepository.GetAsync(expectedRequest, Arg.Any<CancellationToken>()).Returns(expected);

      var result = await _service.GetAsync(request, TestContext.Current.CancellationToken);

      Assert.True(result.IsSuccess);
      Assert.Equal(expected, result.Data);
      await _organizationQueryRepository.Received(1).GetAsync(expectedRequest, Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UpdateAsync_WhenOwnershipAndValidatorSucceed_ReturnsSuccess()
   {
      var id = Guid.NewGuid();
      var request = GetOrganizationUpdateRequest("New Organization Name", "description", true);

      var organization = Organization.Create(OrganizationType.Company, "OriginalCode", "Original Name", "description", LanguageOptions.English);

      _userContext.OrganizationId.Returns(id);
      _organizationRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(organization);
      _organizationValidator.ValidateUpdate(request, true).Returns(Result.Success());

      var result = await _service.UpdateAsync(id, request, TestContext.Current.CancellationToken);

      Assert.True(result.IsSuccess);
      _unitOfWork.Organizations.Received(1).Update(organization);
      await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
      await _eventPublisher.NotifyAuditLogAsync(
         IamConst.Logger.Feature.Organizations,
         IamConst.Logger.Action.Update,
         AuditPrivacyLevel.Medium,
         Arg.Any<RetentionPolicy>(),
         Arg.Any<string>(),
         organization.Id,
         request,
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UpdateAsync_WhenValidatorFails_ReturnsFailure()
   {
      var id = Guid.NewGuid();
      var request = GetOrganizationUpdateRequest(string.Empty, "description", true);
      var organization = Organization.Create(OrganizationType.Company, "OriginalCode", "Original Name", "description", LanguageOptions.English);

      _userContext.OrganizationId.Returns(id);
      _organizationRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(organization);
      _organizationValidator.ValidateUpdate(request, true).Returns(Result.Failure(new NotFoundError()));

      var result = await _service.UpdateAsync(id, request, TestContext.Current.CancellationToken);

      Assert.False(result.IsSuccess);
   }

   [Fact]
   public async Task UpdateCodeAsync_WhenOwnershipAndValidatorSucceed_ReturnsSuccess()
   {
      var id = Guid.NewGuid();
      var request = new OrganizationUpdateCodeRequest("NEWCODE");
      var organization = Organization.Create(OrganizationType.Company, "OriginalCode", "Original Name", "description", LanguageOptions.English);

      _userContext.OrganizationId.Returns(id);
      _organizationRepository.GetByCodeAsync(request.Code, Arg.Any<CancellationToken>()).Returns((Organization)null);
      _organizationValidator.ValidateUpdateCode(request, false).Returns(Result.Success());
      _organizationRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(organization);

      var result = await _service.UpdateCodeAsync(id, request, TestContext.Current.CancellationToken);

      Assert.True(result.IsSuccess);
      Assert.Equal("NEWCODE", organization.Code);
      await _eventPublisher.NotifyAuditLogAsync(
         IamConst.Logger.Feature.Organizations,
         IamConst.Logger.Action.UpdateCode,
         AuditPrivacyLevel.Medium,
         Arg.Any<RetentionPolicy>(),
         Arg.Any<string>(),
         organization.Id,
         request,
         Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task UpdateCodeAsync_WhenNewCodeAlreadyExists_ReturnsFailure()
   {
      var id = Guid.NewGuid();
      var request = new OrganizationUpdateCodeRequest("EXISTING");
      var existingOrganization = Organization.Create(OrganizationType.Company, "EXISTING", "Original Name", "description", LanguageOptions.English);

      _userContext.OrganizationId.Returns(id);
      _organizationRepository.GetByCodeAsync(request.Code, Arg.Any<CancellationToken>()).Returns(existingOrganization);
      _organizationValidator.ValidateUpdateCode(request, true).Returns(Result.Failure(new OrganizationDuplicateCodeError(request.Code)));

      var result = await _service.UpdateCodeAsync(id, request, TestContext.Current.CancellationToken);

      Assert.False(result.IsSuccess);
   }

   private static OrganizationCreateRequest GetOrganizationCreateRequest(OrganizationType type, string name, string code)
   {
      var user = new OrganizationUserCreateRequest(string.Empty, string.Empty, string.Empty);
      var request = new OrganizationCreateRequest(type, name, code, "some description", LanguageOptions.English, user);
      return request;
   }

   private static OrganizationUpdateRequest GetOrganizationUpdateRequest(string name, string description, bool isActive)
   {
      var request = new OrganizationUpdateRequest(name, description, isActive, LanguageOptions.English);
      return request;
   }
}
