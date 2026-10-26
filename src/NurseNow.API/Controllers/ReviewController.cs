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
public sealed class ReviewController(ReviewService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SubmitReview([FromBody] SubmitReviewDto model)
    {
        return OperationResultMapper.Map(await service.SubmitReview(model));
    }

    [HttpPut("{bookingId}/dismiss")]
    public async Task<IActionResult> DismissReview(int bookingId)
    {
        return OperationResultMapper.Map(await service.DismissReview(bookingId));
    }

    [HttpPut("{bookingId}/later")]
    public async Task<IActionResult> RemindReviewLater(int bookingId)
    {
        return OperationResultMapper.Map(await service.RemindReviewLater(bookingId));
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingReview()
    {
        return OperationResultMapper.Map(await service.GetPendingReview());
    }
}
