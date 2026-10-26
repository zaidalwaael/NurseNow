using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseNow.API.Common;
using NurseNow.Application.Features;
using NurseNow.DTOs;
using NurseNow.DTOs.Patient;
using NurseNow.DTOs.Nurse;

namespace NurseNow.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public sealed class AdminController(AdminService service) : ControllerBase
{
    [HttpGet("nurse-details/{userId}")]
    public async Task<IActionResult> GetNurseDetails(string userId)
    {
        return OperationResultMapper.Map(await service.GetNurseDetails(userId));
    }

    [HttpPut("verify-nurse/{userId}")]
    public async Task<IActionResult> UpdateNurseStatus(string userId, [FromBody] UpdateNurseStatusDto model)
    {
        return OperationResultMapper.Map(await service.UpdateNurseStatus(userId, model));
    }

    [HttpGet("pending-nurses")]
    public async Task<IActionResult> GetPendingNurses()
    {
        return OperationResultMapper.Map(await service.GetPendingNurses());
    }
}
