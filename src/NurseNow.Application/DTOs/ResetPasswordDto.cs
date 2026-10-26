using System.ComponentModel.DataAnnotations;

namespace NurseNow.DTOs
{
    public class ResetPasswordDto
    {
        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public string NewPassword { get; set; } = string.Empty;
    }
}
