using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseNow.API.Common;
using NurseNow.Application.Features.Admin;
using NurseNow.DTOs;
using NurseNow.DTOs.Patient;
using NurseNow.DTOs.Nurse;

namespace NurseNow.Controllers.Admin;
[ApiController]
[Route("api/admin/notifications")]
[Authorize(Roles = "Administrator")]
public sealed class AdminNotificationsController(AdminNotificationsService service) : ControllerBase
{
    [HttpGet("audience-counts")]
    public async Task<IActionResult> GetAudienceCounts()
    {
        return OperationResultMapper.Map(await service.GetAudienceCounts());
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendNotification(SendAdminNotificationDto dto)
    {
        return OperationResultMapper.Map(await service.SendNotification(dto));
    }
}
