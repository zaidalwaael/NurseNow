using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseNow.API.Common;
using NurseNow.Application.Features.Admin;
using NurseNow.DTOs;
using NurseNow.DTOs.Patient;
using NurseNow.DTOs.Nurse;

namespace NurseNow.Controllers.Admin;
[ApiController]
[Route("api/admin/management")]
[Authorize(Roles = "Administrator")]
public sealed class AdminManagementController(AdminManagementService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAdmins()
    {
        return OperationResultMapper.Map(await service.GetAdmins());
    }

    [HttpPost]
    public async Task<IActionResult> CreateAdmin(NurseNow.Application.Features.Admin.CreateAdminDto dto)
    {
        return OperationResultMapper.Map(await service.CreateAdmin(dto));
    }

    [HttpDelete("{adminId}")]
    public async Task<IActionResult> DeleteAdmin(string adminId)
    {
        return OperationResultMapper.Map(await service.DeleteAdmin(adminId));
    }
}
