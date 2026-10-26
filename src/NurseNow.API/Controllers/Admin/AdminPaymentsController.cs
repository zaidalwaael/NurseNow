using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseNow.API.Common;
using NurseNow.Application.Features.Admin;
using NurseNow.DTOs;
using NurseNow.DTOs.Patient;
using NurseNow.DTOs.Nurse;

namespace NurseNow.Controllers.Admin;
[ApiController]
[Route("api/admin/payments")]
[Authorize(Roles = "Administrator")]
public sealed class AdminPaymentsController(AdminPaymentsService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllTransactions()
    {
        return OperationResultMapper.Map(await service.GetAllTransactions());
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        return OperationResultMapper.Map(await service.GetStats());
    }
}
