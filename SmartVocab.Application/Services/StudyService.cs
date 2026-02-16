using SmartVocab.Application.DTOs.Study;
using SmartVocab.Application.Interfaces;
using SmartVocab.Domain.Entities;
using SmartVocab.Domain.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SmartVocab.Application.Services
{
    public class StudyService : IStudyService
    {
        private readonly IGenericRepository<UserWordLog> _logRepository;
        private readonly IGenericRepository<UserWordState> _stateRepository;
        private readonly IGenericRepository<Word> _wordRepository; // <-- YENİ EKLENDİ
        private readonly IUnitOfWork _unitOfWork;

        public StudyService(
            IGenericRepository<UserWordLog> logRepository, 
            IGenericRepository<UserWordState> stateRepository,
            IGenericRepository<Word> wordRepository, // <-- YENİ EKLENDİ
            IUnitOfWork unitOfWork)
        {
            _logRepository = logRepository;
            _stateRepository = stateRepository;
            _wordRepository = wordRepository; // <-- YENİ EKLENDİ
            _unitOfWork = unitOfWork;
        }

        public async Task LogInteractionAsync(Guid userId, LogInteractionDto dto)
        {
            // 1. LOGU KAYDET
            var log = new UserWordLog
            {
                UserId = userId,
                WordId = dto.WordId,
                IsCorrect = dto.IsCorrect,
                ResponseTimeMs = dto.ResponseTimeMs,
                SelfRatedDifficulty = dto.Difficulty,
                IsAudioPlayed = dto.IsAudioPlayed,
                FocusLostCount = dto.FocusLostCount,
                SessionDurationSeconds = dto.SessionDurationSeconds,
                LocalHour = dto.LocalHour,
                UiMode = dto.UiMode,
                CreatedAt = DateTime.UtcNow
            };

            await _logRepository.AddAsync(log);

            // 2. SM-2 ALGORİTMASI VE DURUM GÜNCELLEME
            
            var states = await _stateRepository.FindAsync(s => s.UserId == userId && s.WordId == dto.WordId);
            var state = states.FirstOrDefault();

            bool isNewRecord = false; // <-- KİLİT NOKTA: Kayıt yeni mi eski mi takip ediyoruz.

            if (state == null)
            {
                isNewRecord = true;
                state = new UserWordState
                {
                    UserId = userId,
                    WordId = dto.WordId,
                    EasinessFactor = 2.5,
                    IntervalDays = 0,
                    RepetitionCount = 0,
                    Box = 1,
                    NextReviewDate = DateTime.UtcNow
                    // Id burada BaseEntity tarafından otomatik oluşturulur ama DB'de yoktur.
                };
            }

            // ... (Kayıt bulma işlemleri aynı) ...

            // --- YENİ ALGORİTMA: HİBRİT SİSTEM ---
            
            int quality = dto.IsCorrect ? dto.Difficulty : 0;

            // Eğer kullanıcı YANLIŞ yaptıysa (Quality < 3)
            if (quality < 3)
            {
                // Kelimeyi en başa, "Öğrenme Moduna" döndür.
                state.IsLearning = true;
                state.RepetitionCount = 0;
                state.IntervalDays = 0; 
                // NextReviewDate'i hemen 10 dakika sonraya ayarla!
                state.NextReviewDate = DateTime.UtcNow.AddMinutes(10);
            }
            // Eğer kullanıcı DOĞRU bildiyse (Quality >= 3)
            else
            {
                if (state.IsLearning)
                {
                    // Hâlâ öğrenme aşamasındaysa hemen mezun etme.
                    // Bir kez bildi diye 6 gün sonraya atılmaz.
                    // Burayı "Yarın"a atalım.
                    state.IsLearning = false; // Artık öğrenildi, SM-2'ye geçebilir.
                    state.IntervalDays = 1;
                    state.NextReviewDate = DateTime.UtcNow.AddDays(1);
                }
                else
                {
                    // Zaten öğrenilmiş kelime, SM-2 (Uzun Aralıklar) uygula
                    if (state.RepetitionCount == 0) state.IntervalDays = 1;
                    else if (state.RepetitionCount == 1) state.IntervalDays = 6;
                    else state.IntervalDays = (int)Math.Round(state.IntervalDays * state.EasinessFactor);

                    state.RepetitionCount++;
                    state.EasinessFactor = state.EasinessFactor + (0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02));
                    if (state.EasinessFactor < 1.3) state.EasinessFactor = 1.3;

                    state.NextReviewDate = DateTime.UtcNow.AddDays(state.IntervalDays);
                }
            }
            
            state.LastReviewedAt = DateTime.UtcNow;

            // ... (Kaydetme işlemleri aynı) ...


            // --- KARAR ANI (DÜZELTİLEN KISIM) ---
            if (isNewRecord)
            {
                // Eğer yeniyse sadece Ekle
                await _stateRepository.AddAsync(state);
            }
            else
            {
                // Eğer eskiyse sadece Güncelle
                _stateRepository.Update(state);
            }

            // Hepsini tek seferde kaydet
            await _unitOfWork.CommitAsync();
        }



       public async Task<IEnumerable<StudyWordDto>> GetNextSessionWordsAsync(Guid userId, int limit = 10)
        {
            // -----------------------------------------------------------
            // 1. ADIM: AKILLI SIRALAMA (Smart Priority Queue)
            // -----------------------------------------------------------
            
            // Veritabanından "Zamanı Gelmiş" veya "Öğrenme Aşamasında Olan" tüm kelimeleri çek.
            // Not: Performans için normalde IQueryable ile DB tarafında sıralamak daha iyidir
            // ama şimdilik kod karmaşası olmasın diye memory'de sıralayacağız.
            var dueStates = await _stateRepository.FindAsync(s => 
                s.UserId == userId && 
                (s.NextReviewDate <= DateTime.UtcNow || s.IsLearning));

            // BURASI KRİTİK: Kullanıcı 10 dk kalsa bile en verimli 10 dakikayı geçirsin.
            var prioritizedStates = dueStates
                .OrderByDescending(s => s.IsLearning)       // 1. Öncelik: Öğrenme aşamasındakiler (Unutulması en muhtemel)
                .ThenBy(s => s.NextReviewDate)              // 2. Öncelik: En çok gecikmiş olanlar (Tarihi en eski olan en başa)
                .ThenBy(s => s.EasinessFactor)              // 3. Öncelik: En zor kelimeler (EF düşükse zordur)
                .Take(limit)                                // Limit kadar al (Örn: 10 tane)
                .ToList();
            
            var dueWordIds = prioritizedStates.Select(s => s.WordId).ToList();
            
            // Kelime detaylarını çek
            // Not: FindAsync genellikle liste döner, JOIN performansı için ileride Query Object Pattern kullanabiliriz.
            // Şimdilik ID listesiyle çekiyoruz.
            var reviewWords = new List<Word>();
            if (dueWordIds.Any())
            {
                // WordRepository'de "WhereIn" benzeri bir yapı olmadığı için filtreleme yapıyoruz.
                // (Küçük veri setlerinde sorun olmaz)
                var allWords = await _wordRepository.FindAsync(w => dueWordIds.Contains(w.Id));
                
                // Veritabanından gelen kelimeler ID sırasına göre gelebilir.
                // Bizim öncelik sıramızı bozmamaları için tekrar sıralıyoruz.
                reviewWords = allWords
                    .OrderBy(w => dueWordIds.IndexOf(w.Id)) // ID listesindeki sıraya göre diz
                    .ToList();
            }

            var result = new List<StudyWordDto>();

            // Tekrar Kelimelerini DTO'ya çevir
            foreach (var word in reviewWords)
            {
                result.Add(new StudyWordDto
                {
                    Id = word.Id,
                    Text = word.Text,
                    Meaning = word.Meaning,
                    ExampleSentence = word.ExampleSentence,
                    PronunciationUrl = word.PronunciationUrl,
                    Type = word.Type,
                    Level = word.Level,
                    IsReview = true 
                });
            }

            // -----------------------------------------------------------
            // 2. ADIM: YENİ KELİME SERPİŞTİRME (Interleaving)
            // -----------------------------------------------------------
            // Eğer "Acil" kelimeler limiti doldurmadıysa, araya yeni kelime ekleyelim.
            // Kullanıcı hiç girmemişse bile en azından yeni bir şeyler öğrensin.
            
            if (result.Count < limit)
            {
                int needed = limit - result.Count;

                var allUserStates = await _stateRepository.FindAsync(s => s.UserId == userId);
                var knownWordIds = allUserStates.Select(s => s.WordId).ToHashSet(); // HashSet performans için

                // Hiç görmediği kelimelerden rastgele veya sırayla getir
                var newWordsAll = await _wordRepository.FindAsync(w => !knownWordIds.Contains(w.Id));
                
                // Rastgelelik ekleyelim mi? Şimdilik sırayla gidelim, ileride buraya "Shuffle" ekleriz.
                var wordsToAdd = newWordsAll.Take(needed);

                foreach (var word in wordsToAdd)
                {
                    result.Add(new StudyWordDto
                    {
                        Id = word.Id,
                        Text = word.Text,
                        Meaning = word.Meaning,
                        ExampleSentence = word.ExampleSentence,
                        PronunciationUrl = word.PronunciationUrl,
                        Type = word.Type,
                        Level = word.Level,
                        IsReview = false
                    });
                }
            }

            return result;
        }
    }
}