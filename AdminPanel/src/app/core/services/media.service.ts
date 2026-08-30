import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MediaDto } from '../models/media.model';
import { PagedResult } from '../models/news.model';

@Injectable({
  providedIn: 'root'
})
export class MediaService {
  private apiUrl = 'http://localhost:5000/api/media';

  constructor(private http: HttpClient) {}

  upload(file: File, altText?: string, newsId?: string): Observable<MediaDto> {
    const formData = new FormData();
    formData.append('file', file);
    if (altText) formData.append('altText', altText);
    if (newsId) formData.append('newsId', newsId);

    return this.http.post<MediaDto>(`${this.apiUrl}/upload`, formData);
  }

  getPagedMedia(page = 1, pageSize = 20, search?: string): Observable<PagedResult<MediaDto>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search) params = params.set('search', search);

    return this.http.get<PagedResult<MediaDto>>(this.apiUrl, { params });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
