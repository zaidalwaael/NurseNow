//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using NurseNow.Data;
//using NurseNow.DTOs.Admin;
//using QuestPDF.Fluent;
//using QuestPDF.Helpers;
//using QuestPDF.Infrastructure;


//namespace NurseNow.Controllers.Admin
//{
//    [ApiController]
//    [Route("api/admin/reports")]
//    [Authorize]
//    public class AdminReportsController : ControllerBase
//    {
//        private readonly ApplicationDbContext _context;

//        public AdminReportsController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        // =====================================================
//        // GET: /api/admin/reports/overview
//        // =====================================================
//        [HttpGet("overview")]
//        public async Task<IActionResult> GetOverview()
//        {
//            var now = DateTime.UtcNow;
//            var currentMonthStart = new DateTime(now.Year, now.Month, 1);
//            var previousMonthStart = currentMonthStart.AddMonths(-1);
//            var nextMonthStart = currentMonthStart.AddMonths(1);

//            var currentMonthBookings = await _context.Bookings
//                .Where(b => b.BookingDate >= currentMonthStart && b.BookingDate < nextMonthStart)
//                .ToListAsync();

//            var previousMonthBookings = await _context.Bookings
//                .Where(b => b.BookingDate >= previousMonthStart && b.BookingDate < currentMonthStart)
//                .ToListAsync();

//            var currentMonthPayments = await _context.Payments
//                .Where(p => p.CreatedAt >= currentMonthStart && p.CreatedAt < nextMonthStart && p.Status == "Paid")
//                .ToListAsync();

//            var previousMonthPayments = await _context.Payments
//                .Where(p => p.CreatedAt >= previousMonthStart && p.CreatedAt < currentMonthStart && p.Status == "Paid")
//                .ToListAsync();

//            var currentTotalRequests = currentMonthBookings.Count;
//            var previousTotalRequests = previousMonthBookings.Count;

//            var currentCompletedRequests = currentMonthBookings.Count(b => b.Status == "Completed");
//            var previousCompletedRequests = previousMonthBookings.Count(b => b.Status == "Completed");

//            var currentCompletionRate = currentTotalRequests == 0
//                ? 0
//                : Math.Round((decimal)currentCompletedRequests / currentTotalRequests * 100, 1);

//            var previousCompletionRate = previousTotalRequests == 0
//                ? 0
//                : Math.Round((decimal)previousCompletedRequests / previousTotalRequests * 100, 1);

//            var currentRevenue = currentMonthPayments.Sum(p => p.Amount);
//            var previousRevenue = previousMonthPayments.Sum(p => p.Amount);

//            decimal CalculateGrowth(decimal currentValue, decimal previousValue)
//            {
//                if (previousValue == 0)
//                    return currentValue > 0 ? 100 : 0;

//                return Math.Round(((currentValue - previousValue) / previousValue) * 100, 1);
//            }

//            var result = new ReportsOverviewDto
//            {
//                TotalRequests = currentTotalRequests,
//                CompletionRate = currentCompletionRate,
//                Revenue = currentRevenue,
//                RequestsGrowthPercentage = CalculateGrowth(currentTotalRequests, previousTotalRequests),
//                CompletionGrowthPercentage = CalculateGrowth(currentCompletionRate, previousCompletionRate),
//                RevenueGrowthPercentage = CalculateGrowth(currentRevenue, previousRevenue),
//                CurrentMonthLabel = currentMonthStart.ToString("MMM")
//            };

//            return Ok(result);
//        }

//        // =====================================================
//        // GET: /api/admin/reports/monthly-usage
//        // =====================================================
//        [HttpGet("monthly-usage")]
//        public async Task<IActionResult> GetMonthlyUsage()
//        {
//            var now = DateTime.UtcNow;
//            var months = new[]
//            {
//                new DateTime(now.Year, now.Month, 1).AddMonths(-2),
//                new DateTime(now.Year, now.Month, 1).AddMonths(-1),
//                new DateTime(now.Year, now.Month, 1)
//            };

//            var bookings = await _context.Bookings.ToListAsync();

//            var result = months.Select(monthStart =>
//            {
//                var monthEnd = monthStart.AddMonths(1);

//                var monthBookings = bookings
//                    .Where(b => b.BookingDate >= monthStart && b.BookingDate < monthEnd)
//                    .ToList();

//                return new MonthlyUsageItemDto
//                {
//                    Month = monthStart.ToString("MMM"),
//                    TotalRequests = monthBookings.Count,
//                    CompletedRequests = monthBookings.Count(b => b.Status == "Completed")
//                };
//            }).ToList();

//            return Ok(result);
//        }

//        // =====================================================
//        // GET: /api/admin/reports/revenue-trend
//        // =====================================================
//        [HttpGet("revenue-trend")]
//        public async Task<IActionResult> GetRevenueTrend()
//        {
//            var now = DateTime.UtcNow;
//            var months = new[]
//            {
//                new DateTime(now.Year, now.Month, 1).AddMonths(-2),
//                new DateTime(now.Year, now.Month, 1).AddMonths(-1),
//                new DateTime(now.Year, now.Month, 1)
//            };

//            var payments = await _context.Payments
//                .Where(p => p.Status == "Paid")
//                .ToListAsync();

//            var result = months.Select(monthStart =>
//            {
//                var monthEnd = monthStart.AddMonths(1);

//                return new RevenueTrendItemDto
//                {
//                    Month = monthStart.ToString("MMM"),
//                    Revenue = payments
//                        .Where(p => p.CreatedAt >= monthStart && p.CreatedAt < monthEnd)
//                        .Sum(p => p.Amount)
//                };
//            }).ToList();

//            return Ok(result);
//        }

//        // =====================================================
//        // GET: /api/admin/reports/nurse-performance
//        // =====================================================
//        [HttpGet("nurse-performance")]
//        public async Task<IActionResult> GetNursePerformance()
//        {
//            var completedBookings = await _context.Bookings
//                .Include(b => b.Nurse)
//                .Where(b => b.Status == "Completed")
//                .ToListAsync();

//            var reviews = await _context.Reviews.ToListAsync();
//            var payments = await _context.Payments
//                .Where(p => p.Status == "Paid")
//                .ToListAsync();

//            var result = completedBookings
//                .Where(b => b.Nurse != null)
//                .GroupBy(b => new { b.NurseId, NurseName = b.Nurse.FullName })
//                .Select(g =>
//                {
//                    var bookingIds = g.Select(x => x.BookingId).ToList();

//                    var relatedReviews = reviews
//                        .Where(r => bookingIds.Contains(r.BookingId))
//                        .ToList();

//                    var relatedPayments = payments
//                        .Where(p => bookingIds.Contains(p.BookingId))
//                        .ToList();

//                    return new NursePerformanceItemDto
//                    {
//                        NurseName = g.Key.NurseName ?? "Unknown Nurse",
//                        CompletedRequests = g.Count(),

//                        AverageRating = relatedReviews.Any()
//                         ? Math.Round((decimal)relatedReviews.Average(r => r.Rating), 1)
//                         : 0m,

//                        RevenueGenerated = relatedPayments.Sum(p => p.Amount)
//                    };
//                })
//                .OrderByDescending(x => x.CompletedRequests)
//                .Take(10)
//                .ToList();

//            return Ok(result);
//        }

//        // =====================================================
//        // GET: /api/admin/reports/export-pdf
//        // =====================================================
//        [HttpGet("export-pdf")]
//        public async Task<IActionResult> ExportReportsPdf()
//        {
//            var overview = await GetOverviewDataInternal();
//            var monthlyUsage = await GetMonthlyUsageDataInternal();
//            var revenueTrend = await GetRevenueTrendDataInternal();
//            var nursePerformance = await GetNursePerformanceDataInternal();

//            var pdfBytes = Document.Create(container =>
//            {
//                container.Page(page =>
//                {
//                    page.Size(PageSizes.A4);
//                    page.Margin(20);
//                    page.PageColor(Colors.White);
//                    page.DefaultTextStyle(x => x.FontSize(10));

//                    page.Header().Column(column =>
//                    {
//                        column.Item().Text("Reports & Analytics")
//                            .FontSize(18)
//                            .Bold();

//                        column.Item().Text($"Generated on: {DateTime.Now:yyyy-MM-dd hh:mm tt}")
//                            .FontSize(10)
//                            .FontColor(Colors.Grey.Darken1);
//                    });

//                    page.Content().Column(column =>
//                    {
//                        column.Spacing(12);

//                        column.Item().Text("Overview").Bold().FontSize(14);

//                        column.Item().Text(
//                            $"Total Requests ({overview.CurrentMonthLabel}): {overview.TotalRequests}\n" +
//                            $"Completion Rate: {overview.CompletionRate}%\n" +
//                            $"Revenue ({overview.CurrentMonthLabel}): ${overview.Revenue:0.00}");

//                        column.Item().Text("Monthly Usage").Bold().FontSize(14);

//                        column.Item().Table(table =>
//                        {
//                            table.ColumnsDefinition(columns =>
//                            {
//                                columns.RelativeColumn();
//                                columns.RelativeColumn();
//                                columns.RelativeColumn();
//                            });

//                            table.Header(header =>
//                            {
//                                header.Cell().Element(CellStyle).Text("Month").Bold();
//                                header.Cell().Element(CellStyle).Text("Total Requests").Bold();
//                                header.Cell().Element(CellStyle).Text("Completed").Bold();
//                            });

//                            foreach (var item in monthlyUsage)
//                            {
//                                table.Cell().Element(CellStyle).Text(item.Month);
//                                table.Cell().Element(CellStyle).Text(item.TotalRequests.ToString());
//                                table.Cell().Element(CellStyle).Text(item.CompletedRequests.ToString());
//                            }
//                        });

//                        column.Item().Text("Revenue Trend").Bold().FontSize(14);

//                        column.Item().Table(table =>
//                        {
//                            table.ColumnsDefinition(columns =>
//                            {
//                                columns.RelativeColumn();
//                                columns.RelativeColumn();
//                            });

//                            table.Header(header =>
//                            {
//                                header.Cell().Element(CellStyle).Text("Month").Bold();
//                                header.Cell().Element(CellStyle).Text("Revenue").Bold();
//                            });

//                            foreach (var item in revenueTrend)
//                            {
//                                table.Cell().Element(CellStyle).Text(item.Month);
//                                table.Cell().Element(CellStyle).Text($"${item.Revenue:0.00}");
//                            }
//                        });

//                        column.Item().Text("Nurse Performance Summary").Bold().FontSize(14);

//                        column.Item().Table(table =>
//                        {
//                            table.ColumnsDefinition(columns =>
//                            {
//                                columns.RelativeColumn(2);
//                                columns.RelativeColumn();
//                                columns.RelativeColumn();
//                                columns.RelativeColumn();
//                            });

//                            table.Header(header =>
//                            {
//                                header.Cell().Element(CellStyle).Text("Nurse Name").Bold();
//                                header.Cell().Element(CellStyle).Text("Completed Requests").Bold();
//                                header.Cell().Element(CellStyle).Text("Average Rating").Bold();
//                                header.Cell().Element(CellStyle).Text("Revenue Generated").Bold();
//                            });

//                            foreach (var item in nursePerformance)
//                            {
//                                table.Cell().Element(CellStyle).Text(item.NurseName);
//                                table.Cell().Element(CellStyle).Text(item.CompletedRequests.ToString());
//                                table.Cell().Element(CellStyle).Text(item.AverageRating.ToString("0.0"));
//                                table.Cell().Element(CellStyle).Text($"${item.RevenueGenerated:0.00}");
//                            }
//                        });
//                    });

//                    page.Footer()
//                        .AlignCenter()
//                        .Text(text =>
//                        {
//                            text.Span("Page ");
//                            text.CurrentPageNumber();
//                            text.Span(" of ");
//                            text.TotalPages();
//                        });
//                });

//                static IContainer CellStyle(IContainer container)
//                {
//                    return container
//                        .Border(1)
//                        .BorderColor(Colors.Grey.Lighten2)
//                        .Padding(4);
//                }
//            }).GeneratePdf();

//            return File(pdfBytes, "application/pdf", "reports-analytics.pdf");
//        }

//        private async Task<ReportsOverviewDto> GetOverviewDataInternal()
//        {
//            var now = DateTime.UtcNow;
//            var currentMonthStart = new DateTime(now.Year, now.Month, 1);
//            var previousMonthStart = currentMonthStart.AddMonths(-1);
//            var nextMonthStart = currentMonthStart.AddMonths(1);

//            var currentMonthBookings = await _context.Bookings
//                .Where(b => b.BookingDate >= currentMonthStart && b.BookingDate < nextMonthStart)
//                .ToListAsync();

//            var previousMonthBookings = await _context.Bookings
//                .Where(b => b.BookingDate >= previousMonthStart && b.BookingDate < currentMonthStart)
//                .ToListAsync();

//            var currentMonthPayments = await _context.Payments
//                .Where(p => p.CreatedAt >= currentMonthStart && p.CreatedAt < nextMonthStart && p.Status == "Paid")
//                .ToListAsync();

//            var previousMonthPayments = await _context.Payments
//                .Where(p => p.CreatedAt >= previousMonthStart && p.CreatedAt < currentMonthStart && p.Status == "Paid")
//                .ToListAsync();

//            var currentTotalRequests = currentMonthBookings.Count;
//            var previousTotalRequests = previousMonthBookings.Count;

//            var currentCompletedRequests = currentMonthBookings.Count(b => b.Status == "Completed");
//            var previousCompletedRequests = previousMonthBookings.Count(b => b.Status == "Completed");

//            var currentCompletionRate = currentTotalRequests == 0
//                ? 0
//                : Math.Round((decimal)currentCompletedRequests / currentTotalRequests * 100, 1);

//            var previousCompletionRate = previousTotalRequests == 0
//                ? 0
//                : Math.Round((decimal)previousCompletedRequests / previousTotalRequests * 100, 1);

//            var currentRevenue = currentMonthPayments.Sum(p => p.Amount);
//            var previousRevenue = previousMonthPayments.Sum(p => p.Amount);

//            decimal CalculateGrowth(decimal currentValue, decimal previousValue)
//            {
//                if (previousValue == 0)
//                    return currentValue > 0 ? 100 : 0;

//                return Math.Round(((currentValue - previousValue) / previousValue) * 100, 1);
//            }

//            return new ReportsOverviewDto
//            {
//                TotalRequests = currentTotalRequests,
//                CompletionRate = currentCompletionRate,
//                Revenue = currentRevenue,
//                RequestsGrowthPercentage = CalculateGrowth(currentTotalRequests, previousTotalRequests),
//                CompletionGrowthPercentage = CalculateGrowth(currentCompletionRate, previousCompletionRate),
//                RevenueGrowthPercentage = CalculateGrowth(currentRevenue, previousRevenue),
//                CurrentMonthLabel = currentMonthStart.ToString("MMM")
//            };
//        }

//        private async Task<List<MonthlyUsageItemDto>> GetMonthlyUsageDataInternal()
//        {
//            var now = DateTime.UtcNow;
//            var months = new[]
//            {
//                new DateTime(now.Year, now.Month, 1).AddMonths(-2),
//                new DateTime(now.Year, now.Month, 1).AddMonths(-1),
//                new DateTime(now.Year, now.Month, 1)
//            };

//            var bookings = await _context.Bookings.ToListAsync();

//            return months.Select(monthStart =>
//            {
//                var monthEnd = monthStart.AddMonths(1);

//                var monthBookings = bookings
//                    .Where(b => b.BookingDate >= monthStart && b.BookingDate < monthEnd)
//                    .ToList();

//                return new MonthlyUsageItemDto
//                {
//                    Month = monthStart.ToString("MMM"),
//                    TotalRequests = monthBookings.Count,
//                    CompletedRequests = monthBookings.Count(b => b.Status == "Completed")
//                };
//            }).ToList();
//        }

//        private async Task<List<RevenueTrendItemDto>> GetRevenueTrendDataInternal()
//        {
//            var now = DateTime.UtcNow;
//            var months = new[]
//            {
//                new DateTime(now.Year, now.Month, 1).AddMonths(-2),
//                new DateTime(now.Year, now.Month, 1).AddMonths(-1),
//                new DateTime(now.Year, now.Month, 1)
//            };

//            var payments = await _context.Payments
//                .Where(p => p.Status == "Paid")
//                .ToListAsync();

//            return months.Select(monthStart =>
//            {
//                var monthEnd = monthStart.AddMonths(1);

//                return new RevenueTrendItemDto
//                {
//                    Month = monthStart.ToString("MMM"),
//                    Revenue = payments
//                        .Where(p => p.CreatedAt >= monthStart && p.CreatedAt < monthEnd)
//                        .Sum(p => p.Amount)
//                };
//            }).ToList();
//        }

//        private async Task<List<NursePerformanceItemDto>> GetNursePerformanceDataInternal()
//        {
//            var completedBookings = await _context.Bookings
//                .Include(b => b.Nurse)
//                .Where(b => b.Status == "Completed")
//                .ToListAsync();

//            var reviews = await _context.Reviews.ToListAsync();
//            var payments = await _context.Payments
//                .Where(p => p.Status == "Paid")
//                .ToListAsync();

//            return completedBookings
//                .Where(b => b.Nurse != null)
//                .GroupBy(b => new { b.NurseId, NurseName = b.Nurse.FullName })
//                .Select(g =>
//                {
//                    var bookingIds = g.Select(x => x.BookingId).ToList();

//                    var relatedReviews = reviews
//                        .Where(r => bookingIds.Contains(r.BookingId))
//                        .ToList();

//                    var relatedPayments = payments
//                        .Where(p => bookingIds.Contains(p.BookingId))
//                        .ToList();

//                    return new NursePerformanceItemDto
//                    {
//                        NurseName = g.Key.NurseName ?? "Unknown Nurse",
//                        CompletedRequests = g.Count(),

//                        AverageRating = relatedReviews.Any()
//                        ? Math.Round((decimal)relatedReviews.Average(r => r.Rating), 1)
//                        : 0m,

//                        RevenueGenerated = relatedPayments.Sum(p => p.Amount)
//                    };
//                })
//                .OrderByDescending(x => x.CompletedRequests)
//                .Take(10)
//                .ToList();
//        }
//    }
//}