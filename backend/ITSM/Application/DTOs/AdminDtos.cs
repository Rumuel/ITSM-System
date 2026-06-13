using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class UpdateUserRolesRequest
    {
        [Required]
        public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
    }
}
