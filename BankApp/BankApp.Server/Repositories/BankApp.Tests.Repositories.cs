using Xunit;
using Moq;
using System.Collections.Generic;
using BankApp.Server.Repositories.Implementations;
using BankApp.Server.DataAccess.Interfaces;
using BankApp.Models.Entities;

public class UserRepositoryTests
{
    private readonly Mock<IUserDAO> _userDaoMock;
    private readonly Mock<ISessionDAO> _sessionDaoMock;
    private readonly Mock<IOAuthLinkDAO> _oauthDaoMock;
    private readonly Mock<INotificationPreferenceDAO> _notifDaoMock;

    private readonly UserRepository _userRepository;

    public UserRepositoryTests()
    {
        _userDaoMock = new Mock<IUserDAO>();
        _sessionDaoMock = new Mock<ISessionDAO>();
        _oauthDaoMock = new Mock<IOAuthLinkDAO>();
        _notifDaoMock = new Mock<INotificationPreferenceDAO>();

        _userRepository = new UserRepository(
            _userDaoMock.Object,
            _sessionDaoMock.Object,
            _oauthDaoMock.Object,
            _notifDaoMock.Object
        );
    }


    [Fact]
    public void UpdateUser_ReturnsTrue_WhenSuccessful()
    {
        var user = new User { Id = 1 };
        _userDaoMock.Setup(d => d.Update(user)).Returns(true);

        var result = _userRepository.UpdateUser(user);

        Assert.True(result);
    }

    [Fact]
    public void UpdateUser_ReturnFalse_WhenSuccess()
    {
        var user = new User { Id = 1 };
        _userDaoMock.Setup(d => d.Update(user)).Returns(true);

        var result = _userRepository.UpdateUser(user);

        Assert.False(result);
    }

    //Session
    [Fact]
    public void GetActiveSessions_ReturnsSessions()
    {
        var sessions = new List<Session> { new Session { Id = 1 } };
        _sessionDaoMock.Setup(d => d.FindByUserId(1)).Returns(sessions);

        var result = _userRepository.GetActiveSessions(1);

        Assert.Single(result);
    }

    [Fact]
    public void DeleteOAuthLink_CallsDao()
    {
        _userRepository.DeleteOAuthLink(10);

        _oauthDaoMock.Verify(d => d.Delete(10), Times.Once);
    }

    //Notification Preferences

    [Fact]
    public void GetNotificationPreferences_ReturnsPreferences()
    {
        var prefs = new List<NotificationPreference> { new NotificationPreference { Id = 1 } };

        _notifDaoMock.Setup(d => d.FindByUserId(1)).Returns(prefs);

        var result = _userRepository.GetNotificationPreferences(1);

        Assert.Single(result);

    }

    [Fact]

    public void UpdateNotificationPreferences_ReturnsTrue()
    {
        var prefs = new List<NotificationPreference> { new NotificationPreference { Id = 1 } };

        _notifDaoMock.Setup(d => d.Update(1, prefs)).Returns(true);

        var result = _userRepository.UpdateNotificationPreferences(1, prefs);

        Assert.True(result);
    }


}