using SmartVocab.Application.DTOs.Dashboard;
using SmartVocab.Application.Interfaces;
using SmartVocab.Domain.Entities;
using SmartVocab.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartVocab.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IGenericRepository<UserWordLog> _logRepository;
        private readonly IGenericRepository<UserWordState> _stateRepository;
        private readonly IGenericRepository<Word> _wordRepository;

        public DashboardService(
            IGenericRepository<UserWordLog> logRepository,
            IGenericRepository<UserWordState> stateRepository,
            IGenericRepository<Word> wordRepository)
        {
            _logRepository = logRepository;
            _stateRepository = stateRepository;
            _wordRepository = wordRepository;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync(Guid userId)
        {
            // 1. SERİ HESAPLAMA (Streak Algorithm)
            // Kullanıcının çalışma yaptığı tüm tarihleri çek (Sadece tarih kısmı).
            // Not: Performans için normalde SQL tarafında "Distinct Date" yapmak daha iyidir.
            var allLogs = await _logRepository.FindAsync(l => l.UserId == userId);
            
            var studyDates = allLogs
                .Select(l => l.CreatedAt.Date)
                .Distinct()
                .OrderByDescending(d => d) // Bugünden geçmişe doğru sırala
                .ToList();

            int streak = 0;
            var today = DateTime.UtcNow.Date;
            
            // Eğer bugün çalışmadıysa seriye dahil etmek için dünü kontrol etmeliyiz.
            // Ama bugün çalıştıysa seriyi 1 artırmalıyız.
            
            if (studyDates.Any())
            {
                // Eğer son çalışma tarihi bugün veya dün ise seri devam ediyordur.
                // Eğer son çalışma 2 gün önceyse seri bozulmuştur.
                if (studyDates[0] == today || studyDates[0] == today.AddDays(-1))
                {
                    // Döngüyle geriye git
                    // Örn: [Bugün, Dün, Evvelsi Gün...] -> Seri 3
                    var checkDate = studyDates[0]; 
                    streak = 1;

                    for (int i = 1; i < studyDates.Count; i++)
                    {
                        if (studyDates[i] == checkDate.AddDays(-1))
                        {
                            streak++;
                            checkDate = studyDates[i];
                        }
                        else
                        {
                            break; // Zincir koptu
                        }
                    }
                }
            }

            // 2. DİĞER İSTATİSTİKLER
            var states = await _stateRepository.FindAsync(s => s.UserId == userId);
            
            // Öğrenilen: IsLearning=false olanlar (Mezun olmuş kelimeler)
            int learnedCount = states.Count(s => !s.IsLearning);
            
            // Bekleyen Tekrarlar: Vakti gelmiş olanlar
            int dueCount = states.Count(s => s.NextReviewDate <= DateTime.UtcNow);

            // Bugünün süresi
            int todaySeconds = allLogs
                .Where(l => l.CreatedAt.Date == today)
                .Sum(l => l.SessionDurationSeconds); // Session süresi mi yoksa kelime başı süre mi? 
                // Log yapısında SessionDurationSeconds var ama bu o session'ın toplam süresi mi yoksa kelimenin sırası mıydı?
                // Düzeltme: LogInteractionDto'da SessionDurationSeconds o anki saniyeyi tutuyordu.
                // Toplam süreyi ResponseTimeMs üzerinden hesaplamak daha doğru olur (Milisaniye -> Saniye).
            
            int activeStudyTimeSeconds = (int)(allLogs
                .Where(l => l.CreatedAt.Date == today)
                .Sum(l => l.ResponseTimeMs) / 1000.0);


            return new DashboardSummaryDto
            {
                CurrentStreak = streak,
                TotalWordsLearned = learnedCount,
                DueReviewCount = dueCount,
                TodayStudySeconds = activeStudyTimeSeconds
            };
        }

        public async Task<DashboardAnalyticsDto> GetAnalyticsAsync(Guid userId)
        {
            var logs = await _logRepository.FindAsync(l => l.UserId == userId);
            
            if (!logs.Any()) return new DashboardAnalyticsDto(); // Veri yoksa boş dön

            // 1. DOĞRULUK ORANI
            double accuracy = (double)logs.Count(l => l.IsCorrect) / logs.Count() * 100;

            // 2. HAFTALIK AKTİVİTE (Son 7 Gün)
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.UtcNow.Date.AddDays(-i))
                .OrderBy(d => d) // Eskiden yeniye
                .ToList();

            var weeklyActivity = new List<DailyActivityDto>();
            foreach (var date in last7Days)
            {
                weeklyActivity.Add(new DailyActivityDto
                {
                    Date = date.ToString("dd.MM"),
                    WordCount = logs.Count(l => l.CreatedAt.Date == date)
                });
            }

            // 3. SAATLİK ISI HARİTASI (Hangi saatte ne kadar başarılı?)
            var hourlyStats = logs
                .GroupBy(l => l.LocalHour) // Frontend'den gelen saat verisine göre grupla
                .Select(g => new HourlyActivityDto
                {
                    Hour = g.Key,
                    InteractionCount = g.Count(),
                    AverageAccuracy = (double)g.Count(l => l.IsCorrect) / g.Count() * 100
                })
                .OrderBy(h => h.Hour)
                .ToList();

            // 4. SON ÖĞRENİLEN KELİMELER
            // States tablosundan IsLearning=false olan en son 5 kelimeyi bul
            var learnedStates = await _stateRepository.FindAsync(s => s.UserId == userId && !s.IsLearning);
            var lastLearnedIds = learnedStates
                .OrderByDescending(s => s.LastReviewedAt) // En son bakılanlar
                .Take(5)
                .Select(s => s.WordId)
                .ToList();

            var lastWords = new List<string>();
            if (lastLearnedIds.Any())
            {
                var words = await _wordRepository.FindAsync(w => lastLearnedIds.Contains(w.Id));
                lastWords = words.Select(w => w.Text).ToList();
            }

            return new DashboardAnalyticsDto
            {
                AccuracyRate = Math.Round(accuracy, 1),
                WeeklyActivity = weeklyActivity,
                HourlyActivity = hourlyStats,
                LastLearnedWords = lastWords
            };
        }
    }
}