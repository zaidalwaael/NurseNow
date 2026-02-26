using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.Models;
using System.Security.Claims;

namespace NurseNow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(ApplicationDbContext context,
                                 UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            // 🔵 إذا Nurse
            if (roles.Contains("Nurse"))
            {
                var nurseProfile = await _context.NurseProfiles
                    .FirstOrDefaultAsync(n => n.UserId == userId);

                return Ok(new
                {
                    userId = user.Id,
                    fullName = user.FullName,
                    email = user.Email,
                    role = "Nurse",
                    verificationStatus = nurseProfile?.VerificationStatus,
                    specialization = nurseProfile?.Specialization,
                    experienceYears = nurseProfile?.ExperienceYears,
                    phoneNumber = nurseProfile?.PhoneNumber,
                    address = nurseProfile?.Address,
                    location = nurseProfile?.Location,
                    nationalId = nurseProfile?.NationalId,
                    profileImagePath = nurseProfile?.ProfileImagePath,
                    certificatePath = nurseProfile?.CertificatePath
                });
            }

            // 🟣 Admin أو Patient
            return Ok(new
            {
                userId = user.Id,
                fullName = user.FullName,
                email = user.Email,
                role = roles.FirstOrDefault()
            });
        }
    }
}