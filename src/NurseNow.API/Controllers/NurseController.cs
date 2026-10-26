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
[Authorize(Roles = "Nurse")]
public sealed class NurseController(NurseService service) : ControllerBase
{
    [HttpPut("update-profile")]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateNurseProfileDto model, IFormFile? profileImage, IFormFile? certificate, IFormFile? nationalIdImage)
    {
        await using var profileImageStream = profileImage?.OpenReadStream();
        await using var certificateStream = certificate?.OpenReadStream();
        await using var nationalIdImageStream = nationalIdImage?.OpenReadStream();
        return OperationResultMapper.Map(await service.UpdateProfile(model, profileImage is null ? null : new NurseNow.Application.Abstractions.UploadFile(profileImage.FileName, profileImage.Length, profileImageStream!), certificate is null ? null : new NurseNow.Application.Abstractions.UploadFile(certificate.FileName, certificate.Length, certificateStream!), nationalIdImage is null ? null : new NurseNow.Application.Abstractions.UploadFile(nationalIdImage.FileName, nationalIdImage.Length, nationalIdImageStream!)));
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetFullProfile()
    {
        return OperationResultMapper.Map(await service.GetFullProfile());
    }

    [HttpGet("profile/personal-info")]
    public async Task<IActionResult> GetPersonalInfo()
    {
        return OperationResultMapper.Map(await service.GetPersonalInfo());
    }

    [HttpPut("profile/personal-info")]
    public async Task<IActionResult> UpdatePersonalInfo([FromBody] UpdateNursePersonalInfoDto model)
    {
        return OperationResultMapper.Map(await service.UpdatePersonalInfo(model));
    }

    [HttpGet("profile/professional-details")]
    public async Task<IActionResult> GetProfessionalDetails()
    {
        return OperationResultMapper.Map(await service.GetProfessionalDetails());
    }

    [HttpPut("profile/professional-details")]
    public async Task<IActionResult> UpdateProfessionalDetails([FromBody] UpdateNurseProfessionalDetailsDto model)
    {
        return OperationResultMapper.Map(await service.UpdateProfessionalDetails(model));
    }

    [HttpGet("service-catalog")]
    public async Task<IActionResult> GetServiceCatalog()
    {
        return OperationResultMapper.Map(await service.GetServiceCatalog());
    }

    [HttpGet("services")]
    public async Task<IActionResult> GetMyServices()
    {
        return OperationResultMapper.Map(await service.GetMyServices());
    }

    [HttpPost("services")]
    public async Task<IActionResult> AddService([FromBody] SaveNurseServiceDto model)
    {
        return OperationResultMapper.Map(await service.AddService(model));
    }

    [HttpPut("services/{id}")]
    public async Task<IActionResult> UpdateService(int id, [FromBody] SaveNurseServiceDto model)
    {
        return OperationResultMapper.Map(await service.UpdateService(id, model));
    }

    [HttpDelete("services/{id}")]
    public async Task<IActionResult> DeleteService(int id)
    {
        return OperationResultMapper.Map(await service.DeleteService(id));
    }

    [HttpGet("weekly-availability")]
    public async Task<IActionResult> GetWeeklyAvailability()
    {
        return OperationResultMapper.Map(await service.GetWeeklyAvailability());
    }

    [HttpPost("weekly-availability")]
    public async Task<IActionResult> AddWeeklyAvailability([FromBody] AddWeeklyAvailabilityDto model)
    {
        return OperationResultMapper.Map(await service.AddWeeklyAvailability(model));
    }

    [HttpDelete("weekly-availability/{id}")]
    public async Task<IActionResult> DeleteWeeklyAvailability(int id)
    {
        return OperationResultMapper.Map(await service.DeleteWeeklyAvailability(id));
    }

    [HttpGet("availability/day-details")]
    public async Task<IActionResult> GetDayDetails([FromQuery] DateTime date)
    {
        return OperationResultMapper.Map(await service.GetDayDetails(date));
    }

    [HttpPost("availability/override")]
    public async Task<IActionResult> OverrideDayAvailability([FromBody] OverrideDayAvailabilityDto model)
    {
        return OperationResultMapper.Map(await service.OverrideDayAvailability(model));
    }

    [HttpPost("availability/block-day")]
    public async Task<IActionResult> BlockDay([FromBody] BlockDayDto model)
    {
        return OperationResultMapper.Map(await service.BlockDay(model));
    }

    [HttpDelete("availability/override")]
    public async Task<IActionResult> RemoveDayOverride([FromQuery] DateTime date)
    {
        return OperationResultMapper.Map(await service.RemoveDayOverride(date));
    }

    [HttpGet("requests")]
    public async Task<IActionResult> GetNurseRequests([FromQuery] string? status)
    {
        return OperationResultMapper.Map(await service.GetNurseRequests(status));
    }

    [HttpGet("requests/{bookingId}")]
    public async Task<IActionResult> GetRequestDetails(int bookingId)
    {
        return OperationResultMapper.Map(await service.GetRequestDetails(bookingId));
    }

    [HttpPut("requests/{bookingId}/accept")]
    public async Task<IActionResult> AcceptRequest(int bookingId)
    {
        return OperationResultMapper.Map(await service.AcceptRequest(bookingId));
    }

    [HttpPut("requests/{bookingId}/decline")]
    public async Task<IActionResult> DeclineRequest(int bookingId)
    {
        return OperationResultMapper.Map(await service.DeclineRequest(bookingId));
    }

    [HttpGet("appointments")]
    public async Task<IActionResult> GetNurseAppointments([FromQuery] string tab = "upcoming")
    {
        return OperationResultMapper.Map(await service.GetNurseAppointments(tab));
    }

    [HttpGet("appointments/{bookingId}")]
    public async Task<IActionResult> GetAppointmentDetailsForNurse(int bookingId)
    {
        return OperationResultMapper.Map(await service.GetAppointmentDetailsForNurse(bookingId));
    }

    [HttpPut("appointments/{bookingId}/complete")]
    public async Task<IActionResult> CompleteAppointment(int bookingId)
    {
        return OperationResultMapper.Map(await service.CompleteAppointment(bookingId));
    }

    [HttpGet("reviews")]
    public async Task<IActionResult> GetMyReviews()
    {
        return OperationResultMapper.Map(await service.GetMyReviews());
    }

    [HttpPut("appointments/{bookingId}/cancel")]
    public async Task<IActionResult> CancelAppointmentByNurse(int bookingId)
    {
        return OperationResultMapper.Map(await service.CancelAppointmentByNurse(bookingId));
    }
}
