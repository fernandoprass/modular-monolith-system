using Courier.Application.Contracts;
using Courier.Application.Services;
using Courier.Domain.DTOs.Requests;
using Courier.Domain.DTOs.Responses;
using Courier.Domain.Entities;
using Courier.Domain.Enums;
using Courier.Domain.Interfaces.Repositories;
using FluentAssertions;
using Myce.Response;
using NSubstitute;
using Shared.Application.Contracts;
using Shared.Domain.DTOs.Responses;
using Shared.Domain.Messages;

namespace Courier.Application.Tests.Services;

public class EmailServiceTests
{
   private readonly IEmailRepository _emailRepository = Substitute.For<IEmailRepository>();
   private readonly ITemplateRepository _templateRepository = Substitute.For<ITemplateRepository>();
   private readonly IEmailValidator _emailValidator = Substitute.For<IEmailValidator>();
   private readonly IUserContext _userContext = Substitute.For<IUserContext>();
   private readonly EmailService _service;

   public EmailServiceTests()
   {
      _service = new EmailService(_emailRepository, _templateRepository, _emailValidator, _userContext);
   }

   [Fact]
   public async Task GetAsync_ShouldKeepRequestedOrganization_WhenUserIsSystemAdmin()
   {
      var organizationId = Guid.NewGuid();
      var request = CreateSearchRequest(organizationId);
      var page = new PagedResultDto<EmailLiteDto>([], 1, 25, 0, 0);
      _userContext.IsSystemAdmin.Returns(true);
      _emailValidator.ValidateSearch(request).Returns(Result.Success());
      _emailRepository.GetAsync(
         Arg.Is<EmailSearchRequest>(r => r.OrganizationId == organizationId),
         Arg.Any<CancellationToken>()).Returns(page);

      var result = await _service.GetAsync(request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeFalse();
      result.Data.Should().Be(page);
   }

   [Fact]
   public async Task GetAsync_ShouldUseUserOwnerOrganization_WhenUserIsNotSystemAdmin()
   {
      var requestedOrganizationId = Guid.NewGuid();
      var userOwnerId = Guid.NewGuid();
      var request = CreateSearchRequest(requestedOrganizationId);
      var page = new PagedResultDto<EmailLiteDto>([], 1, 25, 0, 0);
      _userContext.IsSystemAdmin.Returns(false);
      _userContext.UserOwnerId.Returns(userOwnerId);
      _emailValidator.ValidateSearch(request).Returns(Result.Success());
      _emailRepository.GetAsync(
         Arg.Is<EmailSearchRequest>(r => r.OrganizationId == userOwnerId),
         Arg.Any<CancellationToken>()).Returns(page);

      var result = await _service.GetAsync(request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeFalse();
      result.Data.Should().Be(page);
   }

   [Fact]
   public async Task GetByIdAsync_ShouldReturnNotFound_WhenEmailDoesNotExist()
   {
      var id = Guid.NewGuid();
      _emailRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Email?)null);

      var result = await _service.GetByIdAsync(id, TestContext.Current.CancellationToken);

      result.HasError.Should().BeTrue();
      result.Messages.Should().ContainSingle(m => m is NotFoundError);
   }

   [Fact]
   public async Task GetByIdAsync_ShouldReturnEmail_WhenUserOwnsOrganization()
   {
      var organizationId = Guid.NewGuid();
      var email = CreateEmail(organizationId);
      _userContext.IsSystemAdmin.Returns(false);
      _userContext.UserOwnerId.Returns(organizationId);
      _emailRepository.GetByIdAsync(email.Id, Arg.Any<CancellationToken>()).Returns(email);

      var result = await _service.GetByIdAsync(email.Id, TestContext.Current.CancellationToken);

      result.HasError.Should().BeFalse();
      result.Data!.Id.Should().Be(email.Id);
   }

   [Fact]
   public async Task GetByIdAsync_ShouldReturnUnauthorized_WhenUserDoesNotOwnOrganization()
   {
      var email = CreateEmail(Guid.NewGuid());
      _userContext.IsSystemAdmin.Returns(false);
      _userContext.UserOwnerId.Returns(Guid.NewGuid());
      _emailRepository.GetByIdAsync(email.Id, Arg.Any<CancellationToken>()).Returns(email);

      var result = await _service.GetByIdAsync(email.Id, TestContext.Current.CancellationToken);

      result.HasError.Should().BeTrue();
      result.Messages.Should().ContainSingle(m => m is UnauthorizedAccessError);
   }

   [Fact]
   public async Task CreateAsync_ShouldPersistEmailAndReturnId()
   {
      var request = CreateRequest();
      var id = Guid.NewGuid();
      _emailValidator.ValidateCreate(request).Returns(Result.Success());
      _templateRepository.GetRetentionPolicyByKeyAsync(request.TemplateKey, Arg.Any<CancellationToken>())
         .Returns(RetentionPolicy.Standard);
      _emailRepository.AddAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns(id);

      var result = await _service.CreateAsync(request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeFalse();
      result.Data!.Id.Should().Be(id);
      await _emailRepository.Received(1).AddAsync(
         Arg.Is<Email>(e =>
            e.OrganizationId == request.OrganizationId &&
            e.UserId == request.UserId &&
            e.Module == request.Module &&
            e.Feature == request.Feature &&
            e.Subject == request.Subject &&
            e.Body == request.Body),
         Arg.Any<CancellationToken>());
   }

   private static EmailCreateRequest CreateRequest()
   {
      return new EmailCreateRequest(
         Guid.NewGuid(),
         Guid.NewGuid(),
         "IAM",
         "Users",
         "welcome",
         "person@example.com",
         "Subject",
         "Body",
         false);
   }

   private static EmailSearchRequest CreateSearchRequest(Guid? organizationId)
   {
      return new EmailSearchRequest(
         organizationId,
         null,
         "IAM",
         null,
         null,
         null,
         DateTime.UtcNow.AddDays(-1),
         DateTime.UtcNow);
   }

   private static Email CreateEmail(Guid organizationId)
   {
      return Email.Create(
         organizationId,
         Guid.NewGuid(),
         "IAM",
         "Users",
         "welcome",
         "person@example.com",
         "Subject",
         "Body",
         false,
         RetentionPolicy.Standard);
   }
}
