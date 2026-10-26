using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseNow.API.Common;
using NurseNow.Application.Features.Admin;
using NurseNow.DTOs;
using NurseNow.DTOs.Patient;
using NurseNow.DTOs.Nurse;

namespace NurseNow.Controllers.Admin;
[ApiController]
[Route("api/admin/complaints")]
[Authorize(Roles = "Administrator")]
public sealed class AdminComplaintsController(AdminComplaintsService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetComplaints(string? search = "", string? status = "")
    {
        return OperationResultMapper.Map(await service.GetComplaints(search, status));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetComplaintDetails(int id)
    {
        return OperationResultMapper.Map(await service.GetComplaintDetails(id));
    }

    [HttpPut("{id}/response")]
    public async Task<IActionResult> SendResponse(int id, ComplaintResponseDto dto)
    {
        return OperationResultMapper.Map(await service.SendResponse(id, dto));
    }

    [HttpPut("{id}/resolve")]
    public async Task<IActionResult> MarkAsResolved(int id)
    {
        return OperationResultMapper.Map(await service.MarkAsResolved(id));
    }
}
