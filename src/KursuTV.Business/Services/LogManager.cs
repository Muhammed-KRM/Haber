using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using KursuTV.Business.DTOs;
using KursuTV.Business.Interfaces;
using KursuTV.Data.Context;
using KursuTV.Data.Entities;

namespace KursuTV.Business.Services;

public class LogManager : ILogService
{
    private readonly AppDbContext _db;

    public LogManager(AppDbContext db)
    {
        _db = db;
    }

    public async Task LogEndpointAsync(EndpointLogEntry entry)
    {
        try
        {
            var log = new EndpointLog
            {
                TraceId    = entry.TraceId,
                Method     = entry.Method,
                Path       = entry.Path,
                Query      = entry.Query,
                RequestBody  = MaskSensitiveData(entry.RequestBody),
                ResponseBody = entry.ResponseBody,
                StatusCode = entry.StatusCode,
                UserId     = entry.UserId,
                UserEmail  = entry.UserEmail,
                IpAddress  = entry.IpAddress,
                UserAgent  = entry.UserAgent,
                DurationMs = entry.DurationMs,
                CreatedAt  = DateTime.UtcNow
            };

            _db.EndpointLogs.Add(log);
            await _db.SaveChangesAsync();
        }
        catch
        {
            // Log yazma hatasÄ± uygulamayÄ± Ã§Ã¶kertmemeli
        }
    }

    public async Task LogFunctionErrorAsync(
        string errorCode,
        Exception ex,
        object? inputData = null,
        Guid? userId = null,
        string? traceId = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        try
        {
            var className = Path.GetFileNameWithoutExtension(filePath);

            string? inputValue = null;
            if (inputData is not null)
            {
                try { inputValue = JsonSerializer.Serialize(inputData); }
                catch { inputValue = inputData.ToString(); }
            }

            var log = new FunctionLog
            {
                ErrorCode    = errorCode,
                ClassName    = className,
                MethodName   = memberName,
                FilePath     = filePath,
                LineNumber   = lineNumber,
                ErrorMessage = ex.Message,
                StackTrace   = ex.StackTrace,
                InputType    = inputData?.GetType().Name,
                InputValue   = inputValue,
                UserId       = userId,
                TraceId      = traceId,
                Severity     = ex is OutOfMemoryException or StackOverflowException ? "Critical" : "Error",
                CreatedAt    = DateTime.UtcNow
            };

            _db.FunctionLogs.Add(log);
            await _db.SaveChangesAsync();
        }
        catch
        {
            // Log yazma hatasÄ± uygulamayÄ± Ã§Ã¶kertmemeli
        }
    }

    public async Task<PagedResultDto<EndpointLogResponseDto>> GetEndpointLogsAsync(LogFilterRequest request, CancellationToken cancellationToken = default)
    {
        var query = _db.EndpointLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(request.Method))
            query = query.Where(x => x.Method == request.Method);
            
        if (request.StatusCode.HasValue)
            query = query.Where(x => x.StatusCode == request.StatusCode.Value);
            
        if (request.StartDate.HasValue)
            query = query.Where(x => x.CreatedAt >= request.StartDate.Value);
            
        if (request.EndDate.HasValue)
            query = query.Where(x => x.CreatedAt <= request.EndDate.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new EndpointLogResponseDto
            {
                Id = x.Id,
                TraceId = x.TraceId,
                Method = x.Method,
                Path = x.Path,
                Query = x.Query,
                RequestBody = x.RequestBody,
                ResponseBody = x.ResponseBody,
                StatusCode = x.StatusCode,
                UserId = x.UserId,
                UserEmail = x.UserEmail,
                IpAddress = x.IpAddress,
                UserAgent = x.UserAgent,
                DurationMs = x.DurationMs,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        return new PagedResultDto<EndpointLogResponseDto>(items, totalCount, request.PageNumber, request.PageSize, totalPages);
    }

    public async Task<PagedResultDto<FunctionLogResponseDto>> GetFunctionLogsAsync(LogFilterRequest request, CancellationToken cancellationToken = default)
    {
        var query = _db.FunctionLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(request.Severity))
            query = query.Where(x => x.Severity == request.Severity);
            
        if (request.StartDate.HasValue)
            query = query.Where(x => x.CreatedAt >= request.StartDate.Value);
            
        if (request.EndDate.HasValue)
            query = query.Where(x => x.CreatedAt <= request.EndDate.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new FunctionLogResponseDto
            {
                Id = x.Id,
                ErrorCode = x.ErrorCode,
                ClassName = x.ClassName,
                MethodName = x.MethodName,
                FilePath = x.FilePath,
                LineNumber = x.LineNumber,
                ErrorMessage = x.ErrorMessage,
                StackTrace = x.StackTrace,
                InputType = x.InputType,
                InputValue = x.InputValue,
                UserId = x.UserId,
                TraceId = x.TraceId,
                Severity = x.Severity,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        return new PagedResultDto<FunctionLogResponseDto>(items, totalCount, request.PageNumber, request.PageSize, totalPages);
    }

    private static string? MaskSensitiveData(string? json)
    {
        if (string.IsNullOrEmpty(json)) return json;

        return Regex.Replace(
            json,
            @"""(password|token|refreshToken|aesKey|ibanEncrypted|tcknEncrypted)""\s*:\s*""[^""]*""",
            @"""$1"":""***""",
            RegexOptions.IgnoreCase);
    }
}
