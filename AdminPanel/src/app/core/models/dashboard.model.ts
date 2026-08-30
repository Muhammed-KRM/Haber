import { NewsListDto } from './news.model';

export interface DashboardStatsDto {
  totalPublishedNews: number;
  totalDraftNews: number;
  totalViews: number;
  totalPendingComments: number;
  totalCategories: number;
  totalAuthors: number;
  recentNews: NewsListDto[];
  topViewedNews: NewsListDto[];
}
