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
        public async Task<IActionResult> UpdateProfile(
            [FromForm] UpdateNurseProfileDto model,
            IFormFile? profileImage,
            IFormFile? certificate,
            IFormFile? nationalIdImage)
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var nurseProfile = await _context.NurseProfiles
                .FirstOrDefaultAsync(n => n.UserId == userId);

            if (nurseProfile == null)
                return NotFound("Nurse profile not found.");

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

        [HttpGet("profile")]
        public async Task<IActionResult> GetFullProfile()
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

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
                    naionalId = nurseProfile.NationalId,
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
                services
            });
        }

        [HttpGet("profile/personal-info")]
        public async Task<IActionResult> GetPersonalInfo()
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

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
                bio = nurseProfile.Bio,
                nationalId = nurseProfile.NationalId,
            });
        }

        [HttpPut("profile/personal-info")]
        public async Task<IActionResult> UpdatePersonalInfo([FromBody] UpdateNursePersonalInfoDto model)
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

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

        [HttpGet("profile/professional-details")]
        public async Task<IActionResult> GetProfessionalDetails()
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

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

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

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

        [HttpGet("services")]
        public async Task<IActionResult> GetMyServices()
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

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

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

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
                NurseId = userId,
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

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

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

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.ServiceId == id && s.NurseId == userId);

            if (service == null)
                return NotFound("Service not found.");

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            return Ok("Service deleted successfully.");
        }

        [HttpGet("weekly-availability")]
        public async Task<IActionResult> GetWeeklyAvailability()
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var weeklySchedule = await _context.WeeklyAvailabilities
                .Where(w => w.NurseId == userId)
                .OrderBy(w => w.WeeklyAvailabilityId)
                .Select(w => new
                {
                    w.WeeklyAvailabilityId,
                    w.DayOfWeek,
                    startTime = w.StartTime.ToString(@"hh\:mm"),
                    endTime = w.EndTime.ToString(@"hh\:mm"),
                    w.IsActive
                })
                .ToListAsync();

            return Ok(weeklySchedule);
        }

        [HttpPost("weekly-availability")]
        public async Task<IActionResult> AddWeeklyAvailability([FromBody] AddWeeklyAvailabilityDto model)
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (model.StartTime >= model.EndTime)
                return BadRequest("Start time must be earlier than end time.");

            var validDays = new[]
            {
                "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"
            };

            if (!validDays.Contains(model.DayOfWeek))
                return BadRequest("Invalid day of week.");

            var alreadyExists = await _context.WeeklyAvailabilities.AnyAsync(w =>
                w.NurseId == userId &&
                w.DayOfWeek == model.DayOfWeek &&
                w.StartTime == model.StartTime &&
                w.EndTime == model.EndTime);

            if (alreadyExists)
                return BadRequest("This time slot already exists.");

            var availability = new WeeklyAvailability
            {
                NurseId = userId,
                DayOfWeek = model.DayOfWeek,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                IsActive = true
            };

            _context.WeeklyAvailabilities.Add(availability);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Working hours added successfully.",
                availabilityId = availability.WeeklyAvailabilityId
            });
        }

        [HttpDelete("weekly-availability/{id}")]
        public async Task<IActionResult> DeleteWeeklyAvailability(int id)
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var availability = await _context.WeeklyAvailabilities
                .FirstOrDefaultAsync(w => w.WeeklyAvailabilityId == id && w.NurseId == userId);

            if (availability == null)
                return NotFound("Weekly availability not found.");

            _context.WeeklyAvailabilities.Remove(availability);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Weekly availability deleted successfully."
            });
        }

        [HttpGet("availability/day-details")]
        public async Task<IActionResult> GetDayDetails([FromQuery] DateTime date)
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var dayOfWeek = date.DayOfWeek.ToString();

            var weeklyAvailability = await _context.WeeklyAvailabilities
                .Where(w => w.NurseId == userId && w.DayOfWeek == dayOfWeek && w.IsActive)
                .Select(w => new
                {
                    w.WeeklyAvailabilityId,
                    w.DayOfWeek,
                    startTime = w.StartTime.ToString(@"hh\:mm"),
                    endTime = w.EndTime.ToString(@"hh\:mm")
                })
                .FirstOrDefaultAsync();

            var overrideRecord = await _context.AvailabilityOverrides
                .Where(o => o.NurseId == userId && o.Date.Date == date.Date)
                .Select(o => new
                {
                    o.AvailabilityOverrideId,
                    date = o.Date.ToString("yyyy-MM-dd"),
                    startTime = o.StartTime.HasValue ? o.StartTime.Value.ToString(@"hh\:mm") : null,
                    endTime = o.EndTime.HasValue ? o.EndTime.Value.ToString(@"hh\:mm") : null,
                    o.IsBlocked
                })
                .FirstOrDefaultAsync();

            var bookedAppointments = new List<object>();

            return Ok(new
            {
                selectedDate = date.ToString("yyyy-MM-dd"),
                dayOfWeek,
                defaultWorkingHours = weeklyAvailability,
                dayOverride = overrideRecord,
                bookedAppointments
            });
        }

        [HttpPost("availability/override")]
        public async Task<IActionResult> OverrideDayAvailability([FromBody] OverrideDayAvailabilityDto model)
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (model.StartTime >= model.EndTime)
                return BadRequest("Start time must be earlier than end time.");

            var existingOverride = await _context.AvailabilityOverrides
                .FirstOrDefaultAsync(o => o.NurseId == userId && o.Date.Date == model.Date.Date);

            if (existingOverride != null)
            {
                existingOverride.StartTime = model.StartTime;
                existingOverride.EndTime = model.EndTime;
                existingOverride.IsBlocked = false;
            }
            else
            {
                var newOverride = new AvailabilityOverride
                {
                    NurseId = userId,
                    Date = model.Date.Date,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    IsBlocked = false
                };

                _context.AvailabilityOverrides.Add(newOverride);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Day working hours overridden successfully."
            });
        }

        [HttpPost("availability/block-day")]
        public async Task<IActionResult> BlockDay([FromBody] BlockDayDto model)
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var existingOverride = await _context.AvailabilityOverrides
                .FirstOrDefaultAsync(o => o.NurseId == userId && o.Date.Date == model.Date.Date);

            if (existingOverride != null)
            {
                existingOverride.StartTime = null;
                existingOverride.EndTime = null;
                existingOverride.IsBlocked = true;
            }
            else
            {
                var blockedDay = new AvailabilityOverride
                {
                    NurseId = userId,
                    Date = model.Date.Date,
                    StartTime = null,
                    EndTime = null,
                    IsBlocked = true
                };

                _context.AvailabilityOverrides.Add(blockedDay);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Day blocked successfully."
            });
        }

        [HttpDelete("availability/override")]
        public async Task<IActionResult> RemoveDayOverride([FromQuery] DateTime date)
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var overrideRecord = await _context.AvailabilityOverrides
                .FirstOrDefaultAsync(o => o.NurseId == userId && o.Date.Date == date.Date);

            if (overrideRecord == null)
                return NotFound("No override found for this date.");

            _context.AvailabilityOverrides.Remove(overrideRecord);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Day override removed successfully."
            });
        }

        [HttpGet("requests")]
        public async Task<IActionResult> GetNurseRequests([FromQuery] string? status)
        {
            var nurseId = GetCurrentUserId();

            if (string.IsNullOrEmpty(nurseId))
                return Unauthorized();

            var query = _context.Bookings
                .Include(b => b.Patient)
                .Include(b => b.Service)
                    .ThenInclude(s => s.ServiceCatalog)
                .Where(b => b.NurseId == nurseId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(b => b.Status == status);
            }

            var requests = await query
                .OrderByDescending(b => b.BookingDate)
                .ThenBy(b => b.StartTime)
                .Select(b => new
                {
                    bookingId = b.BookingId,
                    patientName = b.Patient.FullName,
                    patientPhone = b.Patient.PhoneNumber,
                    serviceName = b.Service.ServiceCatalog.Name,
                    date = b.BookingDate.ToString("yyyy-MM-dd"),
                    time = b.StartTime.ToString(@"hh\:mm"),
                    location = b.ServiceAddress,
                    durationInMinutes = b.Service.ServiceCatalog.DefaultDurationInMinutes,
                    totalPrice = b.Service.Price,
                    notes = b.AdditionalNotes,
                    status = b.Status
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpGet("requests/{bookingId}")]
        public async Task<IActionResult> GetRequestDetails(int bookingId)
        {
            var nurseId = GetCurrentUserId();

            if (string.IsNullOrEmpty(nurseId))
                return Unauthorized();

            var booking = await _context.Bookings
                .Include(b => b.Patient)
                .Include(b => b.Service)
                    .ThenInclude(s => s.ServiceCatalog)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.NurseId == nurseId);

            if (booking == null)
                return NotFound("Request not found.");

            return Ok(new
            {
                bookingId = booking.BookingId,
                patientName = booking.Patient.FullName,
                contact = booking.Patient.PhoneNumber,
                serviceType = booking.Service.ServiceCatalog.Name,
                date = booking.BookingDate.ToString("yyyy-MM-dd"),
                time = booking.StartTime.ToString(@"hh\:mm"),
                location = booking.ServiceAddress,
                durationInMinutes = booking.Service.ServiceCatalog.DefaultDurationInMinutes,
                payment = booking.Service.Price,
                additionalNotes = booking.AdditionalNotes,
                status = booking.Status
            });
        }

        [HttpPut("requests/{bookingId}/accept")]
        public async Task<IActionResult> AcceptRequest(int bookingId)
        {
            var nurseId = GetCurrentUserId();

            if (string.IsNullOrEmpty(nurseId))
                return Unauthorized();

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.NurseId == nurseId);

            if (booking == null)
                return NotFound("Request not found.");

            if (booking.Status != "Pending")
                return BadRequest("Only pending requests can be accepted.");

            booking.Status = "Accepted";

            _context.Notifications.Add(new Notification
            {
                UserId = booking.PatientId,
                Title = "Booking Confirmed",
                Message = "Your booking request has been accepted by the nurse.",
                Type = "Booking",
                BookingId = booking.BookingId,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Request accepted successfully.",
                status = booking.Status
            });
        }

        [HttpPut("requests/{bookingId}/decline")]
        public async Task<IActionResult> DeclineRequest(int bookingId)
        {
            var nurseId = GetCurrentUserId();

            if (string.IsNullOrEmpty(nurseId))
                return Unauthorized();

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.NurseId == nurseId);

            if (booking == null)
                return NotFound("Request not found.");

            if (booking.Status != "Pending")
                return BadRequest("Only pending requests can be declined.");

            booking.Status = "Rejected";

            _context.Notifications.Add(new Notification
            {
                UserId = booking.PatientId,
                Title = "Request Rejected",
                Message = "Your booking request has been rejected by the nurse.",
                Type = "Booking",
                BookingId = booking.BookingId,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Request declined successfully.",
                status = booking.Status
            });
        }

        [HttpGet("appointments")]
        public async Task<IActionResult> GetNurseAppointments([FromQuery] string tab = "upcoming")
        {
            var nurseId = GetCurrentUserId();

            if (string.IsNullOrEmpty(nurseId))
                return Unauthorized();

            var today = DateTime.Today;

            var query = _context.Bookings
                .Include(b => b.Patient)
                .Include(b => b.Service)
                    .ThenInclude(s => s.ServiceCatalog)
                .Where(b => b.NurseId == nurseId)
                .AsQueryable();

            if (tab.ToLower() == "today")
            {
                query = query.Where(b =>
                    b.BookingDate.Date == today &&
                    (b.Status == "Accepted" || b.Status == "Active"));
            }
            else if (tab.ToLower() == "upcoming")
            {
                query = query.Where(b =>
                    b.BookingDate.Date > today &&
                    (b.Status == "Pending" || b.Status == "Accepted" || b.Status == "Active"));
            }
            else if (tab.ToLower() == "past")
            {
                query = query.Where(b =>
                    b.Status == "Completed" ||
                    b.Status == "Cancelled" ||
                    b.Status == "Rejected" ||
                    b.BookingDate.Date < today);
            }
            else
            {
                return BadRequest("Invalid tab value. Use 'today', 'upcoming', or 'past'.");
            }

            var appointments = await query
                .OrderBy(b => b.BookingDate)
                .ThenBy(b => b.StartTime)
                .Select(b => new
                {
                    bookingId = b.BookingId,
                    patientName = b.Patient.FullName,
                    serviceName = b.Service.ServiceCatalog.Name,
                    date = b.BookingDate.ToString("yyyy-MM-dd"),
                    time = b.StartTime.ToString(@"hh\:mm"),
                    address = b.ServiceAddress,
                    totalPrice = b.Service.Price,
                    status = b.Status
                })
                .ToListAsync();

            return Ok(appointments);
        }

        [HttpGet("appointments/{bookingId}")]
        public async Task<IActionResult> GetAppointmentDetailsForNurse(int bookingId)
        {
            var nurseId = GetCurrentUserId();

            if (string.IsNullOrEmpty(nurseId))
                return Unauthorized();

            var booking = await _context.Bookings
                .Include(b => b.Patient)
                .Include(b => b.Service)
                    .ThenInclude(s => s.ServiceCatalog)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.NurseId == nurseId);

            if (booking == null)
                return NotFound("Appointment not found.");

            return Ok(new
            {
                bookingId = booking.BookingId,
                patientName = booking.Patient.FullName,
                phoneNumber = booking.Patient.PhoneNumber,
                serviceName = booking.Service.ServiceCatalog.Name,
                date = booking.BookingDate.ToString("yyyy-MM-dd"),
                time = booking.StartTime.ToString(@"hh\:mm"),
                address = booking.ServiceAddress,
                totalPrice = booking.Service.Price,
                additionalNotes = booking.AdditionalNotes,
                status = booking.Status
            });
        }

        [HttpPut("appointments/{bookingId}/complete")]
        public async Task<IActionResult> CompleteAppointment(int bookingId)
        {
            var nurseId = GetCurrentUserId();

            if (string.IsNullOrEmpty(nurseId))
                return Unauthorized();

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.NurseId == nurseId);

            if (booking == null)
                return NotFound("Appointment not found.");

            if (booking.Status != "Accepted" && booking.Status != "Active")
                return BadRequest("Only accepted or active appointments can be marked as completed.");

            booking.Status = "Completed";

            _context.Notifications.Add(new Notification
            {
                UserId = booking.PatientId,
                Title = "Appointment Completed",
                Message = "Your appointment has been marked as completed.",
                Type = "Booking",
                BookingId = booking.BookingId,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Appointment marked as completed successfully.",
                status = booking.Status
            });
        }

        [HttpPut("appointments/{bookingId}/cancel")]
        public async Task<IActionResult> CancelAppointmentByNurse(int bookingId)
        {
            var nurseId = GetCurrentUserId();

            if (string.IsNullOrEmpty(nurseId))
                return Unauthorized();

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.NurseId == nurseId);

            if (booking == null)
                return NotFound("Appointment not found.");

            if (booking.Status != "Accepted" && booking.Status != "Pending")
                return BadRequest("Only pending or accepted appointments can be cancelled.");

            booking.Status = "Cancelled";

            _context.Notifications.Add(new Notification
            {
                UserId = booking.PatientId,
                Title = "Appointment Cancelled",
                Message = "The nurse has cancelled your appointment.",
                Type = "Booking",
                BookingId = booking.BookingId,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Appointment cancelled successfully.",
                status = booking.Status
            });
        }
    }
}