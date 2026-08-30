export interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  imageUrl?: string;
  calories?: number;
  preparationTime?: string;
  isAvailable?: boolean;
}

export interface Category {
  id: string;
  name: string;
  imageUrl?: string;
  displayOrder: number;
  products: Product[];
}

export interface MenuResponse {
  categories: Category[];
}
