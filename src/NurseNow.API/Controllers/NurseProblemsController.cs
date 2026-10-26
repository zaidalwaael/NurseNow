using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseNow.API.Common;
using NurseNow.Application.Features;
using NurseNow.DTOs;
using NurseNow.DTOs.Patient;
using NurseNow.DTOs.Nurse;

namespace NurseNow.Controllers;
[ApiController]
[Route("api/nurse/problems")]
[Authorize(Roles = "Nurse")]
public sealed class NurseProblemsController(NurseProblemsService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SubmitProblem([FromBody] SubmitProblemDto dto)
    {
        return OperationResultMapper.Map(await service.SubmitProblem(dto));
    }

    [HttpGet("my-reports")]
    public async Task<IActionResult> GetMyReports()
    {
        return OperationResultMapper.Map(await service.GetMyReports());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProblemDetails(int id)
    {
        return OperationResultMapper.Map(await service.GetProblemDetails(id));
    }
}
