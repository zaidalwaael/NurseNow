using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseNow.API.Common;
using NurseNow.Application.Features.Admin;
using NurseNow.DTOs;
using NurseNow.DTOs.Patient;
using NurseNow.DTOs.Nurse;

namespace NurseNow.Controllers.Admin;
[Route("api/admin/service-requests")]
[ApiController]
[Authorize(Roles = "Administrator")]
public sealed class ServiceRequestsController(ServiceRequestsService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(string? search = "", string? status = "")
    {
        return OperationResultMapper.Map(await service.GetAll(search, status));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetails(int id)
    {
        return OperationResultMapper.Map(await service.GetDetails(id));
    }
}
