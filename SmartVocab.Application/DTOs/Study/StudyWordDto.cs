using SmartVocab.Domain.Entities; // Enumlar için
using System;

namespace SmartVocab.Application.DTOs.Study
{
    public class StudyWordDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public string Meaning { get; set; }
        public string ExampleSentence { get; set; }
        public string PronunciationUrl { get; set; }
        public WordType Type { get; set; }
        public CEFRLevel Level { get; set; }
        
        // Frontend bilsin: Bu kelime yeni mi yoksa tekrar mı?
        // (Buna göre "New" etiketi basabilir)
        public bool IsReview { get; set; } 
    }
}