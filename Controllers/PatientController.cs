using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.DTOs;
using NurseNow.Models;
using Stripe;
using System.Security.Claims;

namespace NurseNow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Patient")]
    public class PatientController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PatientController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("nurses/browse")]
        public async Task<IActionResult> BrowseNurses([FromQuery] BrowseNursesQueryDto query)
        {
            var nursesQuery = _context.NurseProfiles
                .Include(n => n.User)
                .Where(n => n.VerificationStatus == "Approved")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();

                nursesQuery = nursesQuery.Where(n =>
                    n.User.FullName.ToLower().Contains(search) ||
                    (n.Specialization != null && n.Specialization.ToLower().Contains(search)) ||
                    (n.Location != null && n.Location.ToLower().Contains(search))
                );
            }

            if (!string.IsNullOrWhiteSpace(query.Location))
            {
                var location = query.Location.Trim().ToLower();

                nursesQuery = nursesQuery.Where(n =>
                    n.Location != null && n.Location.ToLower() == location);
            }

            if (query.ServiceCatalogId.HasValue)
            {
                nursesQuery = nursesQuery.Where(n =>
                    _context.Services.Any(s =>
                        s.NurseId == n.UserId &&
                        s.ServiceCatalogId == query.ServiceCatalogId.Value));
            }

            if (!string.IsNullOrWhiteSpace(query.Location))
            {
                nursesQuery = nursesQuery.OrderBy(n => n.Address);
            }
            else
            {
                nursesQuery = nursesQuery.OrderByDescending(n => n.ExperienceYears);
            }

            var totalCount = await nursesQuery.CountAsync();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var nurses = await nursesQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(n => new
                {
                    nurseId = n.UserId,
                    fullName = n.User.FullName,
                    specialization = n.Specialization,
                    location = n.Location,
                    address = n.Address,
                    experienceYears = n.ExperienceYears,
                    profileImageUrl = n.ProfileImagePath != null
                        ? $"{baseUrl}/{n.ProfileImagePath}"
                        : null,
                    rating = 0.0,
                    reviewsCount = 0,
                    price = query.ServiceCatalogId.HasValue
                        ? _context.Services
                            .Where(s => s.NurseId == n.UserId && s.ServiceCatalogId == query.ServiceCatalogId.Value)
                            .Select(s => (decimal?)s.Price)
                            .FirstOrDefault()
                        : _context.Services
                            .Where(s => s.NurseId == n.UserId)
                            .Select(s => (decimal?)s.Price)
                            .Min(),
                    availabilityLabel = _context.WeeklyAvailabilities.Any(w =>
                        w.NurseId == n.UserId && w.IsActive)
                        ? "Available This Week"
                        : "Unavailable"
                })
                .ToListAsync();

            return Ok(new
            {
                pageNumber = query.PageNumber,
                pageSize = query.PageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize),
                hasNextPage = query.PageNumber * query.PageSize < totalCount,
                items = nurses
            });
        }

        [HttpGet("nurses/{nurseId}")]
        public async Task<IActionResult> GetNurseDetails(string nurseId)
        {
            var nurseProfile = await _context.NurseProfiles
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.UserId == nurseId && n.VerificationStatus == "Approved");

            if (nurseProfile == null)
                return NotFound("Nurse not found.");

            var services = await _context.Services
                .Include(s => s.ServiceCatalog)
                .Where(s => s.NurseId == nurseId)
                .Select(s => s.ServiceCatalog.Name)
                .ToListAsync();

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var today = DateTime.Today;
            var todayDayName = today.DayOfWeek.ToString();

            var hasOverrideToday = await _context.AvailabilityOverrides
                .AnyAsync(o => o.NurseId == nurseId && o.Date.Date == today.Date && !o.IsBlocked);

            var isBlockedToday = await _context.AvailabilityOverrides
                .AnyAsync(o => o.NurseId == nurseId && o.Date.Date == today.Date && o.IsBlocked);

            var hasWeeklyToday = await _context.WeeklyAvailabilities
                .AnyAsync(w => w.NurseId == nurseId && w.DayOfWeek == todayDayName && w.IsActive);

            var hasAnyWeeklyThisWeek = await _context.WeeklyAvailabilities
                .AnyAsync(w => w.NurseId == nurseId && w.IsActive);

            string availabilityLabel;

            if (hasOverrideToday || (!isBlockedToday && hasWeeklyToday))
                availabilityLabel = "Available Today";
            else if (hasAnyWeeklyThisWeek)
                availabilityLabel = "Available This Week";
            else
                availabilityLabel = "Unavailable";

            string headline;
            if (services.Count >= 2)
                headline = $"{services[0]} & {services[1]}";
            else if (services.Count == 1)
                headline = services[0];
            else
                headline = nurseProfile.Specialization ?? "Nurse";

            return Ok(new
            {
                nurseId = nurseProfile.UserId,
                fullName = nurseProfile.User.FullName,
                profileImageUrl = nurseProfile.ProfileImagePath != null
                    ? $"{baseUrl}/{nurseProfile.ProfileImagePath}"
                    : null,
                headline,
                rating = 0.0,
                reviewsCount = 0,
                experienceYears = nurseProfile.ExperienceYears,
                location = nurseProfile.Location,
                address = nurseProfile.Address,
                availabilityLabel,
                certificateUrl = nurseProfile.CertificatePath != null
                    ? $"{baseUrl}/{nurseProfile.CertificatePath}"
                    : null,
                servicesOffered = services
            });
        }

        [HttpGet("nurses/{nurseId}/services")]
        public async Task<IActionResult> GetNurseServices(string nurseId)
        {
            var nurseExists = await _context.NurseProfiles
                .AnyAsync(n => n.UserId == nurseId && n.VerificationStatus == "Approved");

            if (!nurseExists)
                return NotFound("Nurse not found.");

            var services = await _context.Services
                .Include(s => s.ServiceCatalog)
                .Where(s => s.NurseId == nurseId)
                .Select(s => new
                {
                    serviceId = s.ServiceId,
                    serviceCatalogId = s.ServiceCatalogId,
                    serviceName = s.ServiceCatalog.Name,
                    durationInMinutes = s.ServiceCatalog.DefaultDurationInMinutes,
                    price = s.Price
                })
                .ToListAsync();

            return Ok(services);
        }

        [HttpGet("nurses/{nurseId}/available-dates")]
        public async Task<IActionResult> GetAvailableDates(string nurseId, [FromQuery] int daysAhead = 14)
        {
            var nurseExists = await _context.NurseProfiles
                .AnyAsync(n => n.UserId == nurseId && n.VerificationStatus == "Approved");

            if (!nurseExists)
                return NotFound("Nurse not found.");

            if (daysAhead <= 0)
                return BadRequest("daysAhead must be greater than 0.");

            var availableDates = new List<string>();
            var today = DateTime.Today;

            for (int i = 0; i < daysAhead; i++)
            {
                var currentDate = today.AddDays(i);
                var dayName = currentDate.DayOfWeek.ToString();

                var overrideRecord = await _context.AvailabilityOverrides
                    .FirstOrDefaultAsync(o => o.NurseId == nurseId && o.Date.Date == currentDate.Date);

                if (overrideRecord != null && overrideRecord.IsBlocked)
                    continue;

                if (overrideRecord != null &&
                    !overrideRecord.IsBlocked &&
                    overrideRecord.StartTime.HasValue &&
                    overrideRecord.EndTime.HasValue &&
                    overrideRecord.StartTime.Value < overrideRecord.EndTime.Value)
                {
                    availableDates.Add(currentDate.ToString("yyyy-MM-dd"));
                    continue;
                }

                var hasWeeklyAvailability = await _context.WeeklyAvailabilities.AnyAsync(w =>
                    w.NurseId == nurseId &&
                    w.DayOfWeek == dayName &&
                    w.IsActive &&
                    w.StartTime < w.EndTime);

                if (hasWeeklyAvailability)
                {
                    availableDates.Add(currentDate.ToString("yyyy-MM-dd"));
                }
            }

            return Ok(availableDates);
        }

        [HttpGet("nurses/{nurseId}/available-slots")]
        public async Task<IActionResult> GetAvailableSlots(string nurseId, [FromQuery] int serviceId, [FromQuery] DateTime date)
        {
            var nurseExists = await _context.NurseProfiles
                .AnyAsync(n => n.UserId == nurseId && n.VerificationStatus == "Approved");

            if (!nurseExists)
                return NotFound("Nurse not found.");

            var service = await _context.Services
                .Include(s => s.ServiceCatalog)
                .FirstOrDefaultAsync(s => s.ServiceId == serviceId && s.NurseId == nurseId);

            if (service == null)
                return NotFound("Service not found for this nurse.");

            var serviceDuration = service.ServiceCatalog.DefaultDurationInMinutes;

            TimeSpan? startTime = null;
            TimeSpan? endTime = null;

            var overrideRecord = await _context.AvailabilityOverrides
                .FirstOrDefaultAsync(o => o.NurseId == nurseId && o.Date.Date == date.Date);

            if (overrideRecord != null && overrideRecord.IsBlocked)
                return Ok(new List<string>());

            if (overrideRecord != null &&
                !overrideRecord.IsBlocked &&
                overrideRecord.StartTime.HasValue &&
                overrideRecord.EndTime.HasValue)
            {
                startTime = overrideRecord.StartTime.Value;
                endTime = overrideRecord.EndTime.Value;
            }
            else
            {
                var dayName = date.DayOfWeek.ToString();

                var weeklyAvailability = await _context.WeeklyAvailabilities
                    .FirstOrDefaultAsync(w =>
                        w.NurseId == nurseId &&
                        w.DayOfWeek == dayName &&
                        w.IsActive);

                if (weeklyAvailability == null)
                    return Ok(new List<string>());

                startTime = weeklyAvailability.StartTime;
                endTime = weeklyAvailability.EndTime;
            }

            if (!startTime.HasValue || !endTime.HasValue || startTime.Value >= endTime.Value)
                return Ok(new List<string>());

            var slots = new List<string>();
            var current = startTime.Value;
            var duration = TimeSpan.FromMinutes(serviceDuration);

            while (current + duration <= endTime.Value)
            {
                slots.Add(current.ToString(@"hh\:mm"));
                current = current.Add(duration);
            }

            return Ok(slots);
        }

        [HttpPost("bookings")]
        public async Task<IActionResult> CreateBookingRequest([FromBody] CreateBookingRequestDto model)
        {
            var patientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (patientId == null)
                return Unauthorized();

            var nurseExists = await _context.NurseProfiles
                .AnyAsync(n => n.UserId == model.NurseId && n.VerificationStatus == "Approved");

            if (!nurseExists)
                return BadRequest("Selected nurse is not available.");

            var service = await _context.Services
                .Include(s => s.ServiceCatalog)
                .Include(s => s.Nurse)
                .FirstOrDefaultAsync(s => s.ServiceId == model.ServiceId && s.NurseId == model.NurseId);

            if (service == null)
                return BadRequest("Selected service is invalid.");

            if (string.IsNullOrWhiteSpace(model.ServiceAddress))
                return BadRequest("Service address is required.");

            var duration = TimeSpan.FromMinutes(service.ServiceCatalog.DefaultDurationInMinutes);
            var endTime = model.StartTime.Add(duration);

            var overrideRecord = await _context.AvailabilityOverrides
                .FirstOrDefaultAsync(o => o.NurseId == model.NurseId && o.Date.Date == model.Date.Date);

            if (overrideRecord != null && overrideRecord.IsBlocked)
                return BadRequest("The selected date is not available.");

            TimeSpan? workingStart = null;
            TimeSpan? workingEnd = null;

            if (overrideRecord != null &&
                !overrideRecord.IsBlocked &&
                overrideRecord.StartTime.HasValue &&
                overrideRecord.EndTime.HasValue)
            {
                workingStart = overrideRecord.StartTime.Value;
                workingEnd = overrideRecord.EndTime.Value;
            }
            else
            {
                var dayName = model.Date.DayOfWeek.ToString();

                var weeklyAvailability = await _context.WeeklyAvailabilities
                    .FirstOrDefaultAsync(w =>
                        w.NurseId == model.NurseId &&
                        w.DayOfWeek == dayName &&
                        w.IsActive);

                if (weeklyAvailability == null)
                    return BadRequest("The selected date is not available.");

                workingStart = weeklyAvailability.StartTime;
                workingEnd = weeklyAvailability.EndTime;
            }

            if (!workingStart.HasValue || !workingEnd.HasValue)
                return BadRequest("The selected date is not available.");

            if (model.StartTime < workingStart.Value || endTime > workingEnd.Value)
                return BadRequest("The selected time slot is outside working hours.");

            var slotAlreadyBooked = await _context.Bookings.AnyAsync(b =>
                b.NurseId == model.NurseId &&
                b.BookingDate.Date == model.Date.Date &&
                b.StartTime == model.StartTime &&
                b.EndTime == endTime &&
                b.Status != "Rejected");

            if (slotAlreadyBooked)
                return BadRequest("The selected time slot is already booked.");

            var booking = new Booking
            {
                PatientId = patientId,
                NurseId = model.NurseId,
                ServiceId = model.ServiceId,
                BookingDate = model.Date.Date,
                StartTime = model.StartTime,
                EndTime = endTime,
                ServiceAddress = model.ServiceAddress,
                AdditionalNotes = model.AdditionalNotes,
                Status = "Pending"
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            _context.Notifications.Add(new Notification
            {
                UserId = booking.NurseId,
                Title = "New Service Request",
                Message = "You have received a new service request from a patient.",
                Type = "Request",
                BookingId = booking.BookingId,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Booking request submitted successfully.",
                bookingId = booking.BookingId,
                status = booking.Status,
                summary = new
                {
                    nurseName = service.Nurse != null ? service.Nurse.FullName : null,
                    serviceName = service.ServiceCatalog.Name,
                    durationInMinutes = service.ServiceCatalog.DefaultDurationInMinutes,
                    date = booking.BookingDate.ToString("yyyy-MM-dd"),
                    time = booking.StartTime.ToString(@"hh\:mm"),
                    totalPrice = service.Price
                }
            });
        }

        [HttpGet("appointments")]
        public async Task<IActionResult> GetPatientAppointments([FromQuery] string tab = "upcoming")
        {
            var patientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (patientId == null)
                return Unauthorized();

            var query = _context.Bookings
                .Include(b => b.Nurse)
                .Include(b => b.Service)
                    .ThenInclude(s => s.ServiceCatalog)
                .Where(b => b.PatientId == patientId)
                .AsQueryable();

            if (tab.ToLower() == "upcoming")
            {
                query = query.Where(b =>
                    b.Status == "Pending" ||
                    b.Status == "Accepted" ||
                    b.Status == "Active");
            }
            else if (tab.ToLower() == "past")
            {
                query = query.Where(b =>
                    b.Status == "Cancelled" ||
                    b.Status == "Rejected" ||
                    b.Status == "Completed");
            }
            else
            {
                return BadRequest("Invalid tab value. Use 'upcoming' or 'past'.");
            }

            var bookings = await query
                .OrderBy(b => b.BookingDate)
                .ThenBy(b => b.StartTime)
                .ToListAsync();

            var nurseIds = bookings.Select(b => b.NurseId).Distinct().ToList();

            var nurseProfiles = await _context.NurseProfiles
                .Where(n => nurseIds.Contains(n.UserId))
                .ToDictionaryAsync(n => n.UserId, n => n.ProfileImagePath);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var appointments = bookings.Select(b => new
            {
                bookingId = b.BookingId,
                nurseName = b.Nurse.FullName,
                profileImageUrl = nurseProfiles.ContainsKey(b.NurseId) && nurseProfiles[b.NurseId] != null
                    ? $"{baseUrl}/{nurseProfiles[b.NurseId]}"
                    : null,
                serviceName = b.Service.ServiceCatalog.Name,
                date = b.BookingDate.ToString("yyyy-MM-dd"),
                time = b.StartTime.ToString(@"hh\:mm"),
                address = b.ServiceAddress,
                totalPrice = b.Service.Price,
                status = b.Status
            }).ToList();

            return Ok(appointments);
        }

        [HttpGet("appointments/{bookingId}")]
        public async Task<IActionResult> GetAppointmentDetails(int bookingId)
        {
            var patientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (patientId == null)
                return Unauthorized();

            var booking = await _context.Bookings
                .Include(b => b.Nurse)
                .Include(b => b.Service)
                    .ThenInclude(s => s.ServiceCatalog)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.PatientId == patientId);

            if (booking == null)
                return NotFound("Appointment not found.");

            var nurseProfile = await _context.NurseProfiles
                .FirstOrDefaultAsync(n => n.UserId == booking.NurseId);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            return Ok(new
            {
                bookingId = booking.BookingId,
                nurseName = booking.Nurse.FullName,
                profileImageUrl = nurseProfile != null && nurseProfile.ProfileImagePath != null
                    ? $"{baseUrl}/{nurseProfile.ProfileImagePath}"
                    : null,
                phoneNumber = nurseProfile?.PhoneNumber,
                serviceName = booking.Service.ServiceCatalog.Name,
                date = booking.BookingDate.ToString("yyyy-MM-dd"),
                time = booking.StartTime.ToString(@"hh\:mm"),
                address = booking.ServiceAddress,
                durationInMinutes = booking.Service.ServiceCatalog.DefaultDurationInMinutes,
                totalPrice = booking.Service.Price,
                additionalNotes = booking.AdditionalNotes,
                status = booking.Status
            });
        }

        [HttpPut("appointments/{bookingId}/cancel")]
        public async Task<IActionResult> CancelAppointment(int bookingId)
        {
            var patientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (patientId == null)
                return Unauthorized();

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.PatientId == patientId);

            if (booking == null)
                return NotFound("Appointment not found.");

            if (booking.Status != "Pending" && booking.Status != "Accepted")
                return BadRequest("Only pending or accepted appointments can be cancelled.");

            booking.Status = "Cancelled";

            _context.Notifications.Add(new Notification
            {
                UserId = booking.NurseId,
                Title = "Appointment Cancelled",
                Message = "The patient has cancelled the appointment.",
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

        [HttpPost("payments/create-intent/{bookingId}")]
        public async Task<IActionResult> CreatePaymentIntent(int bookingId)
        {
            var patientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (patientId == null)
                return Unauthorized();

            var booking = await _context.Bookings
                .Include(b => b.Service)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.PatientId == patientId);

            if (booking == null)
                return NotFound("Booking not found.");

            if (booking.Status != "Accepted")
                return BadRequest("Payment is allowed only after the booking is accepted.");

            var existingPaidPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.BookingId == bookingId && p.Status == "Paid");

            if (existingPaidPayment != null)
                return BadRequest("This booking has already been paid.");

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(booking.Service.Price * 100),
                Currency = "usd",
                Metadata = new Dictionary<string, string>
                {
                    { "bookingId", bookingId.ToString() },
                    { "patientId", patientId }
                }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            return Ok(new
            {
                clientSecret = paymentIntent.ClientSecret
            });
        }

        [HttpPost("payments/confirm/{bookingId}")]
        public async Task<IActionResult> ConfirmPayment(int bookingId)
        {
            var patientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (patientId == null)
                return Unauthorized();

            var booking = await _context.Bookings
                .Include(b => b.Service)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.PatientId == patientId);

            if (booking == null)
                return NotFound("Booking not found.");

            var existingPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.BookingId == bookingId);

            if (existingPayment != null && existingPayment.Status == "Paid")
                return BadRequest("Payment already confirmed.");

            if (existingPayment == null)
            {
                var payment = new Payment
                {
                    BookingId = bookingId,
                    Amount = booking.Service.Price,
                    Status = "Paid"
                };

                _context.Payments.Add(payment);
            }
            else
            {
                existingPayment.Status = "Paid";
                existingPayment.Amount = booking.Service.Price;
            }

            booking.Status = "Active";

            _context.Notifications.Add(new Notification
            {
                UserId = booking.PatientId,
                Title = "Payment Successful",
                Message = "Your payment was completed successfully.",
                Type = "Payment",
                BookingId = booking.BookingId,
                CreatedAt = DateTime.UtcNow
            });

            _context.Notifications.Add(new Notification
            {
                UserId = booking.NurseId,
                Title = "Payment Received",
                Message = "The patient has completed the payment for the appointment.",
                Type = "Payment",
                BookingId = booking.BookingId,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Payment confirmed successfully.",
                bookingId = booking.BookingId,
                amountPaid = booking.Service.Price,
                paymentStatus = "Paid",
                bookingStatus = booking.Status
            });
        }

        [HttpGet("payments/{bookingId}")]
        public async Task<IActionResult> GetPayment(int bookingId)
        {
            var patientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (patientId == null)
                return Unauthorized();

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.PatientId == patientId);

            if (booking == null)
                return NotFound("Booking not found.");

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.BookingId == bookingId);

            if (payment == null)
                return NotFound("Payment not found.");

            return Ok(new
            {
                paymentId = payment.PaymentId,
                bookingId = payment.BookingId,
                amount = payment.Amount,
                status = payment.Status,
                createdAt = payment.CreatedAt
            });
        }

        [HttpGet("payments/summary/{bookingId}")]
        public async Task<IActionResult> GetPaymentSummary(int bookingId)
        {
            var patientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (patientId == null)
                return Unauthorized();

            var booking = await _context.Bookings
                .Include(b => b.Nurse)
                .Include(b => b.Service)
                    .ThenInclude(s => s.ServiceCatalog)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.PatientId == patientId);

            if (booking == null)
                return NotFound("Booking not found.");

            return Ok(new
            {
                nurseName = booking.Nurse.FullName,
                serviceName = booking.Service.ServiceCatalog.Name,
                date = booking.BookingDate.ToString("yyyy-MM-dd"),
                time = booking.StartTime.ToString(@"hh\:mm"),
                amount = booking.Service.Price
            });
        }
    }
}