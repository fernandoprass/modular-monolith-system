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
using Shared.Domain.Enums;
using Shared.Domain.Messages;

namespace Courier.Application.Tests.Services;

public class NotificationServiceTests
{
   private readonly INotificationRepository _notificationRepository = Substitute.For<INotificationRepository>();
   private readonly INotificationValidator _notificationValidator = Substitute.For<INotificationValidator>();
   private readonly IUserContext _userContext = Substitute.For<IUserContext>();
   private readonly NotificationService _service;

   public NotificationServiceTests()
   {
      _service = new NotificationService(_notificationRepository, _notificationValidator, _userContext);
   }

   [Fact]
   public async Task GetAsync_ShouldUseUserOrganization_WhenUserIsNotSystemAdmin()
   {
      var organizationId = Guid.NewGuid();
      var request = CreateSearchRequest(Guid.NewGuid());
      var page = new PagedResultDto<NotificationLiteDto>([], 1, 25, 0, 0);
      _userContext.OrganizationId.Returns(organizationId);
      _notificationValidator.ValidateSearch(request).Returns(Result.Success());
      _notificationRepository.GetAsync(
         Arg.Is<NotificationSearchRequest>(value => value.OrganizationId == organizationId),
         Arg.Any<CancellationToken>()).Returns(page);

      var result = await _service.GetAsync(request, TestContext.Current.CancellationToken);

      result.HasError.Should().BeFalse();
      result.Data.Should().Be(page);
   }

   [Fact]
   public async Task GetByIdAsync_ShouldReturnNotFound_WhenNotificationDoesNotExist()
   {
      var id = Guid.NewGuid();
      _notificationRepository.GetByIdAsync(id, Arg.Any<CancellationToken>())
         .Returns((Notification?)null);

      var result = await _service.GetByIdAsync(id, TestContext.Current.CancellationToken);

      result.HasError.Should().BeTrue();
      result.Messages.Should().ContainSingle(message => message is NotFoundError);
   }

   [Fact]
   public async Task GetByIdAsync_ShouldReturnUnauthorized_WhenUserDoesNotOwnOrganization()
   {
      var notification = CreateNotification(Guid.NewGuid());
      _userContext.OrganizationId.Returns(Guid.NewGuid());
      _notificationRepository.GetByIdAsync(notification.Id, Arg.Any<CancellationToken>())
         .Returns(notification);

      var result = await _service.GetByIdAsync(notification.Id, TestContext.Current.CancellationToken);

      result.HasError.Should().BeTrue();
      result.Messages.Should().ContainSingle(message => message is UnauthorizedAccessError);
   }

   [Fact]
   public async Task MarkAsReadAsync_ShouldPersistReadStatus_WhenUserOwnsOrganization()
   {
      var organizationId = Guid.NewGuid();
      var notification = CreateNotification(organizationId);
      _userContext.OrganizationId.Returns(organizationId);
      _notificationRepository.GetByIdAsync(notification.Id, Arg.Any<CancellationToken>())
         .Returns(notification);

      var result = await _service.MarkAsReadAsync(notification.Id, TestContext.Current.CancellationToken);

      result.HasError.Should().BeFalse();
      notification.Status.Should().Be(NotificationStatus.Read);
      notification.ReadAt.Should().NotBeNull();
      await _notificationRepository.Received(1).UpdateAsync(notification, Arg.Any<CancellationToken>());
   }

   [Fact]
   public async Task GetUnreadCountAsync_ShouldReturnUserUnreadCount()
   {
      var organizationId = Guid.NewGuid();
      var userId = Guid.NewGuid();
      _userContext.OrganizationId.Returns(organizationId);
      _userContext.UserId.Returns(userId);
      _notificationRepository.GetUnreadCountAsync(
         organizationId,
         userId,
         Arg.Any<CancellationToken>()).Returns(3);

      var result = await _service.GetUnreadCountAsync(TestContext.Current.CancellationToken);

      result.HasError.Should().BeFalse();
      result.Data.Should().Be(3);
   }

   [Fact]
   public async Task DeleteAsync_ShouldDeleteNotification_WhenUserOwnsOrganization()
   {
      var organizationId = Guid.NewGuid();
      var notification = CreateNotification(organizationId);
      _userContext.OrganizationId.Returns(organizationId);
      _notificationRepository.GetByIdAsync(notification.Id, Arg.Any<CancellationToken>())
         .Returns(notification);

      var result = await _service.DeleteAsync(notification.Id, TestContext.Current.CancellationToken);

      result.HasError.Should().BeFalse();
      await _notificationRepository.Received(1).DeleteAsync(notification.Id, Arg.Any<CancellationToken>());
   }

   private static NotificationSearchRequest CreateSearchRequest(Guid? organizationId)
   {
      return new NotificationSearchRequest(
         organizationId,
         null,
         "iam",
         null,
         null,
         DateTime.UtcNow.AddDays(-1),
         DateTime.UtcNow);
   }

   private static Notification CreateNotification(Guid organizationId)
   {
      return Notification.Create(
         organizationId,
         Guid.NewGuid(),
         "iam",
         "user-welcome",
         "Account created",
         "Open your profile.",
         "/profile",
         RetentionPolicy.Standard);
   }
}
