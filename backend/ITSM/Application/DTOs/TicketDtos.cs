namespace Application.DTOs
{
    public class CreateTicketRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Priority { get; set; }
        public int? CategoryId { get; set; }
    }

    public class UpdateTicketRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? Priority { get; set; }
        public int? Status { get; set; }
        public int? AssigneeId { get; set; }
        public int? CategoryId { get; set; }
    }

    public class TicketDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Priority { get; set; }
        public int Status { get; set; }
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
