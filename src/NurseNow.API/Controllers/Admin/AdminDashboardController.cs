using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseNow.API.Common;
using NurseNow.Application.Features.Admin;
using NurseNow.DTOs;
using NurseNow.DTOs.Patient;
using NurseNow.DTOs.Nurse;

namespace NurseNow.Controllers.Admin;
[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Administrator")]
public sealed class AdminDashboardController(AdminDashboardService service) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<IActionResult> GetDashboardOverview()
    {
        return OperationResultMapper.Map(await service.GetDashboardOverview());
    }
}
