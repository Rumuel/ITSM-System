using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketsController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TicketDto>> GetById(int id)
        {
            var ticket = await _ticketService.GetByIdAsync(id);
            if (ticket == null)
                return NotFound();

            return Ok(ticket);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetAll()
        {
            var tickets = await _ticketService.GetAllAsync();
            return Ok(tickets);
        }

        [HttpGet("requester/{requesterId}")]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetByRequester(int requesterId)
        {
            var tickets = await _ticketService.GetByRequesterAsync(requesterId);
            return Ok(tickets);
        }

        [HttpGet("assigned/{assigneeId}")]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetAssignedTo(int assigneeId)
        {
            var tickets = await _ticketService.GetAssignedToAsync(assigneeId);
            return Ok(tickets);
        }

        [HttpPost]
        public async Task<ActionResult<TicketDto>> Create([FromBody] CreateTicketRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
                return Unauthorized();

            var ticket = await _ticketService.CreateAsync(request, userId);
            return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TicketDto>> Update(int id, [FromBody] UpdateTicketRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
                return Unauthorized();

            var ticket = await _ticketService.UpdateAsync(id, request, userId);
            if (ticket == null)
                return NotFound();

            return Ok(ticket);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
                return Unauthorized();

            var result = await _ticketService.DeleteAsync(id, userId);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
