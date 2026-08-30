export enum CommentStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
  Spam = 3
}

export interface CommentDto {
  id: string;
  content: string;
  newsId: string;
  userId?: string;
  authorName: string;
  userProfileImageUrl?: string;
  status: CommentStatus;
  createdAt: string;
  replies?: CommentDto[];
}

export interface CommentModerateDto {
  commentId: string;
  status: CommentStatus;
}
