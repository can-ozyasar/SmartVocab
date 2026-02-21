using System;
using System.Collections.Generic;

namespace SmartVocab.Application.DTOs.Telemetry
{
    public class SessionTelemetryDto
    {
        public string SessionId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TelemetrySettingsDto Settings { get; set; }
        public List<InteractionLogDto> Interactions { get; set; }
    }

    public class TelemetrySettingsDto
    {
        public string Theme { get; set; }
        public bool FluidMode { get; set; }
        public string FluidType { get; set; }
        public bool Binaural { get; set; }
    }

    public class InteractionLogDto
    {
        public Guid WordId { get; set; }
        public string WordText { get; set; }
        public int TimeSpentMs { get; set; }
        public bool Flipped { get; set; }
        public string Outcome { get; set; }
        public double ConfidenceScore { get; set; }
        public DateTime Timestamp { get; set; }
    }
}