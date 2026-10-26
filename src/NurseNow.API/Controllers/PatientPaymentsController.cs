using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseNow.API.Common;
using NurseNow.Application.Features;
using NurseNow.DTOs;
using NurseNow.DTOs.Patient;
using NurseNow.DTOs.Nurse;

namespace NurseNow.Controllers;
[ApiController]
[Route("api/patient/payments")]
[Authorize(Roles = "Patient")]
public sealed class PatientPaymentsController(PatientPaymentsService service) : ControllerBase
{
    [HttpGet("history")]
    public async Task<IActionResult> GetPaymentHistory()
    {
        return OperationResultMapper.Map(await service.GetPaymentHistory());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPaymentDetails(int id)
    {
        return OperationResultMapper.Map(await service.GetPaymentDetails(id));
    }
}
