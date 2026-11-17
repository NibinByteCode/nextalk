using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexTalk.Application.DTOs;
using NexTalk.Application.Services;

namespace NexTalk.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/chat")]
public sealed class ChatController(ChatService chat) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet("conversations")]
    public async Task<IActionResult> MyConversations(CancellationToken ct) => Ok(await chat.GetMyConversationsAsync(UserId, ct));

    [HttpGet("users/search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string query, CancellationToken ct)
    {
        try { return Ok(await chat.SearchUsersAsync(query, UserId, ct)); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("conversations")]
    public async Task<IActionResult> CreateConversation(CreateConversationRequest request, CancellationToken ct)
    {
        try { return Ok(await chat.CreateConversationAsync(UserId, request, ct)); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("rooms")]
    public async Task<IActionResult> PublicRooms(CancellationToken ct) => Ok(await chat.GetPublicRoomsAsync(ct));

    [HttpPost("rooms")]
    public async Task<IActionResult> CreateRoom(CreateRoomRequest request, CancellationToken ct)
    {
        try { return Ok(await chat.CreatePublicRoomAsync(UserId, request.Name, ct)); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("conversations/{id}/messages")]
    public async Task<IActionResult> Messages(string id, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        try { return Ok(await chat.GetMessagesAsync(UserId, id, take, ct)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPost("groups/{id}/members/{userId}")]
    public async Task<IActionResult> AddGroupMember(string id, string userId, CancellationToken ct)
    {
        try { await chat.AddGroupMemberAsync(UserId, id, userId, ct); return NoContent(); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
    }
}
