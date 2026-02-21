using System.Collections.Generic;

namespace SmartVocab.Application.DTOs.Dashboard
{
    public class DashboardAnalyticsDto
    {
        // Doğruluk Oranı (örn: %85)
        public double AccuracyRate { get; set; }
        
        // Son 7 günün çalışma grafiği (Pzt: 10, Sal: 20...)
        public List<DailyActivityDto> WeeklyActivity { get; set; }
        
        // Isı Haritası için: Hangi saatlerde daha aktif? (0-23 arası)
        public List<HourlyActivityDto> HourlyActivity { get; set; }
        
        // En son öğrenilen 5 kelime
        public List<string> LastLearnedWords { get; set; }
        // --- YENİ EKLENEN VİZYONER VERİLER ---
        public int CurrentStreak { get; set; } 
        public List<string> StrugglingWords { get; set; } 
        public string BestTheme { get; set; }
    }

    public class DailyActivityDto
    {
        public string Date { get; set; } // "17.02.2026"
        public int WordCount { get; set; }
    }

    public class HourlyActivityDto
    {
        public int Hour { get; set; } // 0-23
        public int InteractionCount { get; set; }
        public double AverageAccuracy { get; set; } // O saatteki başarı oranı
    }
}