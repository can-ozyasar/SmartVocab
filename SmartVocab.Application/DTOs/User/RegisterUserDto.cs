using System;
using System.ComponentModel.DataAnnotations;

namespace SmartVocab.Application.DTOs.User
{
    // Kullanıcıdan kayıt olurken isteyeceğimiz veriler.
    public class RegisterUserDto
    {
        [Required]
        public string FirstName { get; set; }
        
        [Required]
        public string LastName { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; } // Buraya "Ham" şifre gelecek.
        
        public int DailyGoalMinutes { get; set; } = 15;
    }
}