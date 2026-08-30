export interface CategoryDto {
  id: number;
  name: string;
  slug: string;
  description?: string;
  sortOrder: number;
  isActive: boolean;
  parentId?: number;
  children?: CategoryDto[];
}

export interface CreateCategoryDto {
  name: string;
  slug?: string;
  description?: string;
  sortOrder: number;
  isActive: boolean;
  parentId?: number;
}

export interface UpdateCategoryDto extends CreateCategoryDto {
  id: number;
}
