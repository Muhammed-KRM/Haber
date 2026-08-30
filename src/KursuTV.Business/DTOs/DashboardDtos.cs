namespace KursuTV.Business.DTOs;

public record DashboardStatsDto(
    int TotalPublishedNews,
    int TotalDraftNews,
    long TotalViews,
    int TotalPendingComments,
    int TotalCategories,
    int TotalAuthors,
    List<NewsListDto> RecentNews,
    List<NewsListDto> TopViewedNews
);
