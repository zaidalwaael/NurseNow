using NurseNow.Application.Repositories;
using NurseNow.Domain.Rules;
using NurseNow.DTOs;
using NurseNow.Models;
using System.Security.Claims;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features;
public sealed class PatientService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INurseProfileRepository _nurseProfiles;
    private readonly IReviewRepository _reviews;
    private readonly IWeeklyAvailabilityRepository _weeklyAvailabilities;
    private readonly IAvailabilityOverrideRepository _availabilityOverrides;
    private readonly IServiceRepository _services;
    private readonly IBookingRepository _bookings;
    private readonly IPaymentRepository _payments;
    private readonly INotificationRepository _notifications;
    private readonly IAdminActivityLogRepository _adminActivityLogs;
    private readonly IPaymentGateway _paymentGateway;
    public PatientService(IUnitOfWork unitOfWork, INurseProfileRepository nurseProfiles, IReviewRepository reviews, IWeeklyAvailabilityRepository weeklyAvailabilities, IAvailabilityOverrideRepository availabilityOverrides, IServiceRepository services, IBookingRepository bookings, IPaymentRepository payments, INotificationRepository notifications, IAdminActivityLogRepository adminActivityLogs, IPaymentGateway paymentGateway, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _unitOfWork = unitOfWork;
        _nurseProfiles = nurseProfiles;
        _reviews = reviews;
        _weeklyAvailabilities = weeklyAvailabilities;
        _availabilityOverrides = availabilityOverrides;
        _services = services;
        _bookings = bookings;
        _payments = payments;
        _notifications = notifications;
        _adminActivityLogs = adminActivityLogs;
        _paymentGateway = paymentGateway;
    }

    public async Task<OperationResult> BrowseNurses(BrowseNursesQueryDto query)
    {
        if (query.PageNumber < 1 || query.PageNumber > 100000 || query.PageSize < 1 || query.PageSize > 100)
            return BadRequest("Invalid pagination. PageSize must be 1–100.");
        var (totalCount, nurses) = await _nurseProfiles.SearchApprovedAsync(query, _requestContext.PublicBaseUrl, _requestContext.CancellationToken);
        return Ok(new { pageNumber = query.PageNumber, pageSize = query.PageSize, totalCount, totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize), hasNextPage = query.PageNumber * query.PageSize < totalCount, items = nurses });
    }

    public async Task<OperationResult> GetNurseDetails(string nurseId)
    {
        var nurseProfile = await _nurseProfiles.GetApprovedWithUserAsync(nurseId, _requestContext.CancellationToken);
        if (nurseProfile == null)
            return NotFound("Nurse not found.");
        var services = await _services.ListServiceNamesAsync(nurseId, _requestContext.CancellationToken);
        var baseUrl = _requestContext.PublicBaseUrl;
        var today = DateTime.Today;
        var todayDayName = today.DayOfWeek.ToString();
        var hasOverrideToday = await _availabilityOverrides.HasAvailableOverrideAsync(nurseId, today, _requestContext.CancellationToken);
        var isBlockedToday = await _availabilityOverrides.IsBlockedAsync(nurseId, today, _requestContext.CancellationToken);
        var hasWeeklyToday = await _weeklyAvailabilities.HasActiveDayAsync(nurseId, todayDayName, _requestContext.CancellationToken);
        var hasAnyWeeklyThisWeek = await _weeklyAvailabilities.HasActiveScheduleAsync(nurseId, _requestContext.CancellationToken);
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
        return Ok(new { nurseId = nurseProfile.UserId, fullName = nurseProfile.User.FullName, profileImageUrl = nurseProfile.ProfileImagePath != null ? $"{baseUrl}/{nurseProfile.ProfileImagePath}" : null, headline, rating = Math.Round(await _reviews.GetAverageRatingAsync(nurseProfile, _requestContext.CancellationToken) ?? 0.0, 1), reviewsCount = await _reviews.CountForNurseAsync(nurseProfile, _requestContext.CancellationToken), experienceYears = nurseProfile.ExperienceYears, location = nurseProfile.Location, address = nurseProfile.Address, availabilityLabel, certificateUrl = nurseProfile.CertificatePath != null ? $"{baseUrl}/{nurseProfile.CertificatePath}" : null, servicesOffered = services });
    }

    public async Task<OperationResult> GetNurseServices(string nurseId)
    {
        var nurseExists = await _nurseProfiles.IsApprovedAsync(nurseId, _requestContext.CancellationToken);
        if (!nurseExists)
            return NotFound("Nurse not found.");
        var services = await _services.ListPublicServicesAsync(nurseId, _requestContext.CancellationToken);
        return Ok(services);
    }

    public async Task<OperationResult> GetAvailableDates(string nurseId, int daysAhead = 14)
    {
        var nurseExists = await _nurseProfiles.IsApprovedAsync(nurseId, _requestContext.CancellationToken);
        if (!nurseExists)
            return NotFound("Nurse not found.");
        if (daysAhead <= 0 || daysAhead > 90)
            return BadRequest("daysAhead must be between 1 and 90.");
        var availableDates = new List<string>();
        var today = DateTime.Today;
        var endDate = today.AddDays(daysAhead);
        var overrides = await _availabilityOverrides.ListInRangeAsync(nurseId, today, endDate, _requestContext.CancellationToken);
        var weeklyDays = await _weeklyAvailabilities.ListActiveDaysAsync(nurseId, _requestContext.CancellationToken);
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
        var nurseExists = await _nurseProfiles.IsApprovedAsync(nurseId, _requestContext.CancellationToken);
        if (!nurseExists)
            return NotFound("Nurse not found.");
        var service = await _services.GetForNurseAsync(nurseId, serviceId, _requestContext.CancellationToken);
        if (service == null)
            return NotFound("Service not found for this nurse.");
        var serviceDuration = service.ServiceCatalog.DefaultDurationInMinutes;
        if (serviceDuration <= 0 || serviceDuration > 1440)
            return BadRequest("Service duration is invalid.");
        TimeSpan? startTime = null;
        TimeSpan? endTime = null;
        var overrideRecord = await _availabilityOverrides.GetForDateAsync(nurseId, date, _requestContext.CancellationToken);
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
            var weeklyAvailability = await _weeklyAvailabilities.GetActiveDayAsync(nurseId, dayName, _requestContext.CancellationToken);
            if (weeklyAvailability == null)
                return Ok(new List<string>());
            startTime = weeklyAvailability.StartTime;
            endTime = weeklyAvailability.EndTime;
        }

        if (!startTime.HasValue || !endTime.HasValue || startTime.Value >= endTime.Value)
            return Ok(new List<string>());
        var day = date.Date;
        var nextDay = day.AddDays(1);
        var reservations = await _bookings.ListReservedSlotsAsync(nurseId, day, nextDay, _requestContext.CancellationToken);
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
        await using var transaction = await _unitOfWork.BeginTransactionAsync(_requestContext.CancellationToken);
        if (model.Date.Date < DateTime.Today || model.StartTime < TimeSpan.Zero || model.StartTime >= TimeSpan.FromDays(1))
            return BadRequest("Invalid booking date or time.");
        var patientId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (patientId == null)
            return Unauthorized();
        var nurseExists = await _nurseProfiles.IsBookingNurseApprovedAsync(model, _requestContext.CancellationToken);
        if (!nurseExists)
            return BadRequest("Selected nurse is not available.");
        var service = await _services.GetBookingServiceAsync(model, _requestContext.CancellationToken);
        if (service == null)
            return BadRequest("Selected service is invalid.");
        if (string.IsNullOrWhiteSpace(model.ServiceAddress))
            return BadRequest("Service address is required.");
        var duration = TimeSpan.FromMinutes(service.ServiceCatalog.DefaultDurationInMinutes);
        var endTime = model.StartTime.Add(duration);
        if (duration <= TimeSpan.Zero || endTime > TimeSpan.FromDays(1))
            return BadRequest("Invalid service duration.");
        var overrideRecord = await _availabilityOverrides.GetBookingOverrideAsync(model, _requestContext.CancellationToken);
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
            var weeklyAvailability = await _weeklyAvailabilities.GetBookingScheduleAsync(model, dayName, _requestContext.CancellationToken);
            if (weeklyAvailability == null)
                return BadRequest("The selected date is not available.");
            workingStart = weeklyAvailability.StartTime;
            workingEnd = weeklyAvailability.EndTime;
        }

        if (!workingStart.HasValue || !workingEnd.HasValue)
            return BadRequest("The selected date is not available.");
        if (model.StartTime < workingStart.Value || endTime > workingEnd.Value)
            return BadRequest("The selected time slot is outside working hours.");
        var slotAlreadyBooked = await _bookings.HasOverlapAsync(model, endTime, _requestContext.CancellationToken);
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
        _bookings.Add(booking);
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        _notifications.Add(new Notification { UserId = booking.NurseId, Title = "New Service Request", Message = "You have received a new service request from a patient.", Type = "Request", BookingId = booking.BookingId, CreatedAt = DateTime.UtcNow });
        _adminActivityLogs.Add(new AdminActivityLog { Title = "New service request submitted", Description = $"A new booking request was submitted by patient ID {booking.PatientId}.", ActivityType = "Booking", CreatedAt = DateTime.UtcNow });
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        await transaction.CommitAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Booking request submitted successfully.", bookingId = booking.BookingId, status = booking.Status, summary = new { nurseName = service.Nurse != null ? service.Nurse.FullName : null, serviceName = service.ServiceCatalog.Name, durationInMinutes = service.ServiceCatalog.DefaultDurationInMinutes, date = booking.BookingDate.ToString("yyyy-MM-dd"), time = booking.StartTime.ToString(@"hh\:mm"), totalPrice = service.Price } });
    }

    public async Task<OperationResult> GetPatientAppointments(string tab = "upcoming", int? limit = null)
    {
        var patientId = _requestContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (patientId == null)
            return Unauthorized();
        if (tab.ToLower() != "upcoming" && tab.ToLower() != "past")
            return BadRequest("Invalid tab value. Use 'upcoming' or 'past'.");
        var bookings = await _bookings.ListPatientAppointmentsAsync(tab, limit, patientId, _requestContext.CancellationToken);
        var nurseIds = bookings.Select(b => b.NurseId).Distinct().ToList();
        var nurseProfiles = await _nurseProfiles.GetImagePathsAsync(nurseIds, _requestContext.CancellationToken);
        var baseUrl = _requestContext.PublicBaseUrl;
        var appointments = bookings.Select(b => new { bookingId = b.BookingId, nurseName = b.Nurse.FullName, profileImageUrl = nurseProfiles.ContainsKey(b.NurseId) && nurseProfiles[b.NurseId] != null ? $"{baseUrl}/{nurseProfiles[b.NurseId]}" : null, serviceName = b.Service.ServiceCatalog.Name, date = b.BookingDate.ToString("yyyy-MM-dd"), time = b.StartTime.ToString(@"hh\:mm"), address = b.ServiceAddress, totalPrice = b.Service.Price, status = b.Status }).ToList();
        return Ok(appointments);
    }

    public async Task<OperationResult> GetAppointmentDetails(int bookingId)
    {
        var patientId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (patientId == null)
            return Unauthorized();
        var booking = await _bookings.GetPatientDetailsAsync(bookingId, patientId, _requestContext.CancellationToken);
        if (booking == null)
            return NotFound("Appointment not found.");
        var nurseProfile = await _nurseProfiles.GetBookingNurseProfileAsync(booking, _requestContext.CancellationToken);
        var baseUrl = _requestContext.PublicBaseUrl;
        return Ok(new { bookingId = booking.BookingId, nurseName = booking.Nurse.FullName, profileImageUrl = nurseProfile != null && nurseProfile.ProfileImagePath != null ? $"{baseUrl}/{nurseProfile.ProfileImagePath}" : null, phoneNumber = nurseProfile?.PhoneNumber, serviceName = booking.Service.ServiceCatalog.Name, date = booking.BookingDate.ToString("yyyy-MM-dd"), time = booking.StartTime.ToString(@"hh\:mm"), address = booking.ServiceAddress, durationInMinutes = booking.Service.ServiceCatalog.DefaultDurationInMinutes, totalPrice = booking.Service.Price, additionalNotes = booking.AdditionalNotes, status = booking.Status });
    }

    public async Task<OperationResult> CancelAppointment(int bookingId)
    {
        var patientId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (patientId == null)
            return Unauthorized();
        var booking = await _bookings.GetPatientBookingForUpdateAsync(bookingId, patientId, _requestContext.CancellationToken);
        if (booking == null)
            return NotFound("Appointment not found.");
        if (booking.Status != "Pending" && booking.Status != "Accepted")
            return BadRequest("Only pending or accepted appointments can be cancelled.");
        booking.Status = "Cancelled";
        _notifications.Add(new Notification { UserId = booking.NurseId, Title = "Appointment Cancelled", Message = "The patient has cancelled the appointment.", Type = "Booking", BookingId = booking.BookingId, CreatedAt = DateTime.UtcNow });
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Appointment cancelled successfully.", status = booking.Status });
    }

    public async Task<OperationResult> CreatePaymentIntent(int bookingId)
    {
        var patientId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (patientId == null)
            return Unauthorized();
        var booking = await _bookings.GetPatientBookingWithServiceForUpdateAsync(bookingId, patientId, _requestContext.CancellationToken);
        if (booking == null)
            return NotFound("Booking not found.");
        if (booking.Status != "Accepted")
            return BadRequest("Payment is allowed only after the booking is accepted.");
        var existingPaidPayment = await _payments.GetPaidByBookingAsync(bookingId, _requestContext.CancellationToken);
        if (existingPaidPayment != null)
            return BadRequest("This booking has already been paid.");
        var payment = await _payments.GetByBookingForUpdateAsync(bookingId, _requestContext.CancellationToken);
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
            _payments.Add(payment);
        }

        payment.StripePaymentIntentId = paymentIntent.Id;
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { clientSecret = paymentIntent.ClientSecret });
    }

    public async Task<OperationResult> ConfirmPayment(int bookingId)
    {
        var patientId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (patientId == null)
            return Unauthorized();
        var booking = await _bookings.GetPatientBookingWithServiceForUpdateAsync(bookingId, patientId, _requestContext.CancellationToken);
        if (booking == null)
            return NotFound("Booking not found.");
        if (booking.Status != "Accepted")
            return BadRequest("Booking is not awaiting payment.");
        var existingPayment = await _payments.GetByBookingForUpdateAsync(bookingId, _requestContext.CancellationToken);
        if (existingPayment != null && existingPayment.Status == "Paid")
            return BadRequest("Payment already confirmed.");
        if (existingPayment?.StripePaymentIntentId is null)
            return BadRequest("Create a payment intent before confirming payment.");
        if (!await _paymentGateway.IsPaidAsync(existingPayment.StripePaymentIntentId, bookingId, patientId, existingPayment.Amount, _requestContext.CancellationToken))
            return BadRequest("Payment has not been verified by the payment provider.");
        existingPayment.Status = "Paid";
        existingPayment.PaymentMethod = "Card";
        booking.Status = "Active";
        _notifications.Add(new Notification { UserId = booking.PatientId, Title = "Payment Successful", Message = "Your payment was completed successfully.", Type = "Payment", BookingId = booking.BookingId, CreatedAt = DateTime.UtcNow });
        _notifications.Add(new Notification { UserId = booking.NurseId, Title = "Payment Received", Message = "The patient has completed the payment for the appointment.", Type = "Payment", BookingId = booking.BookingId, CreatedAt = DateTime.UtcNow });
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Payment confirmed successfully.", bookingId = booking.BookingId, amountPaid = existingPayment.Amount, paymentStatus = "Paid", bookingStatus = booking.Status });
    }

    public async Task<OperationResult> GetPayment(int bookingId)
    {
        var patientId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (patientId == null)
            return Unauthorized();
        var booking = await _bookings.GetPatientBookingAsync(bookingId, patientId, _requestContext.CancellationToken);
        if (booking == null)
            return NotFound("Booking not found.");
        var payment = await _payments.GetByBookingAsync(bookingId, _requestContext.CancellationToken);
        if (payment == null)
            return NotFound("Payment not found.");
        return Ok(new { paymentId = payment.PaymentId, bookingId = payment.BookingId, amount = payment.Amount, status = payment.Status, createdAt = payment.CreatedAt });
    }

    public async Task<OperationResult> GetPaymentSummary(int bookingId)
    {
        var patientId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (patientId == null)
            return Unauthorized();
        var booking = await _bookings.GetPatientDetailsAsync(bookingId, patientId, _requestContext.CancellationToken);
        if (booking == null)
            return NotFound("Booking not found.");
        return Ok(new { nurseName = booking.Nurse.FullName, serviceName = booking.Service.ServiceCatalog.Name, date = booking.BookingDate.ToString("yyyy-MM-dd"), time = booking.StartTime.ToString(@"hh\:mm"), amount = booking.Service.Price });
    }

    public async Task<OperationResult> GetNurseReviews(string nurseId)
    {
        var nurseExists = await _nurseProfiles.IsApprovedAsync(nurseId, _requestContext.CancellationToken);
        if (!nurseExists)
            return NotFound("Nurse not found.");
        var reviews = await _reviews.ListPublicReviewsAsync(nurseId, _requestContext.CancellationToken);
        var averageRating = reviews.Any() ? Math.Round(reviews.Average(r => (double)r.rating), 1) : 0.0;
        var reviewsCount = reviews.Count;
        return Ok(new { averageRating, reviewsCount, reviews });
    }

    public async Task<OperationResult> GetPatientDashboardSummary()
    {
        var patientId = _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(patientId))
            return Unauthorized();
        var totalBookings = await _bookings.CountForPatientAsync(patientId, _requestContext.CancellationToken);
        var activeRequests = await _bookings.CountActiveForPatientAsync(patientId, _requestContext.CancellationToken);
        return Ok(new { totalBookings, activeRequests });
    }
}
