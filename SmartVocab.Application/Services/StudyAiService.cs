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
    
// Metodun parametresine "bool isVanilla = false" eklendi!
public async Task<List<StudyBlockDto>> GetTodayStudyBlocksAsync(Guid userId, int limit = 20, bool isVanilla = false)
{
    // 1. Veritabanından eski ve yeni kelimeleri çekme (Burası eski kodun aynısı)
    var dueStates = await _repository.GetDueWordsAsync(userId, limit);
    
    int remainingLimit = limit - dueStates.Count;
    var newWords = new List<Word>();

    if (remainingLimit > 0)
    {
        newWords = await _repository.GetNewWordsAsync(userId, remainingLimit);
    }

    var combinedList = dueStates.Select(state => new 
    {
        Word = state.Word,
        Theme = string.IsNullOrEmpty(state.BestStudyTheme) ? "neutral" : state.BestStudyTheme
    }).ToList();

    combinedList.AddRange(newWords.Select(word => new { Word = word, Theme = "neutral" }));
    if (combinedList.Count == 0)
    {
        return new List<StudyBlockDto>();
    }
    // --- YENİ EKLENEN KISIMLAR BURADAN BAŞLIYOR ---

    // 2. KONTROL GRUBU: Kullanıcı "Sade Mod" (Vanilla) istediyse her şeyi kapat!
    if (isVanilla)
    {
        return new List<StudyBlockDto>
        {
            new StudyBlockDto
            {
                Theme = "neutral",
                BinauralBeats = false,
                MusicVolume = 0,
                FluidFocus = false,
                FluidFocusType = "none",
                EnableFliplessMastery = false,
                WordCount = combinedList.Count,
                Words = combinedList.Select(x => new StudyBlockWordDto { WordId = x.Word.Id, Text = x.Word.Text, Meaning = x.Word.Meaning }).ToList()
            }
        };
    }

    // 3. AI MODU: Gruplama ve Zengin Özellikleri Temalara Dağıtma
    var groupedBlocks = combinedList
        .GroupBy(x => x.Theme)
        .Select(group => new StudyBlockDto
        {
            Theme = group.Key,
            
            // Müzik sadece mor ve mavide çalsın, şiddetleri farklı olsun
            BinauralBeats = group.Key == "purple" || group.Key == "blue", 
            MusicVolume = group.Key == "purple" ? 40 : (group.Key == "blue" ? 30 : 0),
            
            // Animasyon türleri temaya göre değişsin (Maviye su dalgası, Yeşile nefes, diğerlerine yıldız)
            FluidFocus = group.Key != "neutral", 
            FluidFocusType = group.Key == "blue" ? "ripple" : (group.Key == "green" ? "breathing" : "stars"),
            
            // Hızlı onay (Flipless Mastery) sadece yüksek odaklı temalarda (Mor ve Turuncu) aktif olsun
            EnableFliplessMastery = group.Key == "purple" || group.Key == "orange", 

            WordCount = group.Count(),
            Words = group.Select(x => new StudyBlockWordDto { WordId = x.Word.Id, Text = x.Word.Text, Meaning = x.Word.Meaning }).ToList()
        })
        .OrderByDescending(b => b.WordCount) // En kalabalık grup önce başlasın
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