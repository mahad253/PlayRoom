using Microsoft.AspNetCore.SignalR;

namespace GamingPlatform.Hubs;

public class LobbyHub : Hub
{
    // Appelé quand un joueur rejoint un lobby
    public async Task JoinLobby(string lobbyCode, string nickname)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, lobbyCode);

        await Clients.Group(lobbyCode).SendAsync("PlayerJoined", new
        {
            Nickname = nickname
        });
    }

    // Appelé par le host pour démarrer la partie
    public async Task StartGame(string lobbyCode)
    {
        await Clients.Group(lobbyCode).SendAsync("GameStarted");
    }

    // Optionnel : message de chat
    public async Task SendMessage(string lobbyCode, string nickname, string message)
    {
        await Clients.Group(lobbyCode).SendAsync("ReceiveMessage", new
        {
            Nickname = nickname,
            Message = message
        });
    }
}