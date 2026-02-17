using System.ComponentModel.DataAnnotations;

namespace SmartVocab.Application.DTOs.User
{
    public class ChangePasswordDto
    {
        [Required]
        public string OldPassword { get; set; }

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }
        
        [Required]
        [Compare("NewPassword")] // Yeni şifre ile aynı mı kontrolü
        public string ConfirmNewPassword { get; set; }
    }
}