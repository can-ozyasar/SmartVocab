using System;
using System.Collections.Generic;

namespace SmartVocab.Application.DTOs.Telemetry
{
    // Frontend'e gidecek ana blok yapısı
    public class StudyBlockDto
    {
        public string Theme { get; set; }
        public bool BinauralBeats { get; set; }
        public bool FluidFocus { get; set; }
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