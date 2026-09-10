using System;
using KursuTV.Data.Entities;

namespace KursuTV.Business.DTOs;

public record LogFilterRequest
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
    
    // Opsiyonel filtreler
    public string? Method { get; init; }
    public int? StatusCode { get; init; }
    public string? Severity { get; init; } // Error, Critical vs.
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public record EndpointLogResponseDto
{
    public long Id { get; init; }
    public string? TraceId { get; init; }
    public string Method { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public string? Query { get; init; }
    public string? RequestBody { get; init; }
    public string? ResponseBody { get; init; }
    public int StatusCode { get; init; }
    public Guid? UserId { get; init; }
    public string? UserEmail { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public int DurationMs { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record FunctionLogResponseDto
{
    public long Id { get; init; }
    public string? ErrorCode { get; init; }
    public string? ClassName { get; init; }
    public string? MethodName { get; init; }
    public string? FilePath { get; init; }
    public int LineNumber { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public string? StackTrace { get; init; }
    public string? InputType { get; init; }
    public string? InputValue { get; init; }
    public Guid? UserId { get; init; }
    public string? TraceId { get; init; }
    public string? Severity { get; init; }
    public DateTime CreatedAt { get; init; }
}
