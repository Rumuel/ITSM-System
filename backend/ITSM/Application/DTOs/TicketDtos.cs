namespace Application.DTOs
{
    using System.ComponentModel.DataAnnotations;

    public class CreateTicketRequest
    {
        [Required]
        [StringLength(250, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Range(1, 4)]
        public int Priority { get; set; } = 2;

        public int? CategoryId { get; set; }
    }

    public class UpdateTicketRequest
    {
        [StringLength(250, MinimumLength = 3)]
        public string? Title { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [Range(1, 4)]
        public int? Priority { get; set; }

        [Range(1, 5)]
        public int? Status { get; set; }

        public int? AssigneeId { get; set; }
        public int? CategoryId { get; set; }
    }

    public class TicketLookupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? Weight { get; set; }
    }

    public class TicketDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Priority { get; set; }
        public string? PriorityName { get; set; }
        public int Status { get; set; }
        public string? StatusName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int RequesterId { get; set; }
        public string? RequesterName { get; set; }
        public int? AssigneeId { get; set; }
        public string? AssigneeName { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
    }
}
