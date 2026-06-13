using Application.DTOs;
using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [ApiController]
    [Authorize(Roles = AppRoles.Administrator)]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("roles")]
        public ActionResult<IReadOnlyCollection<string>> GetRoles()
        {
            return Ok(AppRoles.All);
        }

        [HttpGet("users")]
        public async Task<ActionResult<IReadOnlyCollection<UserDto>>> GetUsers()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.Name)
                .ThenBy(u => u.UserName)
                .ToListAsync();

            var result = new List<UserDto>();
            foreach (var user in users)
            {
                result.Add(await MapToDtoAsync(user));
            }

            return Ok(result);
        }

        [HttpPut("users/{userId:int}/roles")]
        public async Task<ActionResult<UserDto>> UpdateUserRoles(int userId, [FromBody] UpdateUserRolesRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var requestedRoles = request.Roles
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Select(role => role.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var invalidRoles = requestedRoles
                .Where(role => !AppRoles.All.Contains(role, StringComparer.OrdinalIgnoreCase))
                .ToArray();

            if (invalidRoles.Length > 0)
            {
                return BadRequest(new { message = $"Roles invalidas: {string.Join(", ", invalidRoles)}" });
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return NotFound();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removingAdministrator = currentRoles.Contains(AppRoles.Administrator)
                && !requestedRoles.Contains(AppRoles.Administrator, StringComparer.OrdinalIgnoreCase);

            if (removingAdministrator && await IsOnlyAdministratorAsync(user.Id))
            {
                return BadRequest(new { message = "Nao e possivel remover a role Administrador do ultimo administrador." });
            }

            var rolesToRemove = currentRoles
                .Where(role => !requestedRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
                .ToArray();

            var rolesToAdd = requestedRoles
                .Where(role => !currentRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
                .ToArray();

            if (rolesToRemove.Length > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    return BadRequest(new { message = string.Join(", ", removeResult.Errors.Select(e => e.Description)) });
                }
            }

            if (rolesToAdd.Length > 0)
            {
                var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    return BadRequest(new { message = string.Join(", ", addResult.Errors.Select(e => e.Description)) });
                }
            }

            return Ok(await MapToDtoAsync(user));
        }

        private async Task<bool> IsOnlyAdministratorAsync(int userId)
        {
            var administrators = await _userManager.GetUsersInRoleAsync(AppRoles.Administrator);
            return administrators.Count == 1 && administrators[0].Id == userId;
        }

        private async Task<UserDto> MapToDtoAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = user.Name,
                IsActive = user.IsActive,
                Roles = roles.ToArray()
            };
        }
    }
}
