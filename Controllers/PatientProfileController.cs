using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.DTOs;
using System.Security.Claims;

namespace NurseNow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Patient")]
    public class PatientProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PatientProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        [HttpGet]
        public async Task<IActionResult> GetPatientProfile()
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var profile = await _context.PatientProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return NotFound("Patient profile not found.");

            return Ok(new
            {
                fullName = profile.User.FullName,
                email = profile.User.Email,
                gender = profile.Gender,
                dateOfBirth = profile.DateOfBirth,
                bloodType = profile.BloodType,
                governorate = profile.Governorate,
                area = profile.Area,
                address = profile.Address,
                conditions = profile.Conditions,
                allergies = profile.Allergies,
                notes = profile.Notes        

            });
        }

        [HttpPut("personal-info")]
        public async Task<IActionResult> UpdatePersonalInfo([FromBody] UpdatePatientPersonalInfoDto model)
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var profile = await _context.PatientProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return NotFound("Patient profile not found.");

            profile.Gender = model.Gender;
            profile.DateOfBirth = model.DateOfBirth;
            profile.BloodType = model.BloodType;

            await _context.SaveChangesAsync();

            return Ok("Personal info updated successfully.");
        }

        [HttpPut("address")]
        public async Task<IActionResult> UpdateAddress([FromBody] UpdatePatientAddressDto model)
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var profile = await _context.PatientProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return NotFound("Patient profile not found.");

            profile.Governorate = model.Governorate;
            profile.Area = model.Area;
            profile.Address = model.Address;

            await _context.SaveChangesAsync();

            return Ok("Address updated successfully.");
        }

        [HttpPut("medical-info")]
        public async Task<IActionResult> UpdateMedicalInfo([FromBody] UpdatePatientMedicalInfoDto model)
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var profile = await _context.PatientProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return NotFound("Patient profile not found.");

            profile.Conditions = model.Conditions;
            profile.Allergies = model.Allergies;
            profile.Notes = model.Notes;

            await _context.SaveChangesAsync();

            return Ok("Medical info updated successfully.");
        }
    }
}