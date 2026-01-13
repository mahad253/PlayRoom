using GamingPlatform.Services;
using Microsoft.AspNetCore.Mvc;

namespace GamingPlatform.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LobbyController : ControllerBase
{
    private readonly LobbyService _lobbyService;

    public LobbyController(LobbyService lobbyService)
    {
        _lobbyService = lobbyService;
    }

    // POST api/lobby
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLobbyRequest request)
    {
        var lobby = await _lobbyService.CreateLobbyAsync(
            request.HostName,
            request.IsPrivate,
            request.Password
        );

        // tu pourras construire ici l’URL publique à renvoyer au front
        return Ok(lobby);
    }

    // GET api/lobby
    [HttpGet]
    public async Task<IActionResult> GetWaiting()
    {
        var lobbies = await _lobbyService.GetWaitingLobbiesAsync();
        return Ok(lobbies);
    }

    // POST api/lobby/join
    [HttpPost("join")]
    public async Task<IActionResult> Join([FromBody] JoinLobbyRequest request)
    {
        var lobby = await _lobbyService.JoinLobbyAsync(
            request.Code,
            request.Nickname,
            request.Password
        );

        if (lobby == null)
            return BadRequest(new { message = "Impossible de rejoindre le lobby." });

        return Ok(lobby);
    }
}

public class CreateLobbyRequest
{
    public string HostName { get; set; } = null!;
    public bool IsPrivate { get; set; }
    public string? Password { get; set; }
}

public class JoinLobbyRequest
{
    public string Code { get; set; } = null!;
    public string Nickname { get; set; } = null!;
    public string? Password { get; set; }
}