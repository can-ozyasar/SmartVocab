using SmartVocab.Domain.Common;
using System;

namespace SmartVocab.Domain.Entities
{
    public class UserWordState : BaseEntity
    {
        //  ilişkiler (Foreign Keys) 
        // Hangi Kullanıcı?
        public Guid UserId { get; set; }
        public User User { get; set; }

        // Hangi Kelime?
        public Guid WordId { get; set; }
        public Word Word { get; set; }

        // --- HAFIZA ALGORİTMASI VERİLERİ (SuperMemo-2 / Leitner) ---

        // Box: Leitner kutusu (1-5 arası). 1: Her gün, 5: Ayda bir.
        public int Box { get; set; } = 1;

        // NextReviewDate: Bu kelime kullanıcıya en erken ne zaman sorulmalı?
        // Eğer DateTime.UtcNow > NextReviewDate ise kelime "Due" (Zamanı gelmiş) olur.
        public DateTime NextReviewDate { get; set; } = DateTime.UtcNow;

        // LastReviewDate: En son ne zaman baktı?
        public DateTime? LastReviewedAt { get; set; }

        // --- SM-2 Algoritması Parametreleri ---
        // E-Factor (Easiness Factor): Kelimenin zorluk katsayısı.
        // Standart SM-2'de 2.5 ile başlar. Düşerse zorlaşır, artarsa kolaylaşır.
        public double EasinessFactor { get; set; } = 2.5;

        // Interval: Bir sonraki tekrar için gün sayısı.
        public int IntervalDays { get; set; } = 0;

        // RepetitionCount: Toplam kaç kere doğru bildi?
        public int RepetitionCount { get; set; } = 0;

        
        // Kelime "Öğrenme Aşamasında" mı?
        // True ise: Dakika bazlı sorulur (1 dk, 10 dk).
        // False ise: Gün bazlı sorulur (SM-2 devreye girer).
        public bool IsLearning { get; set; } = true;
    }
}