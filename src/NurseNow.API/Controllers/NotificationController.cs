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
[Authorize]
public sealed class NotificationController(NotificationService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        return OperationResultMapper.Map(await service.GetNotifications());
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        return OperationResultMapper.Map(await service.MarkAsRead(id));
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        return OperationResultMapper.Map(await service.MarkAllAsRead());
    }
}
