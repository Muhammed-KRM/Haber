using FluentAssertions;
using KursuTV.Business.DTOs;
using KursuTV.Business.Exceptions;
using KursuTV.Business.Interfaces;
using KursuTV.Business.Services;
using KursuTV.Data.Entities;
using KursuTV.Data.Enums;
using KursuTV.Data.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace KursuTV.UnitTests.Services;

public class CommentManagerTests
{
    private readonly Mock<ICommentRepository> _commentRepoMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();
    private readonly Mock<ILogger<CommentManager>> _loggerMock = new();
    private readonly CommentManager _manager;

    public CommentManagerTests()
    {
        _manager = new CommentManager(
            _commentRepoMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task AddCommentAsync_ShouldCreateCommentWithPendingStatus()
    {
        // Arrange
        var newsId = Guid.NewGuid();
        var dto = new CreateCommentDto(
            NewsId: newsId,
            Content: "Tebrikler güzel haber.",
            GuestName: "Ahmet",
            ParentCommentId: null
        );

        _commentRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Comment>()))
            .ReturnsAsync((Comment c) => c);

        _commentRepoMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var commentId = await _manager.AddCommentAsync(dto, null);

        // Assert
        commentId.Should().NotBeEmpty();
        _commentRepoMock.Verify(r => r.AddAsync(It.Is<Comment>(c =>
            c.NewsId == newsId &&
            c.Status == CommentStatus.Pending &&
            c.GuestName == "Ahmet")), Times.Once);
        _commentRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetPendingCommentsPagedAsync_ShouldReturnPagedResultsWithTotalPages()
    {
        // Arrange
        var comments = new List<Comment>
        {
            new() { Id = Guid.NewGuid(), Content = "Onay bekleyen yorum", Status = CommentStatus.Pending, CreatedAt = DateTime.UtcNow }
        };

        _commentRepoMock
            .Setup(r => r.GetPendingCommentsPagedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((comments, 25)); // 25 items -> 3 pages with size 10

        // Act
        var result = await _manager.GetPendingCommentsPagedAsync(1, 10);

        // Assert
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(3);
        result.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task ModerateCommentAsync_ShouldCallRepositoryWithStatus()
    {
        // Arrange
        var commentId = Guid.NewGuid();

        // Act
        await _manager.ModerateCommentAsync(commentId, CommentStatus.Approved);

        // Assert
        _commentRepoMock.Verify(r => r.UpdateCommentStatusAsync(commentId, CommentStatus.Approved, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCommentAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        _commentRepoMock
            .Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Comment?)null);

        // Act & Assert
        var action = () => _manager.DeleteCommentAsync(commentId);
        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteCommentAsync_WhenFound_ShouldDeleteAndSave()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var comment = new Comment { Id = commentId, Content = "Silinecek" };

        _commentRepoMock
            .Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);

        _commentRepoMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _manager.DeleteCommentAsync(commentId);

        // Assert
        _commentRepoMock.Verify(r => r.Delete(comment), Times.Once);
        _commentRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
