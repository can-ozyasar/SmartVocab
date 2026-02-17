using SmartVocab.Domain.Entities; // Enumlar için
using System.ComponentModel.DataAnnotations;

namespace SmartVocab.Application.DTOs.Word
{
    public class CreateWordDto
    {
        
        [Required]
        public string Text { get; set; } // "Ambiguous"
        
        [Required]
        public string Meaning { get; set; } // "Belirsiz"
        
        public string? ExampleSentence { get; set; }
        
        // Enumları int olarak alacağız (0: Noun, 1: Verb...)
        // Swagger'da kolaylık olsun diye şimdilik böyle.
        public WordType Type { get; set; } 
        public CEFRLevel Level { get; set; }
    }
}