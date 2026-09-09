using KursuTV.Business.DTOs;
using KursuTV.Business.Interfaces;
using KursuTV.Data.Context;
using KursuTV.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace KursuTV.Business.Services;

public class DashboardManager : IDashboardService
{
    private readonly AppDbContext _context;
    private readonly INewsService _newsService;

    public DashboardManager(AppDbContext context, INewsService newsService)
    {
        _context = context;
        _newsService = newsService;
    }

    public async Task<DashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var totalPublished = await _context.News.CountAsync(n => n.Status == NewsStatus.Published, cancellationToken);
        var totalDraft = await _context.News.CountAsync(n => n.Status == NewsStatus.Draft, cancellationToken);
        var totalViews = await _context.News.SumAsync(n => (long)n.ViewCount, cancellationToken);
        var totalPendingComments = await _context.Comments.CountAsync(c => c.Status == CommentStatus.Pending, cancellationToken);
        var totalCategories = await _context.Categories.CountAsync(c => c.IsActive, cancellationToken);
        var totalAuthors = await _context.Users.CountAsync(u => u.IsActive && u.Role != UserRole.SuperAdmin, cancellationToken);

        var recentNewsResult = await _newsService.GetPublishedNewsPagedAsync(1, 5, cancellationToken: cancellationToken);
        var topViewed = await _newsService.GetMostViewedNewsAsync(5, cancellationToken);

        return new DashboardStatsDto(
            totalPublished,
            totalDraft,
            totalViews,
            totalPendingComments,
            totalCategories,
            totalAuthors,
            recentNewsResult.Items,
            topViewed
        );
    }
}
