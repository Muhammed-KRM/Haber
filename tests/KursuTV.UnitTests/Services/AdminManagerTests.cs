using FluentAssertions;
using KursuTV.Business.Exceptions;
using KursuTV.Business.Services;
using KursuTV.Data.Context;
using KursuTV.Data.Entities;
using KursuTV.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace KursuTV.UnitTests.Services;

public class AdminManagerTests
{
    private readonly Mock<ILogger<AdminManager>> _loggerMock = new();

    private AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"AdminDb_{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetAllUsersAsync_WithRoleFilter_ShouldReturnOnlyMatchingUsers()
    {
        using var context = CreateInMemoryDbContext();
        context.Users.AddRange(
            new User { Id = Guid.NewGuid(), Email = "admin@kursu.tv", FullName = "Admin User", Role = UserRole.SuperAdmin, PasswordHash = "hash" },
            new User { Id = Guid.NewGuid(), Email = "author@kursu.tv", FullName = "Author User", Role = UserRole.ColumnWriter, PasswordHash = "hash" }
        );
        await context.SaveChangesAsync();

        var manager = new AdminManager(context, _loggerMock.Object);

        // Act
        var result = await manager.GetAllUsersAsync(role: UserRole.SuperAdmin);

        // Assert
        result.Should().HaveCount(1);
        result[0].Email.Should().Be("admin@kursu.tv");
    }

    [Fact]
    public async Task SuspendUserAsync_WhenUserExists_ShouldSetIsActiveFalse()
    {
        using var context = CreateInMemoryDbContext();
        var userId = Guid.NewGuid();
        context.Users.Add(new User
        {
            Id = userId,
            Email = "suspend@kursu.tv",
            FullName = "Suspend User",
            Role = UserRole.Reporter,
            IsActive = true,
            PasswordHash = "hash"
        });
        await context.SaveChangesAsync();

        var manager = new AdminManager(context, _loggerMock.Object);

        // Act
        await manager.SuspendUserAsync(userId, "Spam içeriği");

        // Assert
        var updated = await context.Users.FindAsync(userId);
        updated.Should().NotBeNull();
        updated!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task SuspendUserAsync_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        using var context = CreateInMemoryDbContext();
        var manager = new AdminManager(context, _loggerMock.Object);

        var action = () => manager.SuspendUserAsync(Guid.NewGuid(), "Neden");
        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ActivateUserAsync_ShouldSetIsActiveTrue()
    {
        using var context = CreateInMemoryDbContext();
        var userId = Guid.NewGuid();
        context.Users.Add(new User
        {
            Id = userId,
            Email = "inactive@kursu.tv",
            FullName = "Inactive User",
            Role = UserRole.Reporter,
            IsActive = false,
            PasswordHash = "hash"
        });
        await context.SaveChangesAsync();

        var manager = new AdminManager(context, _loggerMock.Object);

        // Act
        await manager.ActivateUserAsync(userId);

        // Assert
        var updated = await context.Users.FindAsync(userId);
        updated!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateUserRoleAsync_ShouldChangeRole()
    {
        using var context = CreateInMemoryDbContext();
        var userId = Guid.NewGuid();
        context.Users.Add(new User
        {
            Id = userId,
            Email = "reporter@kursu.tv",
            FullName = "Muhabir",
            Role = UserRole.Reporter,
            PasswordHash = "hash"
        });
        await context.SaveChangesAsync();

        var manager = new AdminManager(context, _loggerMock.Object);

        // Act
        await manager.UpdateUserRoleAsync(userId, UserRole.Editor);

        // Assert
        var updated = await context.Users.FindAsync(userId);
        updated!.Role.Should().Be(UserRole.Editor);
    }
}
