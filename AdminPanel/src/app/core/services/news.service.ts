import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { NewsCreateDto, NewsDetailDto, NewsListDto, NewsStatus, NewsUpdateDto, PagedResult } from '../models/news.model';

@Injectable({
  providedIn: 'root'
})
export class NewsService {
  private apiUrl = 'http://localhost:5000/api/news';

  constructor(private http: HttpClient) {}

  getAdminNews(params: {
    page?: number;
    pageSize?: number;
    search?: string;
    status?: NewsStatus;
    categoryId?: number;
    authorId?: string;
  }): Observable<PagedResult<NewsListDto>> {
    let httpParams = new HttpParams();
    if (params.page) httpParams = httpParams.set('page', params.page);
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize);
    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.status !== undefined && params.status !== null) httpParams = httpParams.set('status', params.status);
    if (params.categoryId) httpParams = httpParams.set('categoryId', params.categoryId);
    if (params.authorId) httpParams = httpParams.set('authorId', params.authorId);

    return this.http.get<PagedResult<NewsListDto>>(`${this.apiUrl}/admin/list`, { params: httpParams });
  }

  getByIdForEdit(id: string): Observable<NewsDetailDto> {
    return this.http.get<NewsDetailDto>(`${this.apiUrl}/admin/${id}`);
  }

  createNews(dto: NewsCreateDto): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.apiUrl, dto);
  }

  updateNews(dto: NewsUpdateDto): Observable<void> {
    return this.http.put<void>(this.apiUrl, dto);
  }

  autoSaveDraft(dto: NewsUpdateDto): Observable<{ success: boolean; savedAt: string }> {
    return this.http.post<{ success: boolean; savedAt: string }>(`${this.apiUrl}/auto-save`, dto);
  }

  deleteNews(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
