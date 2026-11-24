using NurseNow.Application.Repositories;
using NurseNow.DTOs;
using NurseNow.Models;
using System.Security.Claims;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Application.Features;
public sealed class NurseService : ApplicationService
{
    private readonly IRequestContext _requestContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INurseProfileRepository _nurseProfiles;
    private readonly IServiceRepository _services;
    private readonly IServiceCatalogRepository _serviceCatalogs;
    private readonly IWeeklyAvailabilityRepository _weeklyAvailabilities;
    private readonly IAvailabilityOverrideRepository _availabilityOverrides;
    private readonly IBookingRepository _bookings;
    private readonly IReviewRepository _reviews;
    private readonly INotificationRepository _notifications;
    private readonly IFileStorage _fileStorage;
    public NurseService(IUnitOfWork unitOfWork, INurseProfileRepository nurseProfiles, IServiceRepository services, IServiceCatalogRepository serviceCatalogs, IWeeklyAvailabilityRepository weeklyAvailabilities, IAvailabilityOverrideRepository availabilityOverrides, IBookingRepository bookings, IReviewRepository reviews, INotificationRepository notifications, IFileStorage fileStorage, IRequestContext requestContext)
    {
        _requestContext = requestContext;
        _unitOfWork = unitOfWork;
        _nurseProfiles = nurseProfiles;
        _services = services;
        _serviceCatalogs = serviceCatalogs;
        _weeklyAvailabilities = weeklyAvailabilities;
        _availabilityOverrides = availabilityOverrides;
        _bookings = bookings;
        _reviews = reviews;
        _notifications = notifications;
        _fileStorage = fileStorage;
    }

    private string? GetCurrentUserId()
    {
        return _requestContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public async Task<OperationResult> UpdateProfile(UpdateNurseProfileDto model, UploadFile? profileImage, UploadFile? certificate, UploadFile? nationalIdImage)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var nurseProfile = await _nurseProfiles.GetProfileForUpdateAsync(userId, _requestContext.CancellationToken);
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
        if (profileImage is not null && !UploadPolicy.IsAllowed(profileImage.FileName, profileImage.Length, false))
            return BadRequest("Profile image must be JPG, JPEG, or PNG, at most 5 MB.");
        if (certificate is not null && !UploadPolicy.IsAllowed(certificate.FileName, certificate.Length, true))
            return BadRequest("Certificate must be PDF, at most 5 MB.");
        if (nationalIdImage is not null && !UploadPolicy.IsAllowed(nationalIdImage.FileName, nationalIdImage.Length, false))
            return BadRequest("National ID must be JPG, JPEG, or PNG, at most 5 MB.");
        if (profileImage is not null)
            nurseProfile.ProfileImagePath = await _fileStorage.SaveAsync(profileImage, false, _requestContext.CancellationToken);
        if (certificate is not null)
            nurseProfile.CertificatePath = await _fileStorage.SaveAsync(certificate, true, _requestContext.CancellationToken);
        if (nationalIdImage is not null)
            nurseProfile.NationalIdImagePath = await _fileStorage.SaveAsync(nationalIdImage, false, _requestContext.CancellationToken);
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Profile updated successfully.", nurseProfile.ProfileImagePath, nurseProfile.CertificatePath, nurseProfile.NationalIdImagePath });
    }

    public async Task<OperationResult> GetFullProfile()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var nurseProfile = await _nurseProfiles.GetFullProfileAsync(userId, _requestContext.CancellationToken);
        if (nurseProfile == null)
            return NotFound("Nurse profile not found.");
        var services = await _services.ListProfileServicesAsync(userId, _requestContext.CancellationToken);
        var baseUrl = _requestContext.PublicBaseUrl;
        return Ok(new { personalInfo = new { fullName = nurseProfile.User.FullName, email = nurseProfile.User.Email, phoneNumber = nurseProfile.PhoneNumber, location = nurseProfile.Location, address = nurseProfile.Address, bio = nurseProfile.Bio, naionalId = nurseProfile.NationalId, profileImageUrl = nurseProfile.ProfileImagePath != null ? $"{baseUrl}/{nurseProfile.ProfileImagePath}" : null }, professionalDetails = new { licenseNumber = nurseProfile.LicenseNumber, specialization = nurseProfile.Specialization, experienceYears = nurseProfile.ExperienceYears }, services });
    }

    public async Task<OperationResult> GetPersonalInfo()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var nurseProfile = await _nurseProfiles.GetFullProfileAsync(userId, _requestContext.CancellationToken);
        if (nurseProfile == null)
            return NotFound("Nurse profile not found.");
        return Ok(new { fullName = nurseProfile.User.FullName, email = nurseProfile.User.Email, phoneNumber = nurseProfile.PhoneNumber, location = nurseProfile.Location, address = nurseProfile.Address, bio = nurseProfile.Bio, nationalId = nurseProfile.NationalId, });
    }

    public async Task<OperationResult> UpdatePersonalInfo(UpdateNursePersonalInfoDto model)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var nurseProfile = await _nurseProfiles.GetForUpdateAsync(userId, _requestContext.CancellationToken);
        if (nurseProfile == null)
            return NotFound("Nurse profile not found.");
        nurseProfile.User.FullName = model.FullName;
        nurseProfile.PhoneNumber = model.PhoneNumber;
        nurseProfile.Location = model.Location;
        nurseProfile.Address = model.Address;
        nurseProfile.Bio = model.Bio;
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok("Personal information updated successfully.");
    }

    public async Task<OperationResult> GetProfessionalDetails()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var nurseProfile = await _nurseProfiles.GetByUserAsync(userId, _requestContext.CancellationToken);
        if (nurseProfile == null)
            return NotFound("Nurse profile not found.");
        return Ok(new { licenseNumber = nurseProfile.LicenseNumber, specialization = nurseProfile.Specialization, experienceYears = nurseProfile.ExperienceYears });
    }

    public async Task<OperationResult> UpdateProfessionalDetails(UpdateNurseProfessionalDetailsDto model)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var nurseProfile = await _nurseProfiles.GetProfileForUpdateAsync(userId, _requestContext.CancellationToken);
        if (nurseProfile == null)
            return NotFound("Nurse profile not found.");
        nurseProfile.LicenseNumber = model.LicenseNumber;
        nurseProfile.Specialization = model.Specialization;
        nurseProfile.ExperienceYears = model.ExperienceYears;
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok("Professional details updated successfully.");
    }

    public async Task<OperationResult> GetServiceCatalog()
    {
        var catalog = await _serviceCatalogs.ListAsync(_requestContext.CancellationToken);
        return Ok(catalog);
    }

    public async Task<OperationResult> GetMyServices()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var services = await _services.ListProfileServicesAsync(userId, _requestContext.CancellationToken);
        return Ok(services);
    }

    public async Task<OperationResult> AddService(SaveNurseServiceDto model)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var serviceCatalog = await _serviceCatalogs.FindCatalogForServiceAsync(model, _requestContext.CancellationToken);
        if (serviceCatalog == null)
            return BadRequest("Invalid service.");
        var alreadyExists = await _services.CatalogServiceExistsAsync(model, userId, _requestContext.CancellationToken);
        if (alreadyExists)
            return BadRequest("This service has already been added.");
        var service = new Service
        {
            NurseId = userId,
            ServiceCatalogId = model.ServiceCatalogId,
            Price = model.Price
        };
        _services.Add(service);
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok("Service added successfully.");
    }

    public async Task<OperationResult> UpdateService(int id, SaveNurseServiceDto model)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var service = await _services.GetOwnedForUpdateAsync(id, userId, _requestContext.CancellationToken);
        if (service == null)
            return NotFound("Service not found.");
        var serviceCatalog = await _serviceCatalogs.FindCatalogForServiceAsync(model, _requestContext.CancellationToken);
        if (serviceCatalog == null)
            return BadRequest("Invalid service.");
        service.ServiceCatalogId = model.ServiceCatalogId;
        service.Price = model.Price;
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok("Service updated successfully.");
    }

    public async Task<OperationResult> DeleteService(int id)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var service = await _services.GetOwnedForUpdateAsync(id, userId, _requestContext.CancellationToken);
        if (service == null)
            return NotFound("Service not found.");
        _services.Remove(service);
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok("Service deleted successfully.");
    }

    public async Task<OperationResult> GetWeeklyAvailability()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var weeklySchedule = await _weeklyAvailabilities.ListScheduleAsync(userId, _requestContext.CancellationToken);
        return Ok(weeklySchedule);
    }

    public async Task<OperationResult> AddWeeklyAvailability(AddWeeklyAvailabilityDto model)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        if (model.StartTime >= model.EndTime)
            return BadRequest("Start time must be earlier than end time.");
        var validDays = new[]
        {
            "Sunday",
            "Monday",
            "Tuesday",
            "Wednesday",
            "Thursday",
            "Friday",
            "Saturday"
        };
        if (!validDays.Contains(model.DayOfWeek))
            return BadRequest("Invalid day of week.");
        var alreadyExists = await _weeklyAvailabilities.ActiveDayExistsAsync(model, userId, _requestContext.CancellationToken);
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
        _weeklyAvailabilities.Add(availability);
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Working hours added successfully.", availabilityId = availability.WeeklyAvailabilityId });
    }

    public async Task<OperationResult> DeleteWeeklyAvailability(int id)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var availability = await _weeklyAvailabilities.GetOwnedForUpdateAsync(id, userId, _requestContext.CancellationToken);
        if (availability == null)
            return NotFound("Weekly availability not found.");
        _weeklyAvailabilities.Remove(availability);
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Weekly availability deleted successfully." });
    }

    public async Task<OperationResult> GetDayDetails(DateTime date)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var dayOfWeek = date.DayOfWeek.ToString();
        var weeklyAvailability = await _weeklyAvailabilities.GetDayDetailsAsync(userId, dayOfWeek, _requestContext.CancellationToken);
        var overrideRecord = await _availabilityOverrides.GetDayDetailsAsync(date, userId, _requestContext.CancellationToken);
        var bookedAppointments = new List<object>();
        return Ok(new { selectedDate = date.ToString("yyyy-MM-dd"), dayOfWeek, defaultWorkingHours = weeklyAvailability, dayOverride = overrideRecord, bookedAppointments });
    }

    public async Task<OperationResult> OverrideDayAvailability(OverrideDayAvailabilityDto model)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        if (model.StartTime >= model.EndTime)
            return BadRequest("Start time must be earlier than end time.");
        var existingOverride = await _availabilityOverrides.FindForOverrideAsync(model, userId, _requestContext.CancellationToken);
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
            _availabilityOverrides.Add(newOverride);
        }

        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Day working hours overridden successfully." });
    }

    public async Task<OperationResult> BlockDay(BlockDayDto model)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var existingOverride = await _availabilityOverrides.FindForBlockAsync(model, userId, _requestContext.CancellationToken);
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
            _availabilityOverrides.Add(blockedDay);
        }

        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Day blocked successfully." });
    }

    public async Task<OperationResult> RemoveDayOverride(DateTime date)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var overrideRecord = await _availabilityOverrides.GetForUpdateAsync(date, userId, _requestContext.CancellationToken);
        if (overrideRecord == null)
            return NotFound("No override found for this date.");
        _availabilityOverrides.Remove(overrideRecord);
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Day override removed successfully." });
    }

    public async Task<OperationResult> GetNurseRequests(string? status)
    {
        var nurseId = GetCurrentUserId();
        if (string.IsNullOrEmpty(nurseId))
            return Unauthorized();
        var requests = await _bookings.ListNurseRequestsAsync(status, nurseId, _requestContext.CancellationToken);
        return Ok(requests);
    }

    public async Task<OperationResult> GetRequestDetails(int bookingId)
    {
        var nurseId = GetCurrentUserId();
        if (string.IsNullOrEmpty(nurseId))
            return Unauthorized();
        var booking = await _bookings.GetNurseDetailsAsync(bookingId, nurseId, _requestContext.CancellationToken);
        if (booking == null)
            return NotFound("Request not found.");
        return Ok(new { bookingId = booking.BookingId, patientName = booking.Patient.FullName, contact = booking.Patient.PhoneNumber, serviceType = booking.Service.ServiceCatalog.Name, date = booking.BookingDate.ToString("yyyy-MM-dd"), time = booking.StartTime.ToString(@"hh\:mm"), location = booking.ServiceAddress, durationInMinutes = booking.Service.ServiceCatalog.DefaultDurationInMinutes, payment = booking.Service.Price, additionalNotes = booking.AdditionalNotes, status = booking.Status });
    }

    public async Task<OperationResult> AcceptRequest(int bookingId)
    {
        var nurseId = GetCurrentUserId();
        if (string.IsNullOrEmpty(nurseId))
            return Unauthorized();
        var booking = await _bookings.GetNurseBookingForUpdateAsync(bookingId, nurseId, _requestContext.CancellationToken);
        if (booking == null)
            return NotFound("Request not found.");
        if (booking.Status != "Pending")
            return BadRequest("Only pending requests can be accepted.");
        booking.Status = "Accepted";
        _notifications.Add(new Notification { UserId = booking.PatientId, Title = "Booking Confirmed", Message = "Your booking request has been accepted by the nurse.", Type = "Booking", BookingId = booking.BookingId, CreatedAt = DateTime.UtcNow });
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Request accepted successfully.", status = booking.Status });
    }

    public async Task<OperationResult> DeclineRequest(int bookingId)
    {
        var nurseId = GetCurrentUserId();
        if (string.IsNullOrEmpty(nurseId))
            return Unauthorized();
        var booking = await _bookings.GetNurseBookingForUpdateAsync(bookingId, nurseId, _requestContext.CancellationToken);
        if (booking == null)
            return NotFound("Request not found.");
        if (booking.Status != "Pending")
            return BadRequest("Only pending requests can be declined.");
        booking.Status = "Rejected";
        _notifications.Add(new Notification { UserId = booking.PatientId, Title = "Request Rejected", Message = "Your booking request has been rejected by the nurse.", Type = "Booking", BookingId = booking.BookingId, CreatedAt = DateTime.UtcNow });
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Request declined successfully.", status = booking.Status });
    }

    public async Task<OperationResult> GetNurseAppointments(string tab = "upcoming")
    {
        var nurseId = GetCurrentUserId();
        if (string.IsNullOrEmpty(nurseId))
            return Unauthorized();
        var today = DateTime.Today;
        if (tab.ToLower() != "today" && tab.ToLower() != "upcoming" && tab.ToLower() != "past")
            return BadRequest("Invalid tab value. Use 'today', 'upcoming', or 'past'.");
        var appointments = await _bookings.ListNurseAppointmentsAsync(tab, nurseId, today, _requestContext.CancellationToken);
        return Ok(appointments);
    }

    public async Task<OperationResult> GetAppointmentDetailsForNurse(int bookingId)
    {
        var nurseId = GetCurrentUserId();
        if (string.IsNullOrEmpty(nurseId))
            return Unauthorized();
        var booking = await _bookings.GetNurseDetailsAsync(bookingId, nurseId, _requestContext.CancellationToken);
        if (booking == null)
            return NotFound("Appointment not found.");
        return Ok(new { bookingId = booking.BookingId, patientName = booking.Patient.FullName, phoneNumber = booking.Patient.PhoneNumber, serviceName = booking.Service.ServiceCatalog.Name, date = booking.BookingDate.ToString("yyyy-MM-dd"), time = booking.StartTime.ToString(@"hh\:mm"), address = booking.ServiceAddress, totalPrice = booking.Service.Price, additionalNotes = booking.AdditionalNotes, status = booking.Status });
    }

    public async Task<OperationResult> CompleteAppointment(int bookingId)
    {
        var nurseId = GetCurrentUserId();
        if (string.IsNullOrEmpty(nurseId))
            return Unauthorized();
        var booking = await _bookings.GetNurseBookingForUpdateAsync(bookingId, nurseId, _requestContext.CancellationToken);
        if (booking == null)
            return NotFound("Appointment not found.");
        if (booking.Status != "Accepted" && booking.Status != "Active")
            return BadRequest("Only accepted or active appointments can be marked as completed.");
        booking.Status = "Completed";
        _notifications.Add(new Notification { UserId = booking.PatientId, Title = "Appointment Completed", Message = "Your appointment has been marked as completed.", Type = "Booking", BookingId = booking.BookingId, CreatedAt = DateTime.UtcNow });
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Appointment marked as completed successfully.", status = booking.Status });
    }

    public async Task<OperationResult> GetMyReviews()
    {
        var nurseId = GetCurrentUserId();
        if (string.IsNullOrEmpty(nurseId))
            return Unauthorized();
        var reviews = await _reviews.ListForNurseAsync(nurseId, _requestContext.CancellationToken);
        var averageRating = reviews.Any() ? Math.Round(reviews.Average(r => (double)r.rating), 1) : 0.0;
        return Ok(new { averageRating, reviewsCount = reviews.Count, reviews });
    }

    public async Task<OperationResult> CancelAppointmentByNurse(int bookingId)
    {
        var nurseId = GetCurrentUserId();
        if (string.IsNullOrEmpty(nurseId))
            return Unauthorized();
        var booking = await _bookings.GetNurseBookingForUpdateAsync(bookingId, nurseId, _requestContext.CancellationToken);
        if (booking == null)
            return NotFound("Appointment not found.");
        if (booking.Status != "Accepted" && booking.Status != "Pending")
            return BadRequest("Only pending or accepted appointments can be cancelled.");
        booking.Status = "Cancelled";
        _notifications.Add(new Notification { UserId = booking.PatientId, Title = "Appointment Cancelled", Message = "The nurse has cancelled your appointment.", Type = "Booking", BookingId = booking.BookingId, CreatedAt = DateTime.UtcNow });
        await _unitOfWork.SaveChangesAsync(_requestContext.CancellationToken);
        return Ok(new { message = "Appointment cancelled successfully.", status = booking.Status });
    }
}
