export interface LogFilterRequest {
  pageNumber: number;
  pageSize: number;
  method?: string;
  statusCode?: number;
  severity?: string;
  startDate?: string;
  endDate?: string;
}

export interface EndpointLogResponseDto {
  id: string;
  traceId?: string;
  method: string;
  path: string;
  query?: string;
  requestBody?: string;
  responseBody?: string;
  statusCode: number;
  userId?: string;
  userEmail?: string;
  ipAddress?: string;
  userAgent?: string;
  durationMs: number;
  createdAt: string;
}

export interface FunctionLogResponseDto {
  id: string;
  errorCode?: string;
  className?: string;
  methodName?: string;
  filePath?: string;
  lineNumber: number;
  errorMessage: string;
  stackTrace?: string;
  inputType?: string;
  inputValue?: string;
  userId?: string;
  traceId?: string;
  severity?: string;
  createdAt: string;
}

export interface PagedResultDto<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
