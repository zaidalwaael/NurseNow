using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features;
public sealed class TestService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    public TestService(IRequestContext requestContext)
    {
        _requestContext = requestContext;
    }

    public OperationResult Public()
    {
        return Ok("This endpoint is public");
    }

    public OperationResult Protected()
    {
        return Ok("You are authenticated");
    }

    public OperationResult AdminOnly()
    {
        return Ok("You are an Administrator");
    }

    public OperationResult NurseOnly()
    {
        return Ok("You are a Nurse");
    }
}
