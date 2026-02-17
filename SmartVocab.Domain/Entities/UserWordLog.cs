using SmartVocab.Domain.Common;
using System;

namespace SmartVocab.Domain.Entities
{
    public class UserWordLog : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid WordId { get; set; }

        // --- TEMEL PERFORMANS ---
        public bool IsCorrect { get; set; }
        public int ResponseTimeMs { get; set; } // Düşünme süresi
        public int SelfRatedDifficulty { get; set; } // 1-5 arası

        // --- BİLİŞSEL YÜK VE BAĞLAM (Senin Eklediklerin) ---
        
        // Kullanıcı kelimeye çalışırken telaffuz butonuna bastı mı?
        // (İşitsel hafıza devrede miydi?)
        public bool IsAudioPlayed { get; set; }

        // Kullanıcı bu kart açıkken başka sekmeye/uygulamaya geçti mi?
        // (Dikkat dağınıklığı verisi)
        public int FocusLostCount { get; set; } 

        // Oturumun kaçıncı dakikası? 
        // (Zihinsel yorgunluk analizi için)
        public int SessionDurationSeconds { get; set; }

        // O anki saat dilimi (0-23)
        // (Sabah insanı mı gece kuşu mu analizi için)
        public int LocalHour { get; set; }

        // Frontend Teması: "Dark", "Light", "Gamified"
        // (Hangi arayüzde daha iyi öğreniyor?)
        public string UiMode { get; set; } 
    }
}