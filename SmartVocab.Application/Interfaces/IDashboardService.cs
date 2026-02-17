using SmartVocab.Application.DTOs.Dashboard;
using System;
using System.Threading.Tasks;

namespace SmartVocab.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetSummaryAsync(Guid userId);
        Task<DashboardAnalyticsDto> GetAnalyticsAsync(Guid userId);
    }
}