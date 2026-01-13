namespace GamingPlatform.Services;

using GamingPlatform.Data;
using GamingPlatform.Models;
using Microsoft.EntityFrameworkCore;

public class LobbyService
{
    private readonly GamingPlatformContext _context;

    public LobbyService(GamingPlatformContext context)
    {
        _context = context;
    }

    public async Task<Lobby> CreateLobbyAsync(string hostName, bool isPrivate, string? password)
    {
        var code = Guid.NewGuid().ToString("N")[..6].ToUpper(); // code court

        var lobby = new Lobby
        {
            Code = code,
            HostName = hostName,
            IsPrivate = isPrivate,
            Password = isPrivate ? password : null
        };

        lobby.Players.Add(new LobbyPlayer
        {
            Nickname = hostName,
            IsHost = true
        });

        _context.Lobbies.Add(lobby);
        await _context.SaveChangesAsync();

        return lobby;
    }

    public async Task<List<Lobby>> GetWaitingLobbiesAsync()
    {
        return await _context.Lobbies
            .Include(l => l.Players)
            .Where(l => l.Status == "Waiting")
            .ToListAsync();
    }

    public async Task<Lobby?> JoinLobbyAsync(string code, string nickname, string? password)
    {
        var lobby = await _context.Lobbies
            .Include(l => l.Players)
            .FirstOrDefaultAsync(l => l.Code == code);

        if (lobby == null) return null;

        if (lobby.IsPrivate && lobby.Password != password)
            return null;

        if (lobby.Status != "Waiting")
            return null;

        lobby.Players.Add(new LobbyPlayer
        {
            Nickname = nickname,
            IsHost = false
        });

        await _context.SaveChangesAsync();
        return lobby;
    }
}
