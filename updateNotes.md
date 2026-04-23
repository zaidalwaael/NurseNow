# NurseNow — Nurse & Patient History + Report APIs

## Nurse

### Payment History

#### DTOs

##### `DTOs/Nurse/NursePaymentSummaryDto.cs`
```csharp
namespace NurseNow.DTOs.Nurse
{
    public class NursePaymentSummaryDto
    {
        public decimal TotalEarnings { get; set; }
        public decimal ThisMonthEarnings { get; set; }
        public decimal PendingAmount { get; set; }
    }
}
```

##### `DTOs/Nurse/NursePaymentHistoryItemDto.cs`
```csharp
namespace NurseNow.DTOs.Nurse
{
    public class NursePaymentHistoryItemDto
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }

        public string PatientName { get; set; } = "";
        public string ServiceName { get; set; } = "";

        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "";
        public string Status { get; set; } = "";

        public DateTime CreatedAt { get; set; }
    }
}
```

##### `DTOs/Nurse/NursePaymentHistoryDetailsDto.cs`
```csharp
namespace NurseNow.DTOs.Nurse
{
    public class NursePaymentHistoryDetailsDto
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }

        public string PatientName { get; set; } = "";
        public string NurseName { get; set; } = "";
        public string ServiceName { get; set; } = "";

        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "";
        public string Status { get; set; } = "";

        public DateTime CreatedAt { get; set; }

        public string ServiceAddress { get; set; } = "";
        public DateTime BookingDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
```

#### Controller

##### `Controllers/Nurse/NursePaymentsController.cs`
```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.DTOs.Nurse;

namespace NurseNow.Controllers.Nurse
{
    [ApiController]
    [Route("api/nurse/payments")]
    [Authorize(Roles = "Nurse")]
    public class NursePaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NursePaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetPaymentSummary()
        {
            var nurseId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(nurseId))
                return Unauthorized();

            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var nextMonthStart = monthStart.AddMonths(1);

            var payments = await _context.Payments
                .Include(p => p.Booking)
                .Where(p => p.Booking.NurseId == nurseId)
                .ToListAsync();

            var result = new NursePaymentSummaryDto
            {
                TotalEarnings = payments
                    .Where(p => p.Status == "Paid" || p.Status == "Completed")
                    .Sum(p => p.Amount),

                ThisMonthEarnings = payments
                    .Where(p =>
                        (p.Status == "Paid" || p.Status == "Completed") &&
                        p.CreatedAt >= monthStart &&
                        p.CreatedAt < nextMonthStart)
                    .Sum(p => p.Amount),

                PendingAmount = payments
                    .Where(p => p.Status == "Pending")
                    .Sum(p => p.Amount)
            };

            return Ok(result);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetPaymentHistory(string? tab = "all")
        {
            var nurseId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(nurseId))
                return Unauthorized();

            var query = _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Patient)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Service)
                        .ThenInclude(s => s.ServiceCatalog)
                .Where(p => p.Booking.NurseId == nurseId)
                .AsQueryable();

            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var nextMonthStart = monthStart.AddMonths(1);

            if (tab == "thisMonth")
            {
                query = query.Where(p => p.CreatedAt >= monthStart && p.CreatedAt < nextMonthStart);
            }
            else if (tab == "history")
            {
                query = query.Where(p => p.Status == "Paid" || p.Status == "Completed");
            }

            var payments = await query
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new NursePaymentHistoryItemDto
                {
                    PaymentId = p.PaymentId,
                    BookingId = p.BookingId,
                    PatientName = p.Booking.Patient.FullName,
                    ServiceName = p.Booking.Service.ServiceCatalog.Name,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return Ok(payments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentDetails(int id)
        {
            var nurseId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(nurseId))
                return Unauthorized();

            var payment = await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Patient)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Nurse)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Service)
                        .ThenInclude(s => s.ServiceCatalog)
                .Where(p => p.PaymentId == id && p.Booking.NurseId == nurseId)
                .Select(p => new NursePaymentHistoryDetailsDto
                {
                    PaymentId = p.PaymentId,
                    BookingId = p.BookingId,
                    PatientName = p.Booking.Patient.FullName,
                    NurseName = p.Booking.Nurse.FullName,
                    ServiceName = p.Booking.Service.ServiceCatalog.Name,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    ServiceAddress = p.Booking.ServiceAddress,
                    BookingDate = p.Booking.BookingDate,
                    StartTime = p.Booking.StartTime,
                    EndTime = p.Booking.EndTime
                })
                .FirstOrDefaultAsync();

            if (payment == null)
                return NotFound(new { message = "Payment not found." });

            return Ok(payment);
        }
    }
}
```

#### Endpoints
- `GET /api/nurse/payments/summary`
- `GET /api/nurse/payments/history?tab=all`
- `GET /api/nurse/payments/history?tab=thisMonth`
- `GET /api/nurse/payments/history?tab=history`
- `GET /api/nurse/payments/{id}`

---

### Report a Problem

#### DTOs

##### `DTOs/Nurse/SubmitProblemDto.cs`
```csharp
namespace NurseNow.DTOs.Nurse
{
    public class SubmitProblemDto
    {
        public string Category { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsUrgent { get; set; } = false;
    }
}
```

##### `DTOs/Nurse/NurseProblemItemDto.cs`
```csharp
namespace NurseNow.DTOs.Nurse
{
    public class NurseProblemItemDto
    {
        public int ComplaintId { get; set; }
        public string Category { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Status { get; set; } = "";
        public bool IsUrgent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
```

##### `DTOs/Nurse/NurseProblemDetailsDto.cs`
```csharp
namespace NurseNow.DTOs.Nurse
{
    public class NurseProblemDetailsDto
    {
        public int ComplaintId { get; set; }

        public string Category { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Description { get; set; } = "";

        public bool IsUrgent { get; set; }

        public string Status { get; set; } = "";
        public string? AdminResponse { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
    }
}
```

#### Controller

##### `Controllers/Nurse/NurseProblemsController.cs`
```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.DTOs.Nurse;
using NurseNow.Models;

namespace NurseNow.Controllers.Nurse
{
    [ApiController]
    [Route("api/nurse/problems")]
    [Authorize(Roles = "Nurse")]
    public class NurseProblemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NurseProblemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitProblem([FromBody] SubmitProblemDto dto)
        {
            var nurseId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(nurseId))
                return Unauthorized();

            var allowedCategories = new[]
            {
                "Technical",
                "Payment",
                "Service Issue",
                "Account",
                "Other"
            };

            if (string.IsNullOrWhiteSpace(dto.Category))
                return BadRequest(new { message = "Problem category is required." });

            if (!allowedCategories.Contains(dto.Category))
                return BadRequest(new { message = "Invalid problem category." });

            if (string.IsNullOrWhiteSpace(dto.Subject))
                return BadRequest(new { message = "Subject is required." });

            if (dto.Subject.Trim().Length > 200)
                return BadRequest(new { message = "Subject must not exceed 200 characters." });

            if (string.IsNullOrWhiteSpace(dto.Description))
                return BadRequest(new { message = "Detailed description is required." });

            if (dto.Description.Trim().Length > 1000)
                return BadRequest(new { message = "Description must not exceed 1000 characters." });

            var complaint = new Complaint
            {
                UserId = nurseId,
                Category = dto.Category.Trim(),
                Subject = dto.Subject.Trim(),
                Description = dto.Description.Trim(),
                IsUrgent = dto.IsUrgent,
                Status = "Open",
                CreatedAt = DateTime.UtcNow
            };

            _context.Complaints.Add(complaint);

            _context.AdminActivityLogs.Add(new AdminActivityLog
            {
                Title = "New Nurse Problem Report",
                Description = $"A nurse submitted a new problem report: {dto.Subject}",
                ActivityType = "Complaint",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Problem report submitted successfully.",
                complaintId = complaint.ComplaintId
            });
        }

        [HttpGet("my-reports")]
        public async Task<IActionResult> GetMyReports()
        {
            var nurseId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(nurseId))
                return Unauthorized();

            var reports = await _context.Complaints
                .Where(c => c.UserId == nurseId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new NurseProblemItemDto
                {
                    ComplaintId = c.ComplaintId,
                    Category = c.Category,
                    Subject = c.Subject,
                    Status = c.Status,
                    IsUrgent = c.IsUrgent,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(reports);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProblemDetails(int id)
        {
            var nurseId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(nurseId))
                return Unauthorized();

            var report = await _context.Complaints
                .Where(c => c.ComplaintId == id && c.UserId == nurseId)
                .Select(c => new NurseProblemDetailsDto
                {
                    ComplaintId = c.ComplaintId,
                    Category = c.Category,
                    Subject = c.Subject,
                    Description = c.Description,
                    IsUrgent = c.IsUrgent,
                    Status = c.Status,
                    AdminResponse = c.AdminResponse,
                    CreatedAt = c.CreatedAt,
                    RespondedAt = c.RespondedAt
                })
                .FirstOrDefaultAsync();

            if (report == null)
                return NotFound(new { message = "Problem report not found." });

            return Ok(report);
        }
    }
}
```

#### Endpoints
- `POST /api/nurse/problems`
- `GET /api/nurse/problems/my-reports`
- `GET /api/nurse/problems/{id}`

---

## Patient

### Transaction History

#### DTOs

##### `DTOs/Patient/PatientPaymentHistoryItemDto.cs`
```csharp
namespace NurseNow.DTOs.Patient
{
    public class PatientPaymentHistoryItemDto
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }

        public decimal Amount { get; set; }
        public string Status { get; set; } = "";
        public string PaymentMethod { get; set; } = "";
        public DateTime CreatedAt { get; set; }

        public string NurseName { get; set; } = "";
        public string ServiceName { get; set; } = "";
    }
}
```

##### `DTOs/Patient/PatientPaymentDetailsDto.cs`
```csharp
namespace NurseNow.DTOs.Patient
{
    public class PatientPaymentDetailsDto
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }

        public decimal Amount { get; set; }
        public string Status { get; set; } = "";
        public string PaymentMethod { get; set; } = "";
        public DateTime CreatedAt { get; set; }

        public string PatientName { get; set; } = "";
        public string NurseName { get; set; } = "";
        public string ServiceName { get; set; } = "";

        public DateTime BookingDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string ServiceAddress { get; set; } = "";
    }
}
```

#### Controller

##### `Controllers/Patient/PatientPaymentsController.cs`
```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.DTOs.Patient;

namespace NurseNow.Controllers.Patient
{
    [ApiController]
    [Route("api/patient/payments")]
    [Authorize(Roles = "Patient")]
    public class PatientPaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PatientPaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetPaymentHistory()
        {
            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(patientId))
                return Unauthorized();

            var history = await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Nurse)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Service)
                        .ThenInclude(s => s.ServiceCatalog)
                .Where(p => p.Booking.PatientId == patientId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PatientPaymentHistoryItemDto
                {
                    PaymentId = p.PaymentId,
                    BookingId = p.BookingId,
                    Amount = p.Amount,
                    Status = p.Status,
                    PaymentMethod = p.PaymentMethod,
                    CreatedAt = p.CreatedAt,
                    NurseName = p.Booking.Nurse.FullName,
                    ServiceName = p.Booking.Service.ServiceCatalog.Name
                })
                .ToListAsync();

            return Ok(history);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentDetails(int id)
        {
            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(patientId))
                return Unauthorized();

            var payment = await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Patient)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Nurse)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Service)
                        .ThenInclude(s => s.ServiceCatalog)
                .Where(p => p.PaymentId == id && p.Booking.PatientId == patientId)
                .Select(p => new PatientPaymentDetailsDto
                {
                    PaymentId = p.PaymentId,
                    BookingId = p.BookingId,
                    Amount = p.Amount,
                    Status = p.Status,
                    PaymentMethod = p.PaymentMethod,
                    CreatedAt = p.CreatedAt,
                    PatientName = p.Booking.Patient.FullName,
                    NurseName = p.Booking.Nurse.FullName,
                    ServiceName = p.Booking.Service.ServiceCatalog.Name,
                    BookingDate = p.Booking.BookingDate,
                    StartTime = p.Booking.StartTime,
                    EndTime = p.Booking.EndTime,
                    ServiceAddress = p.Booking.ServiceAddress
                })
                .FirstOrDefaultAsync();

            if (payment == null)
                return NotFound(new { message = "Transaction not found." });

            return Ok(payment);
        }
    }
}
```

#### Endpoints
- `GET /api/patient/payments/history`
- `GET /api/patient/payments/{id}`

---

### Report Issue

#### DTOs

##### `DTOs/Patient/SubmitPatientIssueDto.cs`
```csharp
namespace NurseNow.DTOs.Patient
{
    public class SubmitPatientIssueDto
    {
        public string Category { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsUrgent { get; set; } = false;
    }
}
```

##### `DTOs/Patient/PatientIssueItemDto.cs`
```csharp
namespace NurseNow.DTOs.Patient
{
    public class PatientIssueItemDto
    {
        public int ComplaintId { get; set; }
        public string Category { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Status { get; set; } = "";
        public bool IsUrgent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
```

##### `DTOs/Patient/PatientIssueDetailsDto.cs`
```csharp
namespace NurseNow.DTOs.Patient
{
    public class PatientIssueDetailsDto
    {
        public int ComplaintId { get; set; }

        public string Category { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Description { get; set; } = "";

        public bool IsUrgent { get; set; }

        public string Status { get; set; } = "";
        public string? AdminResponse { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
    }
}
```

#### Controller

##### `Controllers/Patient/PatientIssuesController.cs`
```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.DTOs.Patient;
using NurseNow.Models;

namespace NurseNow.Controllers.Patient
{
    [ApiController]
    [Route("api/patient/issues")]
    [Authorize(Roles = "Patient")]
    public class PatientIssuesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PatientIssuesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitIssue([FromBody] SubmitPatientIssueDto dto)
        {
            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(patientId))
                return Unauthorized();

            var allowedCategories = new[]
            {
                "Technical",
                "Payment",
                "Service Issue",
                "Account",
                "Other"
            };

            if (string.IsNullOrWhiteSpace(dto.Category))
                return BadRequest(new { message = "Category is required." });

            if (!allowedCategories.Contains(dto.Category))
                return BadRequest(new { message = "Invalid category." });

            if (string.IsNullOrWhiteSpace(dto.Subject))
                return BadRequest(new { message = "Subject is required." });

            if (dto.Subject.Trim().Length > 200)
                return BadRequest(new { message = "Subject must not exceed 200 characters." });

            if (string.IsNullOrWhiteSpace(dto.Description))
                return BadRequest(new { message = "Description is required." });

            if (dto.Description.Trim().Length > 1000)
                return BadRequest(new { message = "Description must not exceed 1000 characters." });

            var complaint = new Complaint
            {
                UserId = patientId,
                Category = dto.Category.Trim(),
                Subject = dto.Subject.Trim(),
                Description = dto.Description.Trim(),
                IsUrgent = dto.IsUrgent,
                Status = "Open",
                CreatedAt = DateTime.UtcNow
            };

            _context.Complaints.Add(complaint);

            _context.AdminActivityLogs.Add(new AdminActivityLog
            {
                Title = "New Patient Issue Report",
                Description = $"A patient submitted a new issue report: {dto.Subject}",
                ActivityType = "Complaint",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Issue submitted successfully.",
                complaintId = complaint.ComplaintId
            });
        }

        [HttpGet("my-reports")]
        public async Task<IActionResult> GetMyReports()
        {
            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(patientId))
                return Unauthorized();

            var reports = await _context.Complaints
                .Where(c => c.UserId == patientId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new PatientIssueItemDto
                {
                    ComplaintId = c.ComplaintId,
                    Category = c.Category,
                    Subject = c.Subject,
                    Status = c.Status,
                    IsUrgent = c.IsUrgent,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(reports);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetIssueDetails(int id)
        {
            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(patientId))
                return Unauthorized();

            var report = await _context.Complaints
                .Where(c => c.ComplaintId == id && c.UserId == patientId)
                .Select(c => new PatientIssueDetailsDto
                {
                    ComplaintId = c.ComplaintId,
                    Category = c.Category,
                    Subject = c.Subject,
                    Description = c.Description,
                    IsUrgent = c.IsUrgent,
                    Status = c.Status,
                    AdminResponse = c.AdminResponse,
                    CreatedAt = c.CreatedAt,
                    RespondedAt = c.RespondedAt
                })
                .FirstOrDefaultAsync();

            if (report == null)
                return NotFound(new { message = "Issue report not found." });

            return Ok(report);
        }
    }
}
```

#### Endpoints
- `POST /api/patient/issues`
- `GET /api/patient/issues/my-reports`
- `GET /api/patient/issues/{id}`