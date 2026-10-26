using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseNow.API.Common;
using NurseNow.Application.Features;
using NurseNow.DTOs;
using NurseNow.DTOs.Patient;
using NurseNow.DTOs.Nurse;

namespace NurseNow.Controllers;
[ApiController]
[Route("api/nurse/payments")]
[Authorize(Roles = "Nurse")]
public sealed class NursePaymentsController(NursePaymentsService service) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetPaymentSummary()
    {
        return OperationResultMapper.Map(await service.GetPaymentSummary());
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetPaymentHistory(string? tab = "all")
    {
        return OperationResultMapper.Map(await service.GetPaymentHistory(tab));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPaymentDetails(int id)
    {
        return OperationResultMapper.Map(await service.GetPaymentDetails(id));
    }
}
