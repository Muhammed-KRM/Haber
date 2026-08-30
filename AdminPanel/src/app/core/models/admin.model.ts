export interface AuditLog {
  id: string;
  userId?: string;
  actionType: 'CREATE' | 'UPDATE' | 'DELETE' | 'TOGGLE_STOCK' | 'UPLOAD' | string;
  entityName: 'Product' | 'Category' | 'Media' | string;
  entityId?: string;
  entityTitle?: string;
  oldValuesJson?: string;
  newValuesJson?: string;
  description: string;
  ipAddress?: string;
  endpoint?: string;
  timestamp: string;
}

export interface PaginatedAuditLogResponse {
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  data: AuditLog[];
}

export interface AdminCategory {
  id: string;
  name: string;
  imageUrl?: string;
  displayOrder: number;
  isActive: boolean;
  productCount?: number;
}

export interface AdminCategoryDto {
  name: string;
  imageUrl?: string;
  displayOrder: number;
  isActive: boolean;
}

export interface AdminProductCategoryRef {
  id: string;
  name: string;
}

export interface AdminProduct {
  id: string;
  name: string;
  description: string;
  price: number;
  imageUrl?: string;
  calories?: number;
  preparationTime?: string;
  isAvailable: boolean;
  createdAt: string;
  categories: AdminProductCategoryRef[];
}

export interface AdminProductDetail {
  id: string;
  name: string;
  description: string;
  price: number;
  imageUrl?: string;
  calories?: number;
  preparationTime?: string;
  isAvailable: boolean;
  categoryIds: string[];
  categories: AdminProductCategoryRef[];
}

export interface AdminProductDto {
  name: string;
  description: string;
  price: number;
  imageUrl?: string;
  calories?: number;
  preparationTime?: string;
  isAvailable: boolean;
  categoryIds: string[];
}
