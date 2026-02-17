namespace SmartVocab.Application.DTOs.Study
{
    public class LogInteractionDto
    {
        public Guid WordId { get; set; }
        public bool IsCorrect { get; set; }
        public int ResponseTimeMs { get; set; }
        public int Difficulty { get; set; } // 1-5
        
        // Yeni Bilimsel Veriler
        public bool IsAudioPlayed { get; set; }
        public int FocusLostCount { get; set; }
        public int SessionDurationSeconds { get; set; }
        public int LocalHour { get; set; } // Frontend saati (TSİ) gönderecek
        public string UiMode { get; set; } = "Standard";
    }
}