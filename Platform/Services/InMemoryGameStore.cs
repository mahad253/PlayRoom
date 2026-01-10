using System.Collections.Concurrent;
using GamingPlatform.Models.Connect4;
using System.Linq;

namespace GamingPlatform.Services;

public interface IGameStore
{
    Connect4State GetOrCreate(string lobbyId);
    Connect4LobbyInfo GetLobbyInfo(string lobbyId);
    bool TryGet(string lobbyId, out Connect4State state);
    void Remove(string lobbyId);
    bool TryRemovePlayer(string connectionId, out string lobbyId, out Connect4LobbyInfo lobby);

}

public class InMemoryGameStore : IGameStore
{
    private readonly ConcurrentDictionary<string, Connect4State> _games = new();
    private readonly ConcurrentDictionary<string, Connect4LobbyInfo> _lobbies = new();

    public Connect4State GetOrCreate(string lobbyId)
        => _games.GetOrAdd(lobbyId, id => new Connect4State(id));

    public Connect4LobbyInfo GetLobbyInfo(string lobbyId)
        => _lobbies.GetOrAdd(lobbyId, id => new Connect4LobbyInfo { LobbyId = id });

    public bool TryGet(string lobbyId, out Connect4State state)
        => _games.TryGetValue(lobbyId, out state!);

    public void Remove(string lobbyId)
    {
        _games.TryRemove(lobbyId, out _);
        _lobbies.TryRemove(lobbyId, out _);
    }
    public bool TryRemovePlayer(string connectionId, out string lobbyId, out Connect4LobbyInfo lobby)
    {
        lobbyId = "";
        lobby = null!;

        foreach (var kv in _lobbies)
        {
            var info = kv.Value;

            lock (info)
            {
                var player = info.Players.FirstOrDefault(p => p.ConnectionId == connectionId);
                if (player == null) continue;

                info.Players.Remove(player);

                // Transfert host si besoin
                if (info.HostConnectionId == connectionId)
                {
                    info.HostConnectionId = info.Players.FirstOrDefault()?.ConnectionId;

                    // Mets à jour IsHost sur tous les joueurs (vu que tu as ce champ)
                    foreach (var p in info.Players)
                        p.IsHost = (p.ConnectionId == info.HostConnectionId);
                }

                lobbyId = info.LobbyId;
                lobby = info;

                // Si plus personne dans le lobby => nettoyage complet
                if (info.Players.Count == 0)
                    Remove(info.LobbyId);

                return true;
            }
        }

        return false;
    }

}


