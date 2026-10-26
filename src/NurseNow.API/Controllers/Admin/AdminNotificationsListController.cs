using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseNow.API.Common;
using NurseNow.Application.Features.Admin;
using NurseNow.DTOs;
using NurseNow.DTOs.Patient;
using NurseNow.DTOs.Nurse;

namespace NurseNow.Controllers.Admin;
[ApiController]
[Route("api/admin/notifications-list")]
[Authorize(Roles = "Administrator")]
public sealed class AdminNotificationsListController(AdminNotificationsListService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetNotifications(string? search = "", string? type = "", string? status = "")
    {
        return OperationResultMapper.Map(await service.GetNotifications(search, type, status));
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        return OperationResultMapper.Map(await service.MarkAsRead(id));
    }

    [HttpPut("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        return OperationResultMapper.Map(await service.MarkAllAsRead());
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        return OperationResultMapper.Map(await service.DeleteNotification(id));
    }
}
