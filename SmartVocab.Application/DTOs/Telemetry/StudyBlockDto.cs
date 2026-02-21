using System;
using System.Collections.Generic;

namespace SmartVocab.Application.DTOs.Telemetry
{
   // SmartVocab.Application/DTOs/Telemetry/StudyBlockDto.cs
public class StudyBlockDto
{
    public string Theme { get; set; }
    public bool BinauralBeats { get; set; }
    public int MusicVolume { get; set; } // YENİ: Ses Şiddeti
    public bool FluidFocus { get; set; }
    public string FluidFocusType { get; set; } // YENİ: stars, ripple, breathing
    public bool EnableFliplessMastery { get; set; } // YENİ: Hızlı Onay
    public int WordCount { get; set; }
    public List<StudyBlockWordDto> Words { get; set; }
}

    // O bloğun içindeki kelimelerin yapısı
    public class StudyBlockWordDto
    {
        public Guid WordId { get; set; }
        public string Text { get; set; }
        public string Meaning { get; set; }
    }
}