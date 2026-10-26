namespace NurseNow.Application.Common;
public enum ResultKind
{
    Success,
    Invalid,
    NotFound,
    Unauthenticated,
    Forbidden,
    Conflict
}

public sealed record OperationResult(ResultKind Kind, object? Value = null);
// Application outcomes are mapped to HTTP exclusively in API.
public abstract class ApplicationService
{
    protected static OperationResult Ok(object? value = null) => new(ResultKind.Success, value);
    protected static OperationResult BadRequest(object? value = null) => new(ResultKind.Invalid, value);
    protected static OperationResult NotFound(object? value = null) => new(ResultKind.NotFound, value);
    protected static OperationResult Unauthorized(object? value = null) => new(ResultKind.Unauthenticated, value);
    protected static OperationResult Forbid() => new(ResultKind.Forbidden);
}
