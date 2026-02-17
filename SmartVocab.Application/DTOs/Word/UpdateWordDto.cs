using System;
using System.ComponentModel.DataAnnotations;
using SmartVocab.Domain.Entities;

namespace SmartVocab.Application.DTOs.Word
{
    public class UpdateWordDto
    {
        [Required]
        public Guid Id { get; set; } // Hangi kelimeyi güncelliyoruz?

        [Required]
        public string Text { get; set; }
        
        [Required]
        public string Meaning { get; set; }
        
        public string? ExampleSentence { get; set; }
        public WordType Type { get; set; }
        public CEFRLevel Level { get; set; }
    }
}