export interface MediaDto {
  id: string;
  originalUrl: string;
  thumbnailUrl?: string;
  listUrl?: string;
  altText?: string;
  mimeType: string;
  fileSizeBytes: number;
  newsId?: string;
  uploadedAt: string;
  uploadedByUserName: string;
}
