using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.DTOs;
using NurseNow.Helpers;
using NurseNow.Models;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using NurseNow.Services;


namespace NurseNow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase

    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtService _jwtService;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public AuthController(
          UserManager<ApplicationUser> userManager,
          JwtService jwtService,
          ApplicationDbContext context,
          IEmailService emailService)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _context = context;
            _emailService = emailService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
                return BadRequest("User already exists");

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                RoleType = model.Role,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, model.Role);
            if (model.Role == "Nurse")
            {
                var nurseProfile = new NurseProfile
                {
                    UserId = user.Id,
                    VerificationStatus = "Pending"
                };

                _context.NurseProfiles.Add(nurseProfile);
                await _context.SaveChangesAsync();
            }
            return Ok("User registered successfully");
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return Unauthorized("Invalid credentials");

            var validPassword = await _userManager.CheckPasswordAsync(user, model.Password);

            if (!validPassword)
                return Unauthorized("Invalid credentials");

            var roles = await _userManager.GetRolesAsync(user);


            var token = _jwtService.GenerateToken(user, roles);

            return Ok(new
            {
                token,
                user.FullName,
                user.Email,
                roles
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return Ok("If the email exists, a reset link has been sent.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

           /* var resetLink = $"https://yourdomain.com/reset-password?email={user.Email}&token={encodedToken}";

            var emailBody = $@"
        <h2>Password Reset</h2>
        <p>Click the link below to reset your password:</p>
        <a href='{resetLink}'>Reset Password</a>
    ";
           
            await _emailService.SendEmailAsync(user.Email, "Reset Password", emailBody);
           */
            return Ok(new
            { 
            messsage="reset token generated successfully",
            email=user.Email,
            token=encodedToken
            });
        }




        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return BadRequest("Invalid request.");

            string decodedToken;
            try
            {
                decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            }
            catch
            {
                return BadRequest("Invalid token.");
            }

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Password has been reset successfully.");
        }




    }



}