using System;

namespace SmartVocab.Application.DTOs.User
{
    public class UserProfileDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int DailyGoalMinutes { get; set; }
        public string NativeLanguage { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}