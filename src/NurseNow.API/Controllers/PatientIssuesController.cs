using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseNow.API.Common;
using NurseNow.Application.Features;
using NurseNow.DTOs;
using NurseNow.DTOs.Patient;
using NurseNow.DTOs.Nurse;

namespace NurseNow.Controllers;
[ApiController]
[Route("api/patient/issues")]
[Authorize(Roles = "Patient")]
public sealed class PatientIssuesController(PatientIssuesService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SubmitIssue([FromBody] SubmitPatientIssueDto dto)
    {
        return OperationResultMapper.Map(await service.SubmitIssue(dto));
    }

    [HttpGet("my-reports")]
    public async Task<IActionResult> GetMyReports()
    {
        return OperationResultMapper.Map(await service.GetMyReports());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetIssueDetails(int id)
    {
        return OperationResultMapper.Map(await service.GetIssueDetails(id));
    }
}
