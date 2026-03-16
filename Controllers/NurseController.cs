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

        public NurseController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        [HttpPut("update-profile")]
        [Authorize(Roles = "Nurse")]
        public async Task<IActionResult> UpdateProfile(
    [FromForm] UpdateNurseProfileDto model,
    IFormFile? profileImage,
    IFormFile? certificate,
    IFormFile? nationalIdImage)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var nurseProfile = await _context.NurseProfiles
                .FirstOrDefaultAsync(n => n.UserId == userId);

            if (nurseProfile == null)
                return NotFound("Nurse profile not found.");

            // Update text fields
            nurseProfile.PhoneNumber = model.PhoneNumber;
            nurseProfile.Address = model.Address;
            nurseProfile.Location = model.Location;
            nurseProfile.Bio = model.Bio;
            nurseProfile.LicenseNumber = model.LicenseNumber;
            nurseProfile.Specialization = model.Specialization;
            nurseProfile.ExperienceYears = model.ExperienceYears;
            nurseProfile.NationalId = model.NationalId;

            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Profile Image
            if (profileImage != null)
            {
                var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var imageExtension = Path.GetExtension(profileImage.FileName).ToLower();

                if (!allowedImageExtensions.Contains(imageExtension))
                    return BadRequest("Profile image must be JPG, JPEG, or PNG.");

                var imageName = $"{Guid.NewGuid()}{imageExtension}";
                var imagePath = Path.Combine(uploadsFolder, imageName);

                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }

                nurseProfile.ProfileImagePath = $"uploads/{imageName}";
            }

            // Certificate PDF
            if (certificate != null)
            {
                var certExtension = Path.GetExtension(certificate.FileName).ToLower();

                if (certExtension != ".pdf")
                    return BadRequest("Certificate must be a PDF file.");

                var certName = $"{Guid.NewGuid()}.pdf";
                var certPath = Path.Combine(uploadsFolder, certName);

                using (var stream = new FileStream(certPath, FileMode.Create))
                {
                    await certificate.CopyToAsync(stream);
                }

                nurseProfile.CertificatePath = $"uploads/{certName}";
            }

            // National ID Image
            if (nationalIdImage != null)
            {
                var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var idImageExtension = Path.GetExtension(nationalIdImage.FileName).ToLower();

                if (!allowedImageExtensions.Contains(idImageExtension))
                    return BadRequest("National ID image must be JPG, JPEG, or PNG.");

                var idImageName = $"{Guid.NewGuid()}{idImageExtension}";
                var idImagePath = Path.Combine(uploadsFolder, idImageName);

                using (var stream = new FileStream(idImagePath, FileMode.Create))
                {
                    await nationalIdImage.CopyToAsync(stream);
                }

                nurseProfile.NationalIdImagePath = $"uploads/{idImageName}";
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully.",
                nurseProfile.ProfileImagePath,
                nurseProfile.CertificatePath,
                nurseProfile.NationalIdImagePath
            });
        }



        // ============================
        // Full Profile Widget
        // ============================
        [HttpGet("profile")]
        public async Task<IActionResult> GetFullProfile()
        {
            var userId = GetCurrentUserId();

            var nurseProfile = await _context.NurseProfiles
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.UserId == userId);

            if (nurseProfile == null)
                return NotFound("Nurse profile not found.");

            var services = await _context.Services
                .Include(s => s.ServiceCatalog)
                .Where(s => s.NurseId == userId)
                .Select(s => new
                {
                    s.ServiceId,
                    s.ServiceCatalogId,
                    serviceName = s.ServiceCatalog.Name,
                    durationInMinutes = s.ServiceCatalog.DefaultDurationInMinutes,
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
        // Service Catalog (Dropdown List)
        // ============================
        [HttpGet("service-catalog")]
        public async Task<IActionResult> GetServiceCatalog()
        {
            var catalog = await _context.ServiceCatalogs
                .Select(s => new
                {
                    s.ServiceCatalogId,
                    s.Name,
                    s.DefaultDurationInMinutes
                })
                .ToListAsync();

            return Ok(catalog);
        }

        // ============================
        // Offered Services
        // ============================
        [HttpGet("services")]
        public async Task<IActionResult> GetMyServices()
        {
            var userId = GetCurrentUserId();

            var services = await _context.Services
                .Include(s => s.ServiceCatalog)
                .Where(s => s.NurseId == userId)
                .Select(s => new
                {
                    s.ServiceId,
                    s.ServiceCatalogId,
                    serviceName = s.ServiceCatalog.Name,
                    durationInMinutes = s.ServiceCatalog.DefaultDurationInMinutes,
                    s.Price
                })
                .ToListAsync();

            return Ok(services);
        }

        [HttpPost("services")]
        public async Task<IActionResult> AddService([FromBody] SaveNurseServiceDto model)
        {
            var userId = GetCurrentUserId();

            var serviceCatalog = await _context.ServiceCatalogs
                .FirstOrDefaultAsync(s => s.ServiceCatalogId == model.ServiceCatalogId);

            if (serviceCatalog == null)
                return BadRequest("Invalid service.");

            var alreadyExists = await _context.Services.AnyAsync(s =>
                s.NurseId == userId && s.ServiceCatalogId == model.ServiceCatalogId);

            if (alreadyExists)
                return BadRequest("This service has already been added.");

            var service = new Service
            {
                NurseId = userId!,
                ServiceCatalogId = model.ServiceCatalogId,
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

            var serviceCatalog = await _context.ServiceCatalogs
                .FirstOrDefaultAsync(s => s.ServiceCatalogId == model.ServiceCatalogId);

            if (serviceCatalog == null)
                return BadRequest("Invalid service.");

            service.ServiceCatalogId = model.ServiceCatalogId;
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
    }
}