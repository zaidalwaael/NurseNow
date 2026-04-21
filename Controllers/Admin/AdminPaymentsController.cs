using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.DTOs.Admin;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NurseNow.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/transactions")]
    [Authorize]
    public class AdminPaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminPaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET: /api/admin/transactions?search=ali&status=Paid
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] string? search,
            [FromQuery] string? status)
        {
            var query = _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Patient)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Nurse)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Service)
                        .ThenInclude(s => s.ServiceCatalog)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(p => p.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.ToLower();

                query = query.Where(p =>
                    p.PaymentId.ToString().Contains(keyword) ||
                    p.BookingId.ToString().Contains(keyword) ||
                    (p.Booking.Patient.FullName != null && p.Booking.Patient.FullName.ToLower().Contains(keyword)) ||
                    (p.Booking.Nurse.FullName != null && p.Booking.Nurse.FullName.ToLower().Contains(keyword))
                );
            }

            var transactions = await query
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new TransactionListDto
                {
                    PaymentId = p.PaymentId,
                    TransactionId = $"TXN{p.PaymentId:D6}",
                    BookingId = p.BookingId,
                    BookingCode = $"BK{p.BookingId:D7}",
                    PatientName = p.Booking.Patient.FullName ?? "",
                    NurseName = p.Booking.Nurse.FullName ?? "",
                    ServiceName = p.Booking.Service.ServiceCatalog.Name,
                    TotalAmount = p.Amount,
                    Commission = Math.Round(p.Amount * 0.15m, 2),
                    NurseAmount = Math.Round(p.Amount * 0.85m, 2),
                    Status = p.Status,
                    PaymentMethod = string.IsNullOrWhiteSpace(p.PaymentMethod) ? "N/A" : p.PaymentMethod,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return Ok(transactions);
        }

        // =====================================================
        // GET: /api/admin/transactions/stats
        // =====================================================
        [HttpGet("stats")]
        public async Task<IActionResult> GetTransactionStats()
        {
            var payments = await _context.Payments.ToListAsync();

            var paidPayments = payments.Where(p => p.Status == "Paid").ToList();
            var refundedPayments = payments.Where(p => p.Status == "Refunded").ToList();
            var pendingPayments = payments.Where(p => p.Status == "Pending").ToList();

            var result = new TransactionStatsDto
            {
                TotalRevenue = paidPayments.Sum(p => p.Amount),
                PlatformCommission = paidPayments.Sum(p => Math.Round(p.Amount * 0.15m, 2)),
                NursePayouts = paidPayments.Sum(p => Math.Round(p.Amount * 0.85m, 2)),
                RefundedTransactions = refundedPayments.Sum(p => p.Amount),
                PendingPayments = pendingPayments.Sum(p => p.Amount)
            };

            return Ok(result);
        }

        // =====================================================
        // GET: /api/admin/transactions/activity
        // =====================================================
        [HttpGet("activity")]
        public async Task<IActionResult> GetRecentFinancialActivity()
        {
            var payments = await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Nurse)
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
                .ToListAsync();

            var activities = new List<FinancialActivityDto>();

            foreach (var payment in payments)
            {
                if (payment.Status == "Paid")
                {
                    activities.Add(new FinancialActivityDto
                    {
                        Title = $"Payment received for Booking #{payment.BookingId}",
                        Description = $"Commission earned from Transaction #TXN{payment.PaymentId:D6}",
                        Amount = payment.Amount,
                        Type = "Income",
                        CreatedAt = payment.CreatedAt
                    });

                    activities.Add(new FinancialActivityDto
                    {
                        Title = $"Payout to {payment.Booking?.Nurse?.FullName ?? "Nurse"}",
                        Description = $"Nurse payout for Booking #{payment.BookingId}",
                        Amount = Math.Round(payment.Amount * 0.85m, 2),
                        Type = "Expense",
                        CreatedAt = payment.CreatedAt
                    });
                }

                if (payment.Status == "Refunded")
                {
                    activities.Add(new FinancialActivityDto
                    {
                        Title = $"Refund issued for Booking #{payment.BookingId}",
                        Description = $"Refund for Transaction #TXN{payment.PaymentId:D6}",
                        Amount = payment.Amount,
                        Type = "Refund",
                        CreatedAt = payment.CreatedAt
                    });
                }
            }

            var result = activities
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .ToList();

            return Ok(result);
        }

        // =====================================================
        // GET: /api/admin/transactions/export-pdf
        // =====================================================
        [HttpGet("export-pdf")]
        public async Task<IActionResult> ExportTransactionsPdf(
     [FromQuery] string? search,
     [FromQuery] string? status)
        {
            var query = _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Patient)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Nurse)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Service)
                        .ThenInclude(s => s.ServiceCatalog)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(p => p.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.ToLower();

                query = query.Where(p =>
                    p.PaymentId.ToString().Contains(keyword) ||
                    p.BookingId.ToString().Contains(keyword) ||
                    (p.Booking.Patient.FullName != null && p.Booking.Patient.FullName.ToLower().Contains(keyword)) ||
                    (p.Booking.Nurse.FullName != null && p.Booking.Nurse.FullName.ToLower().Contains(keyword))
                );
            }

            var transactions = await query
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new TransactionListDto
                {
                    PaymentId = p.PaymentId,
                    TransactionId = $"TXN{p.PaymentId:D6}",
                    BookingId = p.BookingId,
                    BookingCode = $"BK{p.BookingId:D7}",
                    PatientName = p.Booking.Patient.FullName ?? "",
                    NurseName = p.Booking.Nurse.FullName ?? "",
                    ServiceName = p.Booking.Service.ServiceCatalog.Name,
                    TotalAmount = p.Amount,
                    Commission = Math.Round(p.Amount * 0.15m, 2),
                    NurseAmount = Math.Round(p.Amount * 0.85m, 2),
                    Status = p.Status,
                    PaymentMethod = string.IsNullOrWhiteSpace(p.PaymentMethod) ? "N/A" : p.PaymentMethod,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Column(column =>
                        {
                            column.Item().Text("Payment & Transactions Report")
                                .FontSize(18)
                                .Bold();

                            column.Item().Text($"Generated on: {DateTime.Now:yyyy-MM-dd hh:mm tt}")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken1);

                            if (!string.IsNullOrWhiteSpace(status) && status != "All")
                                column.Item().Text($"Status Filter: {status}");

                            if (!string.IsNullOrWhiteSpace(search))
                                column.Item().Text($"Search: {search}");
                        });

                    page.Content()
                        .PaddingVertical(10)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.3f); // Transaction ID
                                columns.RelativeColumn(1.2f); // Booking ID
                                columns.RelativeColumn(1.8f); // Patient
                                columns.RelativeColumn(1.8f); // Nurse
                                columns.RelativeColumn(1.6f); // Service
                                columns.RelativeColumn(1.2f); // Total
                                columns.RelativeColumn(1.2f); // Comm
                                columns.RelativeColumn(1.2f); // Nurse Amount
                                columns.RelativeColumn(1.1f); // Status
                                columns.RelativeColumn(1.4f); // Method
                                columns.RelativeColumn(1.7f); // Date
                            });

                            table.Header(header =>
                            {
                                static void HeaderCell(IContainer container, string text)
                                {
                                    container
                                        .Background(Colors.Grey.Lighten2)
                                        .Border(1)
                                        .BorderColor(Colors.Grey.Lighten1)
                                        .Padding(5)
                                        .Text(text)
                                        .Bold();
                                }

                                HeaderCell(header.Cell(), "Transaction ID");
                                HeaderCell(header.Cell(), "Booking ID");
                                HeaderCell(header.Cell(), "Patient");
                                HeaderCell(header.Cell(), "Nurse");
                                HeaderCell(header.Cell(), "Service");
                                HeaderCell(header.Cell(), "Total");
                                HeaderCell(header.Cell(), "Commission");
                                HeaderCell(header.Cell(), "Nurse Amount");
                                HeaderCell(header.Cell(), "Status");
                                HeaderCell(header.Cell(), "Method");
                                HeaderCell(header.Cell(), "Date");
                            });

                            foreach (var item in transactions)
                            {
                                static void BodyCell(IContainer container, string text)
                                {
                                    container
                                        .Border(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .Padding(4)
                                        .Text(text);
                                }

                                BodyCell(table.Cell(), item.TransactionId);
                                BodyCell(table.Cell(), item.BookingCode);
                                BodyCell(table.Cell(), item.PatientName);
                                BodyCell(table.Cell(), item.NurseName);
                                BodyCell(table.Cell(), item.ServiceName);
                                BodyCell(table.Cell(), item.TotalAmount.ToString("0.00"));
                                BodyCell(table.Cell(), item.Commission.ToString("0.00"));
                                BodyCell(table.Cell(), item.NurseAmount.ToString("0.00"));
                                BodyCell(table.Cell(), item.Status);
                                BodyCell(table.Cell(), item.PaymentMethod);
                                BodyCell(table.Cell(), item.CreatedAt.ToString("yyyy-MM-dd hh:mm tt"));
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", "transactions-report.pdf");
        }


    }
}