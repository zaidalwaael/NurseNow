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

        public NurseController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        // ============================
        // Personal Info
        // ============================

        [HttpGet("profile/personal-info")]
        public async Task<IActionResult> GetPersonalInfo()
        {
            var userId = GetCurrentUserId();

            var nurseProfile = await _context.NurseProfiles
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.UserId == userId);

            if (nurseProfile == null)
                return NotFound("Nurse profile not found.");

            return Ok(new
            {
                fullName = nurseProfile.User.FullName,
                email = nurseProfile.User.Email,
                phoneNumber = nurseProfile.PhoneNumber,
                location = nurseProfile.Location,
                address = nurseProfile.Address,
                bio = nurseProfile.Bio
            });
        }

        [HttpPut("profile/personal-info")]
        public async Task<IActionResult> UpdatePersonalInfo([FromBody] UpdateNursePersonalInfoDto model)
        {
            var userId = GetCurrentUserId();

            var nurseProfile = await _context.NurseProfiles
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.UserId == userId);

            if (nurseProfile == null)
                return NotFound("Nurse profile not found.");

            nurseProfile.User.FullName = model.FullName;
            nurseProfile.PhoneNumber = model.PhoneNumber;
            nurseProfile.Location = model.Location;
            nurseProfile.Address = model.Address;
            nurseProfile.Bio = model.Bio;

            await _context.SaveChangesAsync();

            return Ok("Personal information updated successfully.");
        }

        // ============================
        // Professional Details
        // ============================

        [HttpGet("profile/professional-details")]
        public async Task<IActionResult> GetProfessionalDetails()
        {
            var userId = GetCurrentUserId();

            var nurseProfile = await _context.NurseProfiles
                .FirstOrDefaultAsync(n => n.UserId == userId);

            if (nurseProfile == null)
                return NotFound("Nurse profile not found.");

            return Ok(new
            {
                licenseNumber = nurseProfile.LicenseNumber,
                specialization = nurseProfile.Specialization,
                experienceYears = nurseProfile.ExperienceYears
            });
        }

        [HttpPut("profile/professional-details")]
        public async Task<IActionResult> UpdateProfessionalDetails([FromBody] UpdateNurseProfessionalDetailsDto model)
        {
            var userId = GetCurrentUserId();

            var nurseProfile = await _context.NurseProfiles
                .FirstOrDefaultAsync(n => n.UserId == userId);

            if (nurseProfile == null)
                return NotFound("Nurse profile not found.");

            nurseProfile.LicenseNumber = model.LicenseNumber;
            nurseProfile.Specialization = model.Specialization;
            nurseProfile.ExperienceYears = model.ExperienceYears;

            await _context.SaveChangesAsync();

            return Ok("Professional details updated successfully.");
        }

        // ============================
        // Offered Services
        // ============================

        [HttpGet("services")]
        public async Task<IActionResult> GetMyServices()
        {
            var userId = GetCurrentUserId();

            var services = await _context.Services
                .Where(s => s.NurseId == userId)
                .Select(s => new
                {
                    s.ServiceId,
                    s.ServiceName,
                    s.DurationInMinutes,
                    s.Price
                })
                .ToListAsync();

            return Ok(services);
        }

        [HttpPost("services")]
        public async Task<IActionResult> AddService([FromBody] SaveNurseServiceDto model)
        {
            var userId = GetCurrentUserId();

            var service = new Service
            {
                NurseId = userId!,
                ServiceName = model.ServiceName,
                DurationInMinutes = model.DurationInMinutes,
                Price = model.Price
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            return Ok("Service added successfully.");
        }

        [HttpPut("services/{id}")]
        public async Task<IActionResult> UpdateService(int id, [FromBody] SaveNurseServiceDto model)
        {
            var userId = GetCurrentUserId();

            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.ServiceId == id && s.NurseId == userId);

            if (service == null)
                return NotFound("Service not found.");

            service.ServiceName = model.ServiceName;
            service.DurationInMinutes = model.DurationInMinutes;
            service.Price = model.Price;

            await _context.SaveChangesAsync();

            return Ok("Service updated successfully.");
        }

        [HttpDelete("services/{id}")]
        public async Task<IActionResult> DeleteService(int id)
        {
            var userId = GetCurrentUserId();

            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.ServiceId == id && s.NurseId == userId);

            if (service == null)
                return NotFound("Service not found.");

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            return Ok("Service deleted successfully.");
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetFullProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var nurseProfile = await _context.NurseProfiles
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.UserId == userId);

            if (nurseProfile == null)
                return NotFound("Nurse profile not found");

            var services = await _context.Services
                .Where(s => s.NurseId == userId)
                .Select(s => new
                {
                    s.ServiceId,
                    s.ServiceName,
                    s.DurationInMinutes,
                    s.Price
                })
                .ToListAsync();

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            return Ok(new
            {
                personalInfo = new
                {
                    fullName = nurseProfile.User.FullName,
                    email = nurseProfile.User.Email,
                    phoneNumber = nurseProfile.PhoneNumber,
                    location = nurseProfile.Location,
                    address = nurseProfile.Address,
                    bio = nurseProfile.Bio,
                    profileImageUrl = nurseProfile.ProfileImagePath != null
                        ? $"{baseUrl}/{nurseProfile.ProfileImagePath}"
                        : null
                },
                professionalDetails = new
                {
                    licenseNumber = nurseProfile.LicenseNumber,
                    specialization = nurseProfile.Specialization,
                    experienceYears = nurseProfile.ExperienceYears
                },
                services = services
            });
        }

    }
}