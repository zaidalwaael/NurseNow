using NurseNow.Domain.Rules;
using Microsoft.EntityFrameworkCore;
using NurseNow.DTOs;
using NurseNow.Models;
using System.Security.Claims;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features;
public sealed class PatientService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IApplicationDbContext _context;
    private readonly IPaymentGateway _paymentGateway;
    public PatientService(IApplicationDbContext context, IPaymentGateway paymentGateway, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _context = context;
        _paymentGateway = paymentGateway;
    }

    public async Task<OperationResult> BrowseNurses(BrowseNursesQueryDto query)
    {
        if (query.PageNumber < 1 || query.PageNumber > 100000 || query.PageSize < 1 || query.PageSize > 100)
            return BadRequest("Invalid pagination. PageSize must be 1–100.");
        var nursesQuery = _context.NurseProfiles.AsNoTracking().Include(n => n.User).Where(n => n.VerificationStatus == "Approved").AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            nursesQuery = nursesQuery.Where(n => n.User.FullName.ToLower().Contains(search) || (n.Specialization != null && n.Specialization.ToLower().Contains(search)) || (n.Location != null && n.Location.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(query.Location))
        {
            var location = query.Location.Trim().ToLower();
            nursesQuery = nursesQuery.Where(n => n.Location != null && n.Location.ToLower() == location);
        }

        if (query.ServiceCatalogId.HasValue)
        {
            nursesQuery = nursesQuery.Where(n => _context.Services.AsNoTracking().Any(s => s.NurseId == n.UserId && s.ServiceCatalogId == query.ServiceCatalogId.Value));
        }

        if (!string.IsNullOrWhiteSpace(query.Location))
        {
            nursesQuery = nursesQuery.OrderBy(n => n.Address).ThenBy(n => n.UserId);
        }
        else
        {
            nursesQuery = nursesQuery.OrderByDescending(n => n.ExperienceYears).ThenBy(n => n.UserId);
        }

        var totalCount = await nursesQuery.CountAsync(_requestContext.CancellationToken);
        var baseUrl = _requestContext.PublicBaseUrl;
        var nurses = await nursesQuery.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize).Select(n => new { nurseId = n.UserId, fullName = n.User.FullName, specialization = n.Specialization, location = n.Location, address = n.Address, experienceYears = n.ExperienceYears, profileImageUrl = n.ProfileImagePath != null ? $"{baseUrl}/{n.ProfileImagePath}" : null, rating = Math.Round(_context.Reviews.AsNoTracking().Where(r => r.NurseId == n.UserId).Select(r => (double? )r.Rating).Average() ?? 0.0, 1), reviewsCount = _context.Reviews.AsNoTracking().Count(r => r.NurseId == n.UserId), price = query.ServiceCatalogId.HasValue ? _context.Services.AsNoTracking().Where(s => s.NurseId == n.UserId && s.ServiceCatalogId == query.ServiceCatalogId.Value).Select(s => (decimal? )s.Price).FirstOrDefault() : _context.Services.AsNoTracking().Where(s => s.NurseId == n.UserId).Select(s => (decimal? )s.Price).Min(), availabilityLabel = _context.WeeklyAvailabilities.AsNoTracking().Any(w => w.NurseId == n.UserId && w.IsActive) ? "Available This Week" : "Unavailable" }).ToListAsync(_requestContext.CancellationToken);
        return Ok(new { pageNumber = query.PageNumber, pageSize = query.PageSize, totalCount, totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize), hasNextPage = query.PageNumber * query.PageSize < totalCount, items = nurses });
    }

    public async Task<OperationResult> GetNurseDetails(string nurseId)
    {
        var nurseProfile = await _context.NurseProfiles.AsNoTracking().Include(n => n.User).FirstOrDefaultAsync(n => n.UserId == nurseId && n.VerificationStatus == "Approved", _requestContext.CancellationToken);
        if (nurseProfile == null)
            return NotFound("Nurse not found.");
        var services = await _context.Services.AsNoTracking().Include(s => s.ServiceCatalog).Where(s => s.NurseId == nurseId).Select(s => s.ServiceCatalog.Name).ToListAsync(_requestContext.CancellationToken);
        var baseUrl = _requestContext.PublicBaseUrl;
        var today = DateTime.Today;
        var todayDayName = today.DayOfWeek.ToString();
        var hasOverrideToday = await _context.AvailabilityOverrides.AsNoTracking().AnyAsync(o => o.NurseId == nurseId && o.Date.Date == today.Date && !o.IsBlocked, _requestContext.CancellationToken);
        var isBlockedToday = await _context.AvailabilityOverrides.AsNoTracking().AnyAsync(o => o.NurseId == nurseId && o.Date.Date == today.Date && o.IsBlocked, _requestContext.CancellationToken);
        var hasWeeklyToday = await _context.WeeklyAvailabilities.AsNoTracking().AnyAsync(w => w.NurseId == nurseId && w.DayOfWeek == todayDayName && w.IsActive, _requestContext.CancellationToken);
        var hasAnyWeeklyThisWeek = await _context.WeeklyAvailabilities.AsNoTracking().AnyAsync(w => w.NurseId == nurseId && w.IsActive, _requestContext.CancellationToken);
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
        return Ok(new { nurseId = nurseProfile.UserId, fullName = nurseProfile.User.FullName, profileImageUrl = nurseProfile.ProfileImagePath != null ? $"{baseUrl}/{nurseProfile.ProfileImagePath}" : null, headline, rating = Math.Round(await _context.Reviews.AsNoTracking().Where(r => r.NurseId == nurseProfile.UserId).Select(r => (double? )r.Rating).AverageAsync(_requestContext.CancellationToken) ?? 0.0, 1), reviewsCount = await _context.Reviews.AsNoTracking().CountAsync(r => r.NurseId == nurseProfile.UserId, _requestContext.CancellationToken), experienceYears = nurseProfile.ExperienceYears, location = nurseProfile.Location, address = nurseProfile.Address, availabilityLabel, certificateUrl = nurseProfile.CertificatePath != null ? $"{baseUrl}/{nurseProfile.CertificatePath}" : null, servicesOffered = services });
    }

    public async Task<OperationResult> GetNurseServices(string nurseId)
    {
        var nurseExists = await _context.NurseProfiles.AsNoTracking().AnyAsync(n => n.UserId == nurseId && n.VerificationStatus == "Approved", _requestContext.CancellationToken);
        if (!nurseExists)
            return NotFound("Nurse not found.");
        var services = await _context.Services.AsNoTracking().Include(s => s.ServiceCatalog).Where(s => s.NurseId == nurseId).Select(s => new { serviceId = s.ServiceId, serviceCatalogId = s.ServiceCatalogId, serviceName = s.ServiceCatalog.Name, durationInMinutes = s.ServiceCatalog.DefaultDurationInMinutes, price = s.Price }).ToListAsync(_requestContext.CancellationToken);
        return Ok(services);
    }

    public async Task<OperationResult> GetAvailableDates(string nurseId, int daysAhead = 14)
    {
        var nurseExists = await _context.NurseProfiles.AsNoTracking().AnyAsync(n => n.UserId == nurseId && n.VerificationStatus == "Approved", _requestContext.CancellationToken);
        if (!nurseExists)
            return NotFound("Nurse not found.");
        if (daysAhead <= 0 || daysAhead > 90)
            return BadRequest("daysAhead must be between 1 and 90.");
        var availableDates = new List<string>();
        var today = DateTime.Today;
        var endDate = today.AddDays(daysAhead);
        var overrides = await _context.AvailabilityOverrides.AsNoTracking().Where(o => o.NurseId == nurseId && o.Date >= today && o.Date < endDate).OrderBy(o => o.AvailabilityOverrideId).ToListAsync(_requestContext.CancellationToken);
        var weeklyDays = await _context.WeeklyAvailabilities.AsNoTracking().Where(w => w.NurseId == nurseId && w.IsActive && w.StartTime < w.EndTime).Select(w => w.DayOfWeek).Distinct().ToListAsync(_requestContext.CancellationToken);
        var activeDays = weeklyDays.ToHashSet(StringComparer.Ordinal);
        for (int i = 0; i < daysAhead; i++)
        {
            var currentDate = today.AddDays(i);
            var dayName = currentDate.DayOfWeek.ToString();
            var overrideRecord = overrides.FirstOrDefault(o => o.Date.Date == currentDate);
            if (overrideRecord != null && overrideRecord.IsBlocked)
                continue;
            if (overrideRecord != null && !overrideRecord.IsBlocked && overrideRecord.StartTime.HasValue && overrideRecord.EndTime.HasValue && overrideRecord.StartTime.Value < overrideRecord.EndTime.Value)
            {
                availableDates.Add(currentDate.ToString("yyyy-MM-dd"));
                continue;
            }

            var hasWeeklyAvailability = activeDays.Contains(dayName);
            if (hasWeeklyAvailability)
            {
                availableDates.Add(currentDate.ToString("yyyy-MM-dd"));
            }
        }

        return Ok(availableDates);
    }

    public async Task<OperationResult> GetAvailableSlots(string nurseId, int serviceId, DateTime date)
    {
        var nurseExists = await _context.NurseProfiles.AsNoTracking().AnyAsync(n => n.UserId == nurseId && n.VerificationStatus == "Approved", _requestContext.CancellationToken);
        if (!nurseExists)
            return NotFound("Nurse not found.");
        var service = await _context.Services.AsNoTracking().Include(s => s.ServiceCatalog).FirstOrDefaultAsync(s => s.ServiceId == serviceId && s.NurseId == nurseId, _requestContext.CancellationToken);
        if (service == null)
            return NotFound("Service not found for this nurse.");
        var serviceDuration = service.ServiceCatalog.DefaultDurationInMinutes;
        if (serviceDuration <= 0 || serviceDuration > 1440)
            return BadRequest("Service duration is invalid.");
        TimeSpan? startTime = null;
        TimeSpan? endTime = null;
        var overrideRecord = await _context.AvailabilityOverrides.AsNoTracking().FirstOrDefaultAsync(o => o.NurseId == nurseId && o.Date.Date == date.Date, _requestContext.CancellationToken);
        if (overrideRecord != null && overrideRecord.IsBlocked)
            return Ok(new List<string>());
        if (overrideRecord != null && !overrideRecord.IsBlocked && overrideRecord.StartTime.HasValue && overrideRecord.EndTime.HasValue)
        {
            startTime = overrideRecord.StartTime.Value;
            endTime = overrideRecord.EndTime.Value;
        }
        else
        {
            var dayName = date.DayOfWeek.ToString();
            var weeklyAvailability = await _context.WeeklyAvailabilities.AsNoTracking().FirstOrDefaultAsync(w => w.NurseId == nurseId && w.DayOfWeek == dayName && w.IsActive, _requestContext.CancellationToken);
            if (weeklyAvailability == null)
                return Ok(new List<string>());
            startTime = weeklyAvailability.StartTime;
            endTime = weeklyAvailability.EndTime;
        }

        if (!startTime.HasValue || !endTime.HasValue || startTime.Value >= endTime.Value)
            return Ok(new List<string>());
        var day = date.Date;
        var nextDay = day.AddDays(1);
        var reservations = await _context.Bookings.AsNoTracking().Where(b => b.NurseId == nurseId && b.BookingDate >= day && b.BookingDate < nextDay && b.Status != "Rejected" && b.Status != "Cancelled").Select(b => new { b.StartTime, b.EndTime }).ToListAsync(_requestContext.CancellationToken);
        var slots = new List<string>();
        var current = startTime.Value;
        var duration = TimeSpan.FromMinutes(serviceDuration);
        while (current + duration <= endTime.Value)
        {
            if (!reservations.Any(b => BookingRules.Overlaps(current, current + duration, b.StartTime, b.EndTime)))
                slots.Add(current.ToString(@"hh\:mm"));
            current = current.Add(duration);
        }

        return Ok(slots);
    }

    public async Task<OperationResult> CreateBookingRequest(CreateBookingRequestDto model)
    {
        await using var transaction = await _context.BeginTransactionAsync(_requestContext.CancellationToken);
        if (model.Date.Date < DateTime.Today || model.StartTime < TimeSpan.Zero || model.StartTime >= TimeSpan.FromDays(1))
            return BadRequest("Invalid booking date or time.");
        var patientId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (patientId == null)
            return Unauthorized();
        var nurseExists = await _context.NurseProfiles.AnyAsync(n => n.UserId == model.NurseId && n.VerificationStatus == "Approved", _requestContext.CancellationToken);
        if (!nurseExists)
            return BadRequest("Selected nurse is not available.");
        var service = await _context.Services.Include(s => s.ServiceCatalog).Include(s => s.Nurse).FirstOrDefaultAsync(s => s.ServiceId == model.ServiceId && s.NurseId == model.NurseId, _requestContext.CancellationToken);
        if (service == null)
            return BadRequest("Selected service is invalid.");
        if (string.IsNullOrWhiteSpace(model.ServiceAddress))
            return BadRequest("Service address is required.");
        var duration = TimeSpan.FromMinutes(service.ServiceCatalog.DefaultDurationInMinutes);
        var endTime = model.StartTime.Add(duration);
        if (duration <= TimeSpan.Zero || endTime > TimeSpan.FromDays(1))
            return BadRequest("Invalid service duration.");
        var overrideRecord = await _context.AvailabilityOverrides.FirstOrDefaultAsync(o => o.NurseId == model.NurseId && o.Date.Date == model.Date.Date, _requestContext.CancellationToken);
        if (overrideRecord != null && overrideRecord.IsBlocked)
            return BadRequest("The selected date is not available.");
        TimeSpan? workingStart = null;
        TimeSpan? workingEnd = null;
        if (overrideRecord != null && !overrideRecord.IsBlocked && overrideRecord.StartTime.HasValue && overrideRecord.EndTime.HasValue)
        {
            workingStart = overrideRecord.StartTime.Value;
            workingEnd = overrideRecord.EndTime.Value;
        }
        else
        {
            var dayName = model.Date.DayOfWeek.ToString();
            var weeklyAvailability = await _context.WeeklyAvailabilities.FirstOrDefaultAsync(w => w.NurseId == model.NurseId && w.DayOfWeek == dayName && w.IsActive, _requestContext.CancellationToken);
            if (weeklyAvailability == null)
                return BadRequest("The selected date is not available.");
            workingStart = weeklyAvailability.StartTime;
            workingEnd = weeklyAvailability.EndTime;
        }

        if (!workingStart.HasValue || !workingEnd.HasValue)
            return BadRequest("The selected date is not available.");
        if (model.StartTime < workingStart.Value || endTime > workingEnd.Value)
            return BadRequest("The selected time slot is outside working hours.");
        var slotAlreadyBooked = await _context.Bookings.AnyAsync(b => b.NurseId == model.NurseId && b.BookingDate.Date == model.Date.Date && b.StartTime < endTime && b.EndTime > model.StartTime && b.Status != "Rejected" && b.Status != "Cancelled", _requestContext.CancellationToken);
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
        await _context.SaveChangesAsync(_requestContext.CancellationToken);
        _context.Notifications.Add(new Notification { UserId = booking.NurseId, Title = "New Service Request", Message = "You have received a new service request from a patient.", Type = "Request", BookingId = booking.BookingId, CreatedAt = DateTime.UtcNow });
        _context.AdminActivityLogs.Add(new AdminActivityLog { Title = "New service request submitted", Description = $"A new booking request was submitted by patient ID {booking.PatientId}.", ActivityType = "Booking", CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync(_requestContext.CancellationToken);
        await transaction.CommitAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Booking request submitted successfully.", bookingId = booking.BookingId, status = booking.Status, summary = new { nurseName = service.Nurse != null ? service.Nurse.FullName : null, serviceName = service.ServiceCatalog.Name, durationInMinutes = service.ServiceCatalog.DefaultDurationInMinutes, date = booking.BookingDate.ToString("yyyy-MM-dd"), time = booking.StartTime.ToString(@"hh\:mm"), totalPrice = service.Price } });
    }

    public async Task<OperationResult> GetPatientAppointments(string tab = "upcoming", int? limit = null)
    {
        var patientId = _requestContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (patientId == null)
            return Unauthorized();
        var query = _context.Bookings.AsNoTracking().Include(b => b.Nurse).Include(b => b.Service).ThenInclude(s => s.ServiceCatalog).Where(b => b.PatientId == patientId).AsQueryable();
        if (tab.ToLower() == "upcoming")
        {
            query = query.Where(b => b.Status == "Pending" || b.Status == "Accepted" || b.Status == "Active");
        }
        else if (tab.ToLower() == "past")
        {
            query = query.Where(b => b.Status == "Cancelled" || b.Status == "Rejected" || b.Status == "Completed");
        }
        else
        {
            return BadRequest("Invalid tab value. Use 'upcoming' or 'past'.");
        }

        query = query.OrderBy(b => b.BookingDate).ThenBy(b => b.StartTime);
        if (limit.HasValue && limit.Value > 0)
        {
            query = query.Take(Math.Clamp(limit.Value, 1, 100));
        }

        var bookings = await query.ToListAsync(_requestContext.CancellationToken);
        var nurseIds = bookings.Select(b => b.NurseId).Distinct().ToList();
        var nurseProfiles = await _context.NurseProfiles.AsNoTracking().Where(n => nurseIds.Contains(n.UserId)).ToDictionaryAsync(n => n.UserId, n => n.ProfileImagePath);
        var baseUrl = _requestContext.PublicBaseUrl;
        var appointments = bookings.Select(b => new { bookingId = b.BookingId, nurseName = b.Nurse.FullName, profileImageUrl = nurseProfiles.ContainsKey(b.NurseId) && nurseProfiles[b.NurseId] != null ? $"{baseUrl}/{nurseProfiles[b.NurseId]}" : null, serviceName = b.Service.ServiceCatalog.Name, date = b.BookingDate.ToString("yyyy-MM-dd"), time = b.StartTime.ToString(@"hh\:mm"), address = b.ServiceAddress, totalPrice = b.Service.Price, status = b.Status }).ToList();
        return Ok(appointments);
    }

    public async Task<OperationResult> GetAppointmentDetails(int bookingId)
    {
        var patientId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (patientId == null)
            return Unauthorized();
        var booking = await _context.Bookings.AsNoTracking().Include(b => b.Nurse).Include(b => b.Service).ThenInclude(s => s.ServiceCatalog).FirstOrDefaultAsync(b => b.BookingId == bookingId && b.PatientId == patientId, _requestContext.CancellationToken);
        if (booking == null)
            return NotFound("Appointment not found.");
        var nurseProfile = await _context.NurseProfiles.AsNoTracking().FirstOrDefaultAsync(n => n.UserId == booking.NurseId, _requestContext.CancellationToken);
        var baseUrl = _requestContext.PublicBaseUrl;
        return Ok(new { bookingId = booking.BookingId, nurseName = booking.Nurse.FullName, profileImageUrl = nurseProfile != null && nurseProfile.ProfileImagePath != null ? $"{baseUrl}/{nurseProfile.ProfileImagePath}" : null, phoneNumber = nurseProfile?.PhoneNumber, serviceName = booking.Service.ServiceCatalog.Name, date = booking.BookingDate.ToString("yyyy-MM-dd"), time = booking.StartTime.ToString(@"hh\:mm"), address = booking.ServiceAddress, durationInMinutes = booking.Service.ServiceCatalog.DefaultDurationInMinutes, totalPrice = booking.Service.Price, additionalNotes = booking.AdditionalNotes, status = booking.Status });
    }

    public async Task<OperationResult> CancelAppointment(int bookingId)
    {
        var patientId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (patientId == null)
            return Unauthorized();
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId && b.PatientId == patientId, _requestContext.CancellationToken);
        if (booking == null)
            return NotFound("Appointment not found.");
        if (booking.Status != "Pending" && booking.Status != "Accepted")
            return BadRequest("Only pending or accepted appointments can be cancelled.");
        booking.Status = "Cancelled";
        _context.Notifications.Add(new Notification { UserId = booking.NurseId, Title = "Appointment Cancelled", Message = "The patient has cancelled the appointment.", Type = "Booking", BookingId = booking.BookingId, CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Appointment cancelled successfully.", status = booking.Status });
    }

    public async Task<OperationResult> CreatePaymentIntent(int bookingId)
    {
        var patientId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (patientId == null)
            return Unauthorized();
        var booking = await _context.Bookings.Include(b => b.Service).FirstOrDefaultAsync(b => b.BookingId == bookingId && b.PatientId == patientId, _requestContext.CancellationToken);
        if (booking == null)
            return NotFound("Booking not found.");
        if (booking.Status != "Accepted")
            return BadRequest("Payment is allowed only after the booking is accepted.");
        var existingPaidPayment = await _context.Payments.FirstOrDefaultAsync(p => p.BookingId == bookingId && p.Status == "Paid", _requestContext.CancellationToken);
        if (existingPaidPayment != null)
            return BadRequest("This booking has already been paid.");
        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.BookingId == bookingId, _requestContext.CancellationToken);
        var amount = payment?.Amount ?? booking.Service.Price;
        var paymentIntent = payment?.StripePaymentIntentId is { } existingIntentId ? await _paymentGateway.GetAsync(existingIntentId, _requestContext.CancellationToken) : await _paymentGateway.CreateAsync(bookingId, patientId, amount, _requestContext.CancellationToken);
        if (payment is null)
        {
            payment = new Payment
            {
                BookingId = bookingId,
                Amount = amount,
                Status = "Pending",
                PaymentMethod = "Card"
            };
            _context.Payments.Add(payment);
        }

        payment.StripePaymentIntentId = paymentIntent.Id;
        await _context.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { clientSecret = paymentIntent.ClientSecret });
    }

    public async Task<OperationResult> ConfirmPayment(int bookingId)
    {
        var patientId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (patientId == null)
            return Unauthorized();
        var booking = await _context.Bookings.Include(b => b.Service).FirstOrDefaultAsync(b => b.BookingId == bookingId && b.PatientId == patientId, _requestContext.CancellationToken);
        if (booking == null)
            return NotFound("Booking not found.");
        if (booking.Status != "Accepted")
            return BadRequest("Booking is not awaiting payment.");
        var existingPayment = await _context.Payments.FirstOrDefaultAsync(p => p.BookingId == bookingId, _requestContext.CancellationToken);
        if (existingPayment != null && existingPayment.Status == "Paid")
            return BadRequest("Payment already confirmed.");
        if (existingPayment?.StripePaymentIntentId is null)
            return BadRequest("Create a payment intent before confirming payment.");
        if (!await _paymentGateway.IsPaidAsync(existingPayment.StripePaymentIntentId, bookingId, patientId, existingPayment.Amount, _requestContext.CancellationToken))
            return BadRequest("Payment has not been verified by the payment provider.");
        existingPayment.Status = "Paid";
        existingPayment.PaymentMethod = "Card";
        booking.Status = "Active";
        _context.Notifications.Add(new Notification { UserId = booking.PatientId, Title = "Payment Successful", Message = "Your payment was completed successfully.", Type = "Payment", BookingId = booking.BookingId, CreatedAt = DateTime.UtcNow });
        _context.Notifications.Add(new Notification { UserId = booking.NurseId, Title = "Payment Received", Message = "The patient has completed the payment for the appointment.", Type = "Payment", BookingId = booking.BookingId, CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Payment confirmed successfully.", bookingId = booking.BookingId, amountPaid = existingPayment.Amount, paymentStatus = "Paid", bookingStatus = booking.Status });
    }

    public async Task<OperationResult> GetPayment(int bookingId)
    {
        var patientId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (patientId == null)
            return Unauthorized();
        var booking = await _context.Bookings.AsNoTracking().FirstOrDefaultAsync(b => b.BookingId == bookingId && b.PatientId == patientId, _requestContext.CancellationToken);
        if (booking == null)
            return NotFound("Booking not found.");
        var payment = await _context.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.BookingId == bookingId, _requestContext.CancellationToken);
        if (payment == null)
            return NotFound("Payment not found.");
        return Ok(new { paymentId = payment.PaymentId, bookingId = payment.BookingId, amount = payment.Amount, status = payment.Status, createdAt = payment.CreatedAt });
    }

    public async Task<OperationResult> GetPaymentSummary(int bookingId)
    {
        var patientId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (patientId == null)
            return Unauthorized();
        var booking = await _context.Bookings.AsNoTracking().Include(b => b.Nurse).Include(b => b.Service).ThenInclude(s => s.ServiceCatalog).FirstOrDefaultAsync(b => b.BookingId == bookingId && b.PatientId == patientId, _requestContext.CancellationToken);
        if (booking == null)
            return NotFound("Booking not found.");
        return Ok(new { nurseName = booking.Nurse.FullName, serviceName = booking.Service.ServiceCatalog.Name, date = booking.BookingDate.ToString("yyyy-MM-dd"), time = booking.StartTime.ToString(@"hh\:mm"), amount = booking.Service.Price });
    }

    public async Task<OperationResult> GetNurseReviews(string nurseId)
    {
        var nurseExists = await _context.NurseProfiles.AsNoTracking().AnyAsync(n => n.UserId == nurseId && n.VerificationStatus == "Approved", _requestContext.CancellationToken);
        if (!nurseExists)
            return NotFound("Nurse not found.");
        var reviews = await _context.Reviews.AsNoTracking().Include(r => r.Patient).Where(r => r.NurseId == nurseId).OrderByDescending(r => r.CreatedAt).Select(r => new { reviewId = r.ReviewId, patientName = r.Patient.FullName, rating = r.Rating, comment = r.Comment, createdAt = r.CreatedAt }).ToListAsync(_requestContext.CancellationToken);
        var averageRating = reviews.Any() ? Math.Round(reviews.Average(r => (double)r.rating), 1) : 0.0;
        var reviewsCount = reviews.Count;
        return Ok(new { averageRating, reviewsCount, reviews });
    }

    public async Task<OperationResult> GetPatientDashboardSummary()
    {
        var patientId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(patientId))
            return Unauthorized();
        var totalBookings = await _context.Bookings.AsNoTracking().CountAsync(b => b.PatientId == patientId, _requestContext.CancellationToken);
        var activeRequests = await _context.Bookings.AsNoTracking().CountAsync(b => b.PatientId == patientId && (b.Status == "Pending" || b.Status == "Accepted" || b.Status == "Active"), _requestContext.CancellationToken);
        return Ok(new { totalBookings, activeRequests });
    }
}
