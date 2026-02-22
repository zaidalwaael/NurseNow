using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.DTOs;
using NurseNow.Models;
using System.Security.Claims;

namespace NurseNow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Nurse")]
    public class NurseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public NurseController(ApplicationDbContext context,
                               IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile(
            [FromForm] UpdateNurseProfileDto model,
            IFormFile? profileImage,
            IFormFile? certificate)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var nurseProfile = await _context.NurseProfiles
                .FirstOrDefaultAsync(n => n.UserId == userId);

            if (nurseProfile == null)
                return NotFound("Profile not found");

            // 🔹 Update basic info
            nurseProfile.Specialization = model.Specialization;
            nurseProfile.ExperienceYears = model.ExperienceYears;
            nurseProfile.PhoneNumber = model.PhoneNumber;
            nurseProfile.Address = model.Address;
            nurseProfile.Location = model.Location;
            nurseProfile.NationalId = model.NationalId;

            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // 🔹 Upload profile image
            if (profileImage != null)
            {
                var imageName = $"{Guid.NewGuid()}_{profileImage.FileName}";
                var imagePath = Path.Combine(uploadsFolder, imageName);

                using var stream = new FileStream(imagePath, FileMode.Create);
                await profileImage.CopyToAsync(stream);

                nurseProfile.ProfileImagePath = $"uploads/{imageName}";
            }

            // 🔹 Upload certificate (PDF only)
            if (certificate != null)
            {
                if (Path.GetExtension(certificate.FileName).ToLower() != ".pdf")
                    return BadRequest("Certificate must be a PDF file");

                var certName = $"{Guid.NewGuid()}_{certificate.FileName}";
                var certPath = Path.Combine(uploadsFolder, certName);

                using var stream = new FileStream(certPath, FileMode.Create);
                await certificate.CopyToAsync(stream);

                nurseProfile.CertificatePath = $"uploads/{certName}";
            }

            await _context.SaveChangesAsync();

            return Ok("Profile updated successfully");
        }
    }
}