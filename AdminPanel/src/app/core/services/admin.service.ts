import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  AdminCategory,
  AdminCategoryDto,
  AdminProduct,
  AdminProductDetail,
  AdminProductDto,
  PaginatedAuditLogResponse
} from '../models/admin.model';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private http = inject(HttpClient);
  private baseUrl = 'https://localhost:7000/api/admin';

  // ==========================================
  // Kategori API
  // ==========================================
  getCategories(): Observable<AdminCategory[]> {
    return this.http.get<AdminCategory[]>(`${this.baseUrl}/AdminCategories`);
  }

  getCategoryById(id: string): Observable<AdminCategory> {
    return this.http.get<AdminCategory>(`${this.baseUrl}/AdminCategories/${id}`);
  }

  createCategory(dto: AdminCategoryDto): Observable<AdminCategory> {
    return this.http.post<AdminCategory>(`${this.baseUrl}/AdminCategories`, dto);
  }

  updateCategory(id: string, dto: AdminCategoryDto): Observable<AdminCategory> {
    return this.http.put<AdminCategory>(`${this.baseUrl}/AdminCategories/${id}`, dto);
  }

  deleteCategory(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/AdminCategories/${id}`);
  }

  // ==========================================
  // Ürün API (Çoklu Kategori Destekli)
  // ==========================================
  getProducts(): Observable<AdminProduct[]> {
    return this.http.get<AdminProduct[]>(`${this.baseUrl}/AdminProducts`);
  }

  getProductById(id: string): Observable<AdminProductDetail> {
    return this.http.get<AdminProductDetail>(`${this.baseUrl}/AdminProducts/${id}`);
  }

  createProduct(dto: AdminProductDto): Observable<AdminProduct> {
    return this.http.post<AdminProduct>(`${this.baseUrl}/AdminProducts`, dto);
  }

  updateProduct(id: string, dto: AdminProductDto): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/AdminProducts/${id}`, dto);
  }

  toggleProductStock(id: string): Observable<{ id: string; name: string; isAvailable: boolean }> {
    return this.http.patch<{ id: string; name: string; isAvailable: boolean }>(
      `${this.baseUrl}/AdminProducts/${id}/toggle-availability`,
      {}
    );
  }

  deleteProduct(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/AdminProducts/${id}`);
  }

  // ==========================================
  // Medya (Görsel Yükleme) API
  // ==========================================
  uploadImage(file: File): Observable<{ fileName: string; imageUrl: string; size: number }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ fileName: string; imageUrl: string; size: number }>(
      `${this.baseUrl}/Media/upload`,
      formData
    );
  }

  // ==========================================
  // Audit Logs (Değişiklik Geçmişi) API
  // ==========================================
  getAuditLogs(
    page: number = 1,
    pageSize: number = 20,
    actionType?: string,
    entityName?: string,
    search?: string
  ): Observable<PaginatedAuditLogResponse> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (actionType) params = params.set('actionType', actionType);
    if (entityName) params = params.set('entityName', entityName);
    if (search) params = params.set('search', search);

    return this.http.get<PaginatedAuditLogResponse>(`${this.baseUrl}/AdminAuditLogs`, { params });
  }
}
