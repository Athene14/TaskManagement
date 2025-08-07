using Moq;
using NotificationService.Domain.Abstractions;
using NotificationService.Domain.Models;
using TaskManagementServices.Shared.Exceptions;
using TaskManagementServices.Shared.NotificationService.DTO;

namespace NotificationService.Tests
{
    public class NotificationServiceTests
    {
        private readonly Mock<INotificationRepository> _notificationRepoMock;
        private readonly Mock<INotificationRecipientRepository> _recipientRepoMock;
        private readonly App.Service.NotificationService _notificationService;

        public NotificationServiceTests()
        {
            _notificationRepoMock = new Mock<INotificationRepository>();
            _recipientRepoMock = new Mock<INotificationRecipientRepository>();
            _notificationService = new App.Service.NotificationService(
                _notificationRepoMock.Object,
                _recipientRepoMock.Object
            );
        }

        // GetUserNotificationsAsync
        [Fact]
        public async Task GetUserNotificationsAsync_ValidRequest_ReturnsNotifications()
        {
            var userId = Guid.NewGuid();
            var unreadOnly = true;

            var recipients = new List<NotificationRecipientDomainModel>
            {
                new() { NotificationId = Guid.NewGuid(), RecipientId = userId, IsRead = false },
                new() { NotificationId = Guid.NewGuid(), RecipientId = userId, IsRead = false }
            };

            var notificationIds = recipients.Select(r => r.NotificationId).ToArray();
            var notifications = new List<NotificationDomainModel>
            {
                new() { Id = notificationIds[0], Message = "Message 1", CreatedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                new() { Id = notificationIds[1], Message = "Message 2", CreatedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
            };

            _recipientRepoMock.Setup(repo => repo.GetByUserIdAsync(userId, unreadOnly))
                .ReturnsAsync(recipients);

            _notificationRepoMock.Setup(repo => repo.GetManyByIds(notificationIds))
                .ReturnsAsync(notifications);

            var result = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, r => Assert.False(r.IsRead));
        }

        [Fact]
        public async Task GetUserNotificationsAsync_NoNotifications_ReturnsEmptyList()
        {
            var userId = Guid.NewGuid();
            var unreadOnly = true;

            _recipientRepoMock.Setup(repo => repo.GetByUserIdAsync(userId, unreadOnly))
                .ReturnsAsync(new List<NotificationRecipientDomainModel>());

            var result = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly);

            Assert.NotNull(result);
            Assert.Empty(result);
        }



        [Fact]
        public async Task GetUserNotificationsAsync_MultipleNotificationsForSameUser_ReturnsCorrectData()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var notificationId = Guid.NewGuid();
            var message = "Test Message";
            var created = DateTimeOffset.UtcNow;

            var recipients = new List<NotificationRecipientDomainModel>
            {
                new() { NotificationId = notificationId, RecipientId = userId, IsRead = false }
            };

            var notifications = new List<NotificationDomainModel>
            {
                new() { Id = notificationId, Message = message, CreatedTimestamp = created.ToUnixTimeSeconds() }
            };

            _recipientRepoMock.Setup(repo => repo.GetByUserIdAsync(userId, false))
                .ReturnsAsync(recipients);

            _notificationRepoMock.Setup(repo => repo.GetManyByIds(It.IsAny<Guid[]>()))
                .ReturnsAsync(notifications);

            // Act
            var result = (await _notificationService.GetUserNotificationsAsync(userId, false)).FirstOrDefault();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(notificationId, result.Id);
            Assert.Equal(message, result.Message);
            Assert.Equal(userId, result.UserId);
            Assert.False(result.IsRead);
        }

        // CreateNotificationAsync
        [Fact]
        public async Task CreateNotificationAsync_ValidRequest_CreatesNotificationAndRecipients()
        {
            // Arrange
            var request = new CreateNotificationRequest
            {
                Message = "Important Notification",
                RecipientIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
            };

            NotificationDomainModel createdNotification = null;
            List<NotificationRecipientDomainModel> createdRecipients = null;

            _notificationRepoMock.Setup(repo => repo.CreateAsync(It.IsAny<NotificationDomainModel>()))
                .Callback<NotificationDomainModel>(n => createdNotification = n)
                .Returns(Task.FromResult(Guid.NewGuid()));

            _recipientRepoMock.Setup(repo => repo.AddRecipientsAsync(It.IsAny<IEnumerable<NotificationRecipientDomainModel>>()))
                .Callback<IEnumerable<NotificationRecipientDomainModel>>(r => createdRecipients = r.ToList())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _notificationService.CreateNotificationAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.CreatedNotificationId);

            // ѕроверка уведомлени€
            Assert.NotNull(createdNotification);
            Assert.Equal(request.Message, createdNotification.Message);
            Assert.True(createdNotification.CreatedTimestamp > 0);

            // ѕроверка получателей
            Assert.NotNull(createdRecipients);
            Assert.Equal(request.RecipientIds.Count, createdRecipients.Count);
            Assert.All(createdRecipients, r =>
            {
                Assert.Equal(createdNotification.Id, r.NotificationId);
                Assert.False(r.IsRead);
                Assert.Contains(r.RecipientId, request.RecipientIds);
            });
        }

        [Fact]
        public async Task CreateNotificationAsync_NullRequest_ThrowsInvalidArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidArgumentException>(() =>
                _notificationService.CreateNotificationAsync(null));
        }

        [Fact]
        public async Task CreateNotificationAsync_EmptyMessage_ThrowsInvalidArgumentException()
        {
            var request = new CreateNotificationRequest
            {
                Message = "   ",
                RecipientIds = new List<Guid> { Guid.NewGuid() }
            };

            var exception = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
                _notificationService.CreateNotificationAsync(request));
            Assert.Contains("Notification message cannot be empty", exception.Message);
        }

        [Fact]
        public async Task CreateNotificationAsync_EmptyRecipients_ThrowsInvalidArgumentException()
        {
            var request = new CreateNotificationRequest
            {
                Message = "Valid Message",
                RecipientIds = new List<Guid>()
            };

            var exception = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
                _notificationService.CreateNotificationAsync(request));
            Assert.Contains("Recipient list cannot be empty", exception.Message);
        }

        [Fact]
        public async Task CreateNotificationAsync_NullRecipients_ThrowsInvalidArgumentException()
        {
            // Arrange
            var request = new CreateNotificationRequest
            {
                Message = "Valid Message",
                RecipientIds = null
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
                _notificationService.CreateNotificationAsync(request));
            Assert.Contains("Recipient list cannot be empty", exception.Message);
        }

        [Fact]
        public async Task CreateNotificationAsync_LargeNumberOfRecipients_HandlesCorrectly()
        {
            var request = new CreateNotificationRequest
            {
                Message = "Mass Notification",
                RecipientIds = Enumerable.Range(0, 1000).Select(_ => Guid.NewGuid()).ToList()
            };

            var result = await _notificationService.CreateNotificationAsync(request);

            Assert.NotNull(result);
            _recipientRepoMock.Verify(repo =>
                repo.AddRecipientsAsync(It.Is<List<NotificationRecipientDomainModel>>(r => r.Count == 1000)),
                Times.Once);
        }

        [Fact]
        public async Task CreateNotificationAsync_LongMessage_HandlesCorrectly()
        {
            var longMessage = new string('a', 10000);
            var request = new CreateNotificationRequest
            {
                Message = longMessage,
                RecipientIds = new List<Guid> { Guid.NewGuid() }
            };

            var result = await _notificationService.CreateNotificationAsync(request);

            Assert.NotNull(result);
            _notificationRepoMock.Verify(repo =>
                repo.CreateAsync(It.Is<NotificationDomainModel>(n => n.Message == longMessage)),
                Times.Once);
        }

        [Fact]
        public async Task MarkAsReadAsync_ValidRequest_MarksAsRead()
        {
            var notificationId = Guid.NewGuid();
            var recipientId = Guid.NewGuid();

            await _notificationService.MarkAsReadAsync(notificationId, recipientId);

            _recipientRepoMock.Verify(repo =>
                repo.MarkAsReadAsync(recipientId, notificationId),
                Times.Once);
        }

        [Fact]
        public async Task MarkAsReadAsync_EmptyNotificationId_ThrowsInvalidArgumentException()
        {
            var emptyNotificationId = Guid.Empty;

            await Assert.ThrowsAsync<InvalidArgumentException>(()=> _notificationService.MarkAsReadAsync(emptyNotificationId, Guid.NewGuid()));
        }

        [Fact]
        public async Task MarkAsReadAsync_EmptyRecipientId_ThrowsInvalidArgumentException()
        {
            var emptyRecipientId = Guid.Empty;

            await Assert.ThrowsAsync<InvalidArgumentException>(() => _notificationService.MarkAsReadAsync(Guid.NewGuid(), emptyRecipientId));
        }


        // ѕроверка на дубликаты получателей в запросе. ƒубликаты должны отсе€тс€
        [Fact]
        public async Task CreateNotificationAsync_DuplicateRecipients_HandlesCorrectly()
        {
            var duplicateId = Guid.NewGuid();
            var request = new CreateNotificationRequest
            {
                Message = "Duplicate Test",
                RecipientIds = new List<Guid> { duplicateId, duplicateId, duplicateId }
            };

            await _notificationService.CreateNotificationAsync(request);

            _recipientRepoMock.Verify(repo =>
                repo.AddRecipientsAsync(It.Is<List<NotificationRecipientDomainModel>>(r =>
                    r.Count == 1 && r.All(rec => rec.RecipientId == duplicateId))), // провер€ем, что из 3х дубликатов уведомление получит 1
                Times.Once);
        }
    }
}