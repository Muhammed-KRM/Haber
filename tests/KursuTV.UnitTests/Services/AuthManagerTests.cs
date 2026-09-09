using FluentAssertions;
using FluentValidation;
using FluentValidationResult = FluentValidation.Results.ValidationResult;
using FluentValidationFailure = FluentValidation.Results.ValidationFailure;
using KursuTV.Business.DTOs;
using KursuTV.Business.Exceptions;
using KursuTV.Business.Helpers;
using KursuTV.Business.Interfaces;
using KursuTV.Business.Services;
using KursuTV.Data.Entities;
using KursuTV.Data.Enums;
using KursuTV.Data.Repositories;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace KursuTV.UnitTests.Services;

public class AuthManagerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IValidator<UserRegisterDto>> _registerValidatorMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();
    private readonly Mock<IConfiguration> _configMock = new();
    private readonly Mock<ILogService> _logServiceMock = new();
    private readonly AuthManager _manager;

    public AuthManagerTests()
    {
        _manager = new AuthManager(
            _userRepoMock.Object,
            _registerValidatorMock.Object,
            _emailServiceMock.Object,
            _publishEndpointMock.Object,
            _configMock.Object,
            _logServiceMock.Object
        );
    }

    [Fact]
    public async Task RegisterAsync_WhenValidationFails_ShouldReturnFailure()
    {
        // Arrange
        var dto = new UserRegisterDto { Email = "invalid" };
        var validationFailures = new List<FluentValidationFailure> { new("Email", "E-posta geçersiz") };
        _registerValidatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidationResult(validationFailures));

        // Act
        var result = await _manager.RegisterAsync(dto);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("E-posta geçersiz");
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyRegistered_ShouldReturnFailure()
    {
        // Arrange
        var dto = new UserRegisterDto { Email = "existing@kursutv.com", Password = "Password123!", FullName = "Var Olan" };
        _registerValidatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidationResult());

        _userRepoMock
            .Setup(r => r.IsEmailUniqueAsync(dto.Email))
            .ReturnsAsync(false);

        // Act
        var result = await _manager.RegisterAsync(dto);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("zaten kayıtlı");
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldSaveUserAndReturnSuccess()
    {
        // Arrange
        var dto = new UserRegisterDto { Email = "new@kursutv.com", Password = "Password123!", FullName = "Yeni Muhabir" };
        _registerValidatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidationResult());

        _userRepoMock
            .Setup(r => r.IsEmailUniqueAsync(dto.Email))
            .ReturnsAsync(true);

        _userRepoMock
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        _userRepoMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _manager.RegisterAsync(dto);

        // Assert
        result.Success.Should().BeTrue();
        result.User.Should().NotBeNull();
        result.User!.Email.Should().Be("new@kursutv.com");
        _userRepoMock.Verify(r => r.AddAsync(It.Is<User>(u => u.Email == dto.Email && u.Role == UserRole.Reporter)), Times.Once);
        _userRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var dto = new UserLoginDto { Email = "notfound@kursutv.com", Password = "Pass" };
        _userRepoMock
            .Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _manager.LoginAsync(dto);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("E-posta veya şifre hatalı");
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIncorrect_ShouldReturnFailure()
    {
        // Arrange
        var realPassword = "CorrectPassword123!";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@kursutv.com",
            PasswordHash = PasswordHasher.Hash(realPassword),
            IsActive = true
        };

        var dto = new UserLoginDto { Email = "user@kursutv.com", Password = "WrongPassword999" };
        _userRepoMock
            .Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _manager.LoginAsync(dto);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("E-posta veya şifre hatalı");
    }

    [Fact]
    public async Task LoginAsync_WhenUserIsSuspended_ShouldReturnFailure()
    {
        // Arrange
        var password = "CorrectPassword123!";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "banned@kursutv.com",
            PasswordHash = PasswordHasher.Hash(password),
            IsActive = false
        };

        var dto = new UserLoginDto { Email = "banned@kursutv.com", Password = password };
        _userRepoMock
            .Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _manager.LoginAsync(dto);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("askıya alınmıştır");
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var password = "CorrectPassword123!";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "editor@kursutv.com",
            FullName = "Editör Can",
            PasswordHash = PasswordHasher.Hash(password),
            IsActive = true,
            Role = UserRole.Editor
        };

        var dto = new UserLoginDto { Email = "editor@kursutv.com", Password = password };
        _userRepoMock
            .Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _manager.LoginAsync(dto);

        // Assert
        result.Success.Should().BeTrue();
        result.User.Should().NotBeNull();
        result.User!.Email.Should().Be("editor@kursutv.com");
    }

    [Fact]
    public async Task SetUserStatusAsync_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var action = () => _manager.SetUserStatusAsync(userId, true);
        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task SetUserStatusAsync_WhenFound_ShouldUpdateAndSave()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, IsActive = true };

        _userRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _manager.SetUserStatusAsync(userId, false);

        // Assert
        user.IsActive.Should().BeFalse();
        _userRepoMock.Verify(r => r.Update(user), Times.Once);
        _userRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
