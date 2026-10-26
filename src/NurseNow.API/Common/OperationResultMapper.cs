using Microsoft.AspNetCore.Mvc;
using NurseNow.Application.Common;

namespace NurseNow.API.Common;
public static class OperationResultMapper
{
    public static IActionResult Map(OperationResult result) => result.Kind switch
    {
        ResultKind.Success => new OkObjectResult(result.Value),
        ResultKind.Invalid => new BadRequestObjectResult(result.Value),
        ResultKind.NotFound => new NotFoundObjectResult(result.Value),
        ResultKind.Unauthenticated => result.Value is null ? new UnauthorizedResult() : new UnauthorizedObjectResult(result.Value),
        ResultKind.Forbidden => new ForbidResult(),
        ResultKind.Conflict => new ConflictObjectResult(result.Value),
        _ => throw new ArgumentOutOfRangeException(nameof(result))};
}
