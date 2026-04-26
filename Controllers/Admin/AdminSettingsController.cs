//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using NurseNow.Data;
//using NurseNow.DTOs.Admin;
//using NurseNow.Models;

//namespace NurseNow.Controllers.Admin
//{
//    [ApiController]
//    [Route("api/admin/settings")]
//    [Authorize(Roles = "Administrator")]
//    public class AdminSettingsController : ControllerBase
//    {
//        private readonly ApplicationDbContext _context;

//        public AdminSettingsController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetSettings()
//        {
//            var settings = await _context.SystemSettings.FirstOrDefaultAsync();

//            if (settings == null)
//            {
//                settings = new SystemSetting();
//                _context.SystemSettings.Add(settings);
//                await _context.SaveChangesAsync();
//            }

//            return Ok(settings);
//        }

//        [HttpPut]
//        public async Task<IActionResult> UpdateSettings([FromBody] SystemSettingsDto dto)
//        {
//            var settings = await _context.SystemSettings.FirstOrDefaultAsync();

//            if (settings == null)
//            {
//                settings = new SystemSetting();
//                _context.SystemSettings.Add(settings);
//            }

//            settings.AutoAssignNurse = dto.AutoAssignNurse;
//            settings.RequireDocumentVerification = dto.RequireDocumentVerification;
//            settings.EmailNotifications = dto.EmailNotifications;

//            settings.NotifyNewNurse = dto.NotifyNewNurse;
//            settings.NotifyNewServiceRequest = dto.NotifyNewServiceRequest;
//            settings.NotifyNewComplaint = dto.NotifyNewComplaint;

//            settings.SessionTimeout = dto.SessionTimeout;
//            settings.MinimumPasswordLength = dto.MinimumPasswordLength;

//            await _context.SaveChangesAsync();

//            return Ok(new { message = "Settings updated successfully." });
//        }
//    }
//}