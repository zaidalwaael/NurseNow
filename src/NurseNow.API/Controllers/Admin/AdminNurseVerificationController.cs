using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseNow.API.Common;
using NurseNow.Application.Features.Admin;
using NurseNow.DTOs;
using NurseNow.DTOs.Patient;
using NurseNow.DTOs.Nurse;

namespace NurseNow.Controllers.Admin;
[ApiController]
[Route("api/admin/nurse-verification")]
[Authorize(Roles = "Administrator")]
public sealed class AdminNurseVerificationController(AdminNurseVerificationService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPendingNurses([FromQuery] string? search)
    {
        return OperationResultMapper.Map(await service.GetPendingNurses(search));
    }

    [HttpGet("{nurseProfileId}")]
    public async Task<IActionResult> GetNurseDetails(int nurseProfileId)
    {
        return OperationResultMapper.Map(await service.GetNurseDetails(nurseProfileId));
    }

    [HttpPut("{nurseProfileId}/approve")]
    public async Task<IActionResult> ApproveNurse(int nurseProfileId)
    {
        return OperationResultMapper.Map(await service.ApproveNurse(nurseProfileId));
    }

    [HttpPut("{nurseProfileId}/reject")]
    public async Task<IActionResult> RejectNurse(int nurseProfileId, RejectNurseDto dto)
    {
        return OperationResultMapper.Map(await service.RejectNurse(nurseProfileId, dto));
    }
}
