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
[Authorize(Roles = "Patient")]
public sealed class PatientProfileController(PatientProfileService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPatientProfile()
    {
        return OperationResultMapper.Map(await service.GetPatientProfile());
    }

    [HttpPut("personal-info")]
    public async Task<IActionResult> UpdatePersonalInfo([FromBody] UpdatePatientPersonalInfoDto model)
    {
        return OperationResultMapper.Map(await service.UpdatePersonalInfo(model));
    }

    [HttpPut("address")]
    public async Task<IActionResult> UpdateAddress([FromBody] UpdatePatientAddressDto model)
    {
        return OperationResultMapper.Map(await service.UpdateAddress(model));
    }

    [HttpPut("medical-info")]
    public async Task<IActionResult> UpdateMedicalInfo([FromBody] UpdatePatientMedicalInfoDto model)
    {
        return OperationResultMapper.Map(await service.UpdateMedicalInfo(model));
    }
}
