import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { EndpointLogResponseDto, FunctionLogResponseDto, LogFilterRequest, PagedResultDto } from '../models/log.model';

@Injectable({
  providedIn: 'root'
})
export class LogService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5000/api/logs';

  getEndpointLogs(filter: LogFilterRequest): Observable<PagedResultDto<EndpointLogResponseDto>> {
    let params = new HttpParams()
      .set('pageNumber', filter.pageNumber.toString())
      .set('pageSize', filter.pageSize.toString());

    if (filter.method) params = params.set('method', filter.method);
    if (filter.statusCode) params = params.set('statusCode', filter.statusCode.toString());
    if (filter.startDate) params = params.set('startDate', filter.startDate);
    if (filter.endDate) params = params.set('endDate', filter.endDate);

    return this.http.get<PagedResultDto<EndpointLogResponseDto>>(`${this.apiUrl}/endpoints`, { params });
  }

  getFunctionLogs(filter: LogFilterRequest): Observable<PagedResultDto<FunctionLogResponseDto>> {
    let params = new HttpParams()
      .set('pageNumber', filter.pageNumber.toString())
      .set('pageSize', filter.pageSize.toString());

    if (filter.severity) params = params.set('severity', filter.severity);
    if (filter.startDate) params = params.set('startDate', filter.startDate);
    if (filter.endDate) params = params.set('endDate', filter.endDate);

    return this.http.get<PagedResultDto<FunctionLogResponseDto>>(`${this.apiUrl}/functions`, { params });
  }
}
