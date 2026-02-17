using System.ComponentModel.DataAnnotations;

namespace SmartVocab.Application.DTOs.User
{
    public class UpdateProfileDto
    {
        [Required]
        public string FirstName { get; set; }
        
        [Required]
        public string LastName { get; set; }
        
        [Range(5, 120)] // Günde en az 5, en çok 120 dk hedef konabilir.
        public int DailyGoalMinutes { get; set; }
        
        public string NativeLanguage { get; set; }
    }
}