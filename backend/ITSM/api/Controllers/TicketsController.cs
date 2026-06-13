using Application.DTOs;
using Application.Interfaces;
using Domain.Constants;
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
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            var tickets = CanManageAnyTicket()
                ? await _ticketService.GetAllAsync()
                : await _ticketService.GetByRequesterAsync(userId);

            return Ok(tickets);
        }

        [HttpGet("mine")]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetMine()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            var tickets = await _ticketService.GetByRequesterAsync(userId);
            return Ok(tickets);
        }

        [HttpGet("priorities")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<TicketLookupDto>>> GetPriorities()
        {
            var priorities = await _ticketService.GetPrioritiesAsync();
            return Ok(priorities);
        }

        [HttpGet("statuses")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<TicketLookupDto>>> GetStatuses()
        {
            var statuses = await _ticketService.GetStatusesAsync();
            return Ok(statuses);
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

            var userId = GetCurrentUserId();
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

            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            var ticket = await _ticketService.UpdateAsync(id, request, userId, CanManageAnyTicket());
            if (ticket == null)
                return NotFound();

            return Ok(ticket);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            var result = await _ticketService.DeleteAsync(id, userId, CanManageAnyTicket());
            if (!result)
                return NotFound();

            return NoContent();
        }

        private int GetCurrentUserId()
        {
            return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : 0;
        }

        private bool CanManageAnyTicket()
        {
            return User.IsInRole(AppRoles.Administrator) || User.IsInRole(AppRoles.Technician);
        }
    }
}
