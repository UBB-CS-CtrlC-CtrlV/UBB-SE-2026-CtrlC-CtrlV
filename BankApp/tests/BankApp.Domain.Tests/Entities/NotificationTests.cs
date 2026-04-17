using BankApp.Domain.Entities;

namespace BankApp.Domain.Tests.Entities;

/// <summary>
/// Unit tests for <see cref="Notification"/>.
/// </summary>
public class NotificationTests
{
    private const int DefaultNotificationId = 0;
    private const int DefaultUserId = 0;

    [Fact]
    public void Constructor_WhenCreated_SetsExpectedDefaults()
    {
        // Act
        DateTime timestampBeforeCreation = DateTime.UtcNow;
        var notification = new Notification();
        DateTime timestampAfterCreation = DateTime.UtcNow;

        // Assert
        notification.Id.Should().Be(DefaultNotificationId);
        notification.UserId.Should().Be(DefaultUserId);
        notification.Title.Should().BeEmpty();
        notification.Message.Should().BeEmpty();
        notification.Type.Should().BeEmpty();
        notification.Channel.Should().BeEmpty();
        notification.IsRead.Should().BeFalse();
        notification.RelatedEntityType.Should().BeNull();
        notification.RelatedEntityId.Should().BeNull();
        notification.CreatedAt.Should().BeOnOrAfter(timestampBeforeCreation);
        notification.CreatedAt.Should().BeOnOrBefore(timestampAfterCreation);
    }

    /// <summary>
    /// Verifies that all notification properties can be assigned and read back.
    /// </summary>
    [Fact]
    public void Properties_WhenAssigned_ReturnAssignedValues()
    {
        // Arrange
        const int testId = 7;
        const int testUserId = 9;
        const string testTitle = "Test Title";
        const string testMessage = "Test Message";
        const string testType = "Test Type";
        const string testChannel = "Test Channel";
        const bool testIsRead = true;
        const string testRelatedEntityType = "Test Related EntityType";
        const int testRelatedEntityId = 8;
        DateTime testCreatedAt = DateTime.UtcNow;
        var notification = new Notification()
        {
            Id = testId,
            UserId = testUserId,
            Title = testTitle,
            Message = testMessage,
            Type = testType,
            Channel = testChannel,
            IsRead = testIsRead,
            RelatedEntityType = testRelatedEntityType,
            RelatedEntityId = testRelatedEntityId,
            CreatedAt = testCreatedAt,
        };

        // Assert
        notification.Id.Should().Be(testId);
        notification.UserId.Should().Be(testUserId);
        notification.Title.Should().Be(testTitle);
        notification.Message.Should().Be(testMessage);
        notification.Type.Should().Be(testType);
        notification.Channel.Should().Be(testChannel);
        notification.IsRead.Should().Be(testIsRead);
        notification.RelatedEntityType.Should().Be(testRelatedEntityType);
        notification.RelatedEntityId.Should().Be(testRelatedEntityId);
        notification.CreatedAt.Should().Be(testCreatedAt);
    }
}
