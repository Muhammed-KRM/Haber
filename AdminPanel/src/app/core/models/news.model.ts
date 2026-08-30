export enum NewsStatus {
  Draft = 0,
  PendingReview = 1,
  Published = 2,
  Archived = 3
}

export enum NewsType {
  Article = 0,
  Gallery = 1,
  Video = 2,
  Breaking = 3,
  Column = 4
}

export interface CategoryBriefDto {
  id: number;
  name: string;
  slug: string;
}

export interface TagBriefDto {
  id: number;
  name: string;
  slug: string;
}

export interface NewsListDto {
  id: string;
  title: string;
  slug: string;
  spot?: string;
  coverImageUrl?: string;
  coverImageAlt?: string;
  status: NewsStatus;
  type: NewsType;
  isBreaking: boolean;
  headlineOrder: number;
  viewCount: number;
  publishedAt?: string;
  createdAt: string;
  authorName: string;
  categories: CategoryBriefDto[];
}

export interface NewsDetailDto {
  id: string;
  title: string;
  slug: string;
  spot?: string;
  content: string;
  coverImageUrl?: string;
  coverImageAlt?: string;
  metaTitle?: string;
  metaDescription?: string;
  status: NewsStatus;
  type: NewsType;
  isBreaking: boolean;
  headlineOrder: number;
  viewCount: number;
  publishedAt?: string;
  createdAt: string;
  authorId: string;
  authorName: string;
  authorProfileImageUrl?: string;
  authorBio?: string;
  categories: CategoryBriefDto[];
  tags: TagBriefDto[];
}

export interface NewsCreateDto {
  title: string;
  slug?: string;
  spot?: string;
  content: string;
  coverImageUrl?: string;
  coverImageAlt?: string;
  metaTitle?: string;
  metaDescription?: string;
  status: NewsStatus;
  type: NewsType;
  isBreaking: boolean;
  headlineOrder: number;
  scheduledPublishAt?: string;
  categoryIds: number[];
  tagNames: string[];
}

export interface NewsUpdateDto extends NewsCreateDto {
  id: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
