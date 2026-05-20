using FCG.Monolith.API.DTOs.Users;
using FCG.Monolith.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Monolith.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository) => _userRepository = userRepository;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var users = await _userRepository.GetAllAsync(ct);
        return Ok(users.Select(u => new UserResponse(u.Id, u.Name, u.Email, u.Role.ToString(), u.CreatedAt)));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user is null) return NotFound();
        return Ok(new UserResponse(user.Id, user.Name, user.Email, user.Role.ToString(), user.CreatedAt));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user is null) return NotFound();
        await _userRepository.DeleteAsync(user, ct);
        await _userRepository.SaveChangesAsync(ct);
        return NoContent();
    }
}
