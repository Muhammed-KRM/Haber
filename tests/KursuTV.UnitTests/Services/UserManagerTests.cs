using FluentAssertions;
using KursuTV.Business.DTOs;
using KursuTV.Business.Exceptions;
using KursuTV.Business.Helpers;
using KursuTV.Business.Services;
using KursuTV.Data.Entities;
using KursuTV.Data.Enums;
using KursuTV.Data.Repositories;
using Moq;
using Xunit;

namespace KursuTV.UnitTests.Services;

public class UserManagerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly UserManager _manager;

    public UserManagerTests()
    {
        _manager = new UserManager(_userRepoMock.Object);
    }

    [Fact]
    public async Task GetProfileAsync_WhenUserNotFound_ShouldReturnNull()
    {
        var userId = Guid.NewGuid();
        _userRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _manager.GetProfileAsync(userId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProfileAsync_WhenUserExists_ShouldReturnUserDto()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "test@kursutv.com", FullName = "Test Kullanıcı", Role = UserRole.ColumnWriter };

        _userRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _manager.GetProfileAsync(userId);

        result.Should().NotBeNull();
        result!.Email.Should().Be("test@kursutv.com");
        result.FullName.Should().Be("Test Kullanıcı");
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenCurrentPasswordIncorrect_ShouldThrowBusinessException()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            PasswordHash = PasswordHasher.Hash("ActualPassword1!")
        };

        _userRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var dto = new ChangePasswordDto
        {
            CurrentPassword = "WrongPassword!",
            NewPassword = "NewPassword123!"
        };

        var action = () => _manager.ChangePasswordAsync(userId, dto);

        await action.Should().ThrowAsync<BusinessException>().WithMessage("*hatalı*");
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenValid_ShouldUpdatePasswordHashAndSave()
    {
        var userId = Guid.NewGuid();
        var oldPassword = "OldPassword1!";
        var newPassword = "BrandNewPassword123!";
        var user = new User
        {
            Id = userId,
            PasswordHash = PasswordHasher.Hash(oldPassword)
        };

        _userRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var dto = new ChangePasswordDto
        {
            CurrentPassword = oldPassword,
            NewPassword = newPassword
        };

        await _manager.ChangePasswordAsync(userId, dto);

        PasswordHasher.Verify(newPassword, user.PasswordHash).Should().BeTrue();
        _userRepoMock.Verify(r => r.Update(user), Times.Once);
        _userRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
