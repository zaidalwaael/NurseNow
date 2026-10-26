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
public sealed class TestController(TestService service) : ControllerBase
{
    [HttpGet("public")]
    public IActionResult Public()
    {
        return OperationResultMapper.Map(service.Public());
    }

    [Authorize]
    [HttpGet("protected")]
    public IActionResult Protected()
    {
        return OperationResultMapper.Map(service.Protected());
    }

    [Authorize(Roles = "Administrator")]
    [HttpGet("admin-only")]
    public IActionResult AdminOnly()
    {
        return OperationResultMapper.Map(service.AdminOnly());
    }

    [Authorize(Roles = "Nurse")]
    [HttpGet("nurse-only")]
    public IActionResult NurseOnly()
    {
        return OperationResultMapper.Map(service.NurseOnly());
    }
}
