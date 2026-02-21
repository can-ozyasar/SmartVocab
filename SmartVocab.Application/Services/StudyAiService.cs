using System;
using System.Threading.Tasks;
using SmartVocab.Application.DTOs.Telemetry;
using SmartVocab.Application.Interfaces; // Interface'i ekledik
using SmartVocab.Domain.Entities;
using System.Collections.Generic;
using System.Linq;

// DİKKAT: Microsoft.EntityFrameworkCore YOK! ApplicationDbContext YOK!

namespace SmartVocab.Application.Services
{
    public class StudyAiService
    {
        private readonly IStudyRepository _repository; // DbContext yerine Interface kullanıyoruz

        public StudyAiService(IStudyRepository repository)
        {
            _repository = repository;
        }
    

// ...

public async Task<List<StudyBlockDto>> GetTodayStudyBlocksAsync(Guid userId, int limit = 20)
{
    // 1. Veritabanından zamanı gelmiş kelimeleri çek
    var dueStates = await _repository.GetDueWordsAsync(userId, limit);

    // 2. Kelimeleri AI'ın belirlediği temalara göre grupla (Kümeleme/Chunking)
    var groupedBlocks = dueStates
        .GroupBy(state => string.IsNullOrEmpty(state.BestStudyTheme) ? "neutral" : state.BestStudyTheme)
        .Select(group => new StudyBlockDto
        {
            Theme = group.Key,
            
            // Şimdilik Labs'taki diğer özellikleri temaya göre mantıksal eşleştiriyoruz.
            // Örn: Mor temada derin odak (Binaural) aç, Nötr temada animasyonları (Fluid) kapat.
            BinauralBeats = group.Key == "purple" || group.Key == "blue", 
            FluidFocus = group.Key != "neutral", 
            
            WordCount = group.Count(),
            
            Words = group.Select(w => new StudyBlockWordDto
            {
                WordId = w.WordId,
                Text = w.Word.Text,
                Meaning = w.Word.Meaning
            }).ToList()
        })
        .OrderByDescending(b => b.WordCount) // En çok kelime olan blok ilk başlasın
        .ToList();

        return groupedBlocks;
        }

        public async Task ProcessTelemetryAsync(Guid userId, SessionTelemetryDto telemetry)
        {
            foreach (var log in telemetry.Interactions)
            {
                // 1. Veritabanından kelimeyi getir (Interface üzerinden)
                var state = await _repository.GetUserWordStateAsync(userId, log.WordId);

                if (state == null)
                {
                    state = new UserWordState
                    {
                        UserId = userId,
                        WordId = log.WordId,
                        IsLearning = true,
                        NextReviewDate = DateTime.UtcNow,
                        EasinessFactor = 2.5
                    };
                    await _repository.AddUserWordStateAsync(state); // Interface üzerinden ekle
                }

                // 2. Kalite puanı hesapla
                int quality;
                if (log.Outcome == "unknown") quality = 0;
                else 
                {
                    if (log.ConfidenceScore > 0.9) quality = 5;
                    else if (log.ConfidenceScore > 0.5) quality = 4;
                    else quality = 3;
                }

                if (telemetry.Settings.FluidMode && quality == 5)
                {
                    state.BestStudyTheme = telemetry.Settings.Theme;
                }

                // 3. SM-2 Algoritması
                UpdateWordState(state, quality);
                state.LastReviewedAt = DateTime.UtcNow;
            }

            // Değişiklikleri kaydet
            await _repository.SaveChangesAsync();
        }

        private void UpdateWordState(UserWordState state, int quality)
        {
            if (quality < 3)
            {
                state.RepetitionCount = 0;
                state.IntervalDays = 1;
                state.IsLearning = true;
                state.NextReviewDate = DateTime.UtcNow.AddDays(1);
            }
            else
            {
                if (state.IsLearning)
                {
                    state.IsLearning = false;
                    state.IntervalDays = 1; 
                    state.RepetitionCount = 1;
                }
                else
                {
                    if (state.RepetitionCount == 0) state.IntervalDays = 1;
                    else if (state.RepetitionCount == 1) state.IntervalDays = 6;
                    else state.IntervalDays = (int)Math.Round(state.IntervalDays * state.EasinessFactor);
                    
                    state.RepetitionCount++;
                }

                state.EasinessFactor = state.EasinessFactor + (0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02));
                if (state.EasinessFactor < 1.3) state.EasinessFactor = 1.3;

                state.NextReviewDate = DateTime.UtcNow.AddDays(state.IntervalDays);
            }
        }
    }
}