using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartVocab.Application.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartVocab.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var data = await _dashboardService.GetSummaryAsync(userId);
            return Ok(data);
        }

        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var data = await _dashboardService.GetAnalyticsAsync(userId);
            return Ok(data);
        }
    }
}