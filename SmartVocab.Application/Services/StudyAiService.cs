using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartVocab.Application.DTOs.Telemetry;
using SmartVocab.Domain.Entities;
// Context namespace'ini buraya ekle (Örn: SmartVocab.Infrastructure.Persistence)

namespace SmartVocab.Application.Services
{
    public class StudyAiService
    {
        // DbContext adın neyse onu kullan (ApplicationDbContext, SmartVocabContext vb.)
        private readonly ApplicationDbContext _context; 

        public StudyAiService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task ProcessTelemetryAsync(Guid userId, SessionTelemetryDto telemetry)
        {
            foreach (var log in telemetry.Interactions)
            {
                // 1. Kullanıcının kelime durumunu bul
                var state = await _context.UserWordStates
                    .FirstOrDefaultAsync(w => w.UserId == userId && w.WordId == log.WordId);

                // Eğer kelimeyi ilk defa görüyorsa kayıt aç
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
                    _context.UserWordStates.Add(state);
                }

                // 2. KALİTE PUANI (Quality) HESAPLA (0 - 5 arası)
                // Frontend'den gelen ConfidenceScore (0.0 - 1.0) SM-2 uyumlu 0-5 puanına çevrilir.
                int quality;
                if (log.Outcome == "unknown") 
                {
                    quality = 0; // Bilemedi (0)
                }
                else 
                {
                    // Bildi: Confidence 0.0-0.5 arası -> 3 (Zorlanarak Geçti)
                    // Confidence 0.5-0.9 arası -> 4 (İyi)
                    // Confidence > 0.9 -> 5 (Mükemmel - Refleks)
                    if (log.ConfidenceScore > 0.9) quality = 5;
                    else if (log.ConfidenceScore > 0.5) quality = 4;
                    else quality = 3;
                }

                // --- LABS AI BONUSU ---
                // Eğer Akışkan Mod (Fluid Mode) açıksa ve kullanıcı "Mükemmel" bildiyse
                // Bu temayı "BestStudyTheme" olarak kaydet.
                if (telemetry.Settings.FluidMode && quality == 5)
                {
                    state.BestStudyTheme = telemetry.Settings.Theme;
                    // Gelecekte: Kullanıcıya zorlandığı kelimeleri bu temada soracağız.
                }

                // 3. TARİH HESAPLA (SM-2 Modifiye)
                UpdateWordState(state, quality);

                state.LastReviewedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        private void UpdateWordState(UserWordState state, int quality)
        {
            // SENARYO A: Kullanıcı BİLEMEDİ (Quality < 3)
            if (quality < 3)
            {
                state.RepetitionCount = 0;
                state.IntervalDays = 1; // 1 gün sonra tekrar sor
                state.IsLearning = true; // Tekrar öğrenme moduna al
                state.NextReviewDate = DateTime.UtcNow.AddDays(1);
            }
            // SENARYO B: Kullanıcı BİLDİ (Quality >= 3)
            else
            {
                // Eğer hala "Öğrenme Modu"ndaysa (IsLearning = true)
                if (state.IsLearning)
                {
                    // Artık ezber moduna terfi etti
                    state.IsLearning = false;
                    state.IntervalDays = 1; 
                    state.RepetitionCount = 1;
                }
                else
                {
                    // Zaten ezber modundaydı, aralığı açıyoruz (Exponential Growth)
                    if (state.RepetitionCount == 0) state.IntervalDays = 1;
                    else if (state.RepetitionCount == 1) state.IntervalDays = 6;
                    else
                    {
                        // Önceki aralık * Zorluk Katsayısı
                        state.IntervalDays = (int)Math.Round(state.IntervalDays * state.EasinessFactor);
                    }
                    state.RepetitionCount++;
                }

                // Zorluk Katsayısını (EasinessFactor) Güncelle
                // Kolay bildikçe artar, zorlandıkça düşer (min 1.3)
                state.EasinessFactor = state.EasinessFactor + (0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02));
                if (state.EasinessFactor < 1.3) state.EasinessFactor = 1.3;

                // Sonraki tarihi belirle
                state.NextReviewDate = DateTime.UtcNow.AddDays(state.IntervalDays);
            }
        }
    }
}