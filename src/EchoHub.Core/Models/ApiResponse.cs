namespace EchoHub.Core.Models;

public class ApiResponse
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public string[]? Errors { get; init; }

    public static ApiResponse Ok(string? message = null)
        => new() { Success = true, Message = message };

    public static ApiResponse Fail(string message, string[]? errors = null)
        => new() { Success = false, Message = message, Errors = errors };

    public static ApiResponse<T> Ok<T>(T data, string? message = null)
        => new() { Success = true, Data = data, Message = message };
}

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; init; }
}
