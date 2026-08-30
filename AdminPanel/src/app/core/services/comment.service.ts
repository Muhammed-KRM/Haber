import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CommentDto, CommentModerateDto, CommentStatus } from '../models/comment.model';
import { PagedResult } from '../models/news.model';

@Injectable({
  providedIn: 'root'
})
export class CommentService {
  private apiUrl = 'http://localhost:5000/api/comments';

  constructor(private http: HttpClient) {}

  getPending(page = 1, pageSize = 20): Observable<PagedResult<CommentDto>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResult<CommentDto>>(`${this.apiUrl}/admin/pending`, { params });
  }

  moderate(commentId: string, status: CommentStatus): Observable<void> {
    const body: CommentModerateDto = { commentId, status };
    return this.http.post<void>(`${this.apiUrl}/admin/moderate`, body);
  }

  delete(commentId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/admin/${commentId}`);
  }
}
