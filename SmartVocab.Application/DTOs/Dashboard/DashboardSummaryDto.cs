namespace SmartVocab.Application.DTOs.Dashboard
{
    public class DashboardSummaryDto
    {
        // Motivasyon için en önemli veri: Günlük Seri!
        public int CurrentStreak { get; set; }
        
        // Toplam Öğrenilen (IsLearning = false olanlar)
        public int TotalWordsLearned { get; set; }
        
        // Bugün çalışılan süre (saniye cinsinden)
        public int TodayStudySeconds { get; set; }
        
        // Bugün tekrar edilmesi gereken kaç kelime var?
        // (Kullanıcıya "Hadi bitir şunları" demek için)
        public int DueReviewCount { get; set; }
    }
}