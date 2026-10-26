using System.ComponentModel.DataAnnotations;

namespace NurseNow.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty; // Patient or Nurse
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
