namespace EchoHub.Core.Models;

/// <summary>
/// Structured error entry carried inside a <see cref="Response"/>.
/// </summary>
/// <param name="Code">Machine-readable error code (e.g. <c>HostAlreadyClaimed</c>, <c>NotFound</c>).</param>
/// <param name="Message">Human-readable message, when useful.</param>
/// <param name="Data">Optional structured context (e.g. conflicting hosts, validation details).</param>
public record ErrorDetail(string Code, string? Message = null, object? Data = null);

/// <summary>
/// Standard envelope for request/response traffic on both HTTP endpoints and hub invocations.
/// Broadcasts/events do not use this envelope.
/// </summary>
public class Response
{
    /// <summary>Current protocol version stamped onto every envelope.</summary>
    public const string CurrentVersion = "1.0";

    /// <summary>Whether the call succeeded.</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Structured error entries. Null on success.</summary>
    public ErrorDetail[]? Errors { get; init; }

    /// <summary>Protocol version of the envelope itself, for client compatibility checks.</summary>
    public string? Version { get; init; } = CurrentVersion;
}

/// <summary>
/// Typed envelope. <see cref="Data"/> is populated on success, null on failure.
/// </summary>
public class Response<T> : Response
{
    /// <summary>The response payload when <see cref="Response.IsSuccess"/> is true.</summary>
    public T? Data { get; init; }
}

/// <summary>
/// Factory for <see cref="Response"/> / <see cref="Response{T}"/>.
/// Lives on a separate type to avoid clashing with <c>ControllerBase.Response</c> inside MVC controllers.
/// </summary>
public static class Respond
{
    /// <summary>Produce a success envelope with no data.</summary>
    public static Response Ok() => new() { IsSuccess = true };

    /// <summary>Produce a success envelope carrying <paramref name="data"/>.</summary>
    public static Response<T> Ok<T>(T data) => new() { IsSuccess = true, Data = data };

    /// <summary>Produce a failure envelope with a single error.</summary>
    public static Response Fail(string code, string? message = null, object? data = null) =>
        new() { IsSuccess = false, Errors = [new ErrorDetail(code, message, data)] };

    /// <summary>Produce a failure envelope with explicit error details.</summary>
    public static Response Fail(params ErrorDetail[] errors) =>
        new() { IsSuccess = false, Errors = errors };

    /// <summary>Produce a failure envelope typed to a payload (<see cref="Response{T}.Data"/> stays null).</summary>
    public static Response<T> Fail<T>(string code, string? message = null, object? data = null) =>
        new() { IsSuccess = false, Errors = [new ErrorDetail(code, message, data)] };
}
