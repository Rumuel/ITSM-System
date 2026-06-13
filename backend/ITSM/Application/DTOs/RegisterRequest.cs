namespace Application.DTOs
{
    using System.ComponentModel.DataAnnotations;

    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;
    }
}
