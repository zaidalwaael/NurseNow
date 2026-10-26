using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseNow.API.Common;
using NurseNow.Application.Features.Admin;
using NurseNow.DTOs;
using NurseNow.DTOs.Patient;
using NurseNow.DTOs.Nurse;

namespace NurseNow.Controllers.Admin;
[ApiController]
[Route("api/admin/profile")]
[Authorize(Roles = "Administrator")]
public sealed class AdminProfileController(AdminProfileService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        return OperationResultMapper.Map(await service.GetProfile());
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile(UpdateAdminProfileDto dto)
    {
        return OperationResultMapper.Map(await service.UpdateProfile(dto));
    }
}
