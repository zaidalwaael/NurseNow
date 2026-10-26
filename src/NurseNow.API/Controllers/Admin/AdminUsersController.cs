using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseNow.API.Common;
using NurseNow.Application.Features.Admin;
using NurseNow.DTOs;
using NurseNow.DTOs.Patient;
using NurseNow.DTOs.Nurse;

namespace NurseNow.Controllers.Admin;
[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Administrator")]
public sealed class AdminUsersController(AdminUsersService service) : ControllerBase
{
    [HttpGet]
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] string type = "Patient", [FromQuery] string? search = null, [FromQuery] string? status = null)
    {
        return OperationResultMapper.Map(await service.GetUsers(type, search, status));
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserDetails(string userId)
    {
        return OperationResultMapper.Map(await service.GetUserDetails(userId));
    }

    [HttpPut("{userId}/toggle-status")]
    public async Task<IActionResult> ToggleUserStatus(string userId)
    {
        return OperationResultMapper.Map(await service.ToggleUserStatus(userId));
    }

    [HttpPut("{userId}/reset-password")]
    public async Task<IActionResult> ResetUserPassword(string userId, ResetUserPasswordDto dto)
    {
        return OperationResultMapper.Map(await service.ResetUserPassword(userId, dto));
    }
}
