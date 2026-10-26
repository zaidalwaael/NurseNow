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
public sealed class PatientController(PatientService service) : ControllerBase
{
    [HttpGet("nurses/browse")]
    public async Task<IActionResult> BrowseNurses([FromQuery] BrowseNursesQueryDto query)
    {
        return OperationResultMapper.Map(await service.BrowseNurses(query));
    }

    [HttpGet("nurses/{nurseId}")]
    public async Task<IActionResult> GetNurseDetails(string nurseId)
    {
        return OperationResultMapper.Map(await service.GetNurseDetails(nurseId));
    }

    [HttpGet("nurses/{nurseId}/services")]
    public async Task<IActionResult> GetNurseServices(string nurseId)
    {
        return OperationResultMapper.Map(await service.GetNurseServices(nurseId));
    }

    [HttpGet("nurses/{nurseId}/available-dates")]
    public async Task<IActionResult> GetAvailableDates(string nurseId, [FromQuery] int daysAhead = 14)
    {
        return OperationResultMapper.Map(await service.GetAvailableDates(nurseId, daysAhead));
    }

    [HttpGet("nurses/{nurseId}/available-slots")]
    public async Task<IActionResult> GetAvailableSlots(string nurseId, [FromQuery] int serviceId, [FromQuery] DateTime date)
    {
        return OperationResultMapper.Map(await service.GetAvailableSlots(nurseId, serviceId, date));
    }

    [HttpPost("bookings")]
    public async Task<IActionResult> CreateBookingRequest([FromBody] CreateBookingRequestDto model)
    {
        return OperationResultMapper.Map(await service.CreateBookingRequest(model));
    }

    [HttpGet("appointments")]
    public async Task<IActionResult> GetPatientAppointments([FromQuery] string tab = "upcoming", [FromQuery] int? limit = null)
    {
        return OperationResultMapper.Map(await service.GetPatientAppointments(tab, limit));
    }

    [HttpGet("appointments/{bookingId}")]
    public async Task<IActionResult> GetAppointmentDetails(int bookingId)
    {
        return OperationResultMapper.Map(await service.GetAppointmentDetails(bookingId));
    }

    [HttpPut("appointments/{bookingId}/cancel")]
    public async Task<IActionResult> CancelAppointment(int bookingId)
    {
        return OperationResultMapper.Map(await service.CancelAppointment(bookingId));
    }

    [HttpPost("payments/create-intent/{bookingId}")]
    public async Task<IActionResult> CreatePaymentIntent(int bookingId)
    {
        return OperationResultMapper.Map(await service.CreatePaymentIntent(bookingId));
    }

    [HttpPost("payments/confirm/{bookingId}")]
    public async Task<IActionResult> ConfirmPayment(int bookingId)
    {
        return OperationResultMapper.Map(await service.ConfirmPayment(bookingId));
    }

    [HttpGet("payments/{bookingId}")]
    public async Task<IActionResult> GetPayment(int bookingId)
    {
        return OperationResultMapper.Map(await service.GetPayment(bookingId));
    }

    [HttpGet("payments/summary/{bookingId}")]
    public async Task<IActionResult> GetPaymentSummary(int bookingId)
    {
        return OperationResultMapper.Map(await service.GetPaymentSummary(bookingId));
    }

    [HttpGet("nurses/{nurseId}/reviews")]
    public async Task<IActionResult> GetNurseReviews(string nurseId)
    {
        return OperationResultMapper.Map(await service.GetNurseReviews(nurseId));
    }

    [HttpGet("dashboard-summary")]
    public async Task<IActionResult> GetPatientDashboardSummary()
    {
        return OperationResultMapper.Map(await service.GetPatientDashboardSummary());
    }
}
