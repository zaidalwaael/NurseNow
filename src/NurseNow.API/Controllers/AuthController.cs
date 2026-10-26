using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseNow.API.Common;
using NurseNow.Application.Features;
using NurseNow.DTOs;
using NurseNow.DTOs.Patient;
using NurseNow.DTOs.Nurse;

namespace NurseNow.Controllers;
[Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("auth")]
[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(AuthService service) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        return OperationResultMapper.Map(await service.Register(model));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto model)
    {
        return OperationResultMapper.Map(await service.Login(model));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
    {
        return OperationResultMapper.Map(await service.ForgotPassword(model));
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
    {
        return OperationResultMapper.Map(await service.ResetPassword(model));
    }
}
