using System.Collections.Concurrent;
using GamingPlatform.Models;
using PlayRoom.Models;

namespace GamingPlatform.Services;

public class LobbyService
{
    private readonly ConcurrentDictionary<string, Lobby> _lobbies = new();

    public Lobby CreateLobby(string gameType = "Morpion", int maxPlayers = 2)
    {
        var lobby = new Lobby
        {
            GameType = gameType,
            MaxPlayers = maxPlayers
        };

        _lobbies[lobby.Id] = lobby;
        return lobby;
    }

    public Lobby? GetLobby(string lobbyId)
    {
        _lobbies.TryGetValue(lobbyId, out var lobby);
        return lobby;
    }

    public (Lobby lobby, LobbyPlayer player) JoinLobby(string lobbyId, string connectionId, string pseudo)
    {
        var lobby = GetLobby(lobbyId) ?? throw new InvalidOperationException("Lobby introuvable.");

        lock (lobby)
        {
            if (lobby.Status != LobbyStatus.Waiting)
                throw new InvalidOperationException("La partie a déjà démarré.");

            if (lobby.Players.Any(p => p.ConnectionId == connectionId))
                return (lobby, lobby.Players.First(p => p.ConnectionId == connectionId));

            if (lobby.Players.Count >= lobby.MaxPlayers)
                throw new InvalidOperationException("Lobby complet.");

            var isHost = lobby.Players.Count == 0;

            var player = new LobbyPlayer
            {
                ConnectionId = connectionId,
                Pseudo = pseudo,
                IsHost = isHost
            };

            lobby.Players.Add(player);

            if (isHost)
                lobby.HostConnectionId = connectionId;

            return (lobby, player);
        }
    }

    public Lobby RemovePlayer(string connectionId)
    {
        // Retire le joueur du lobby où il se trouve
        foreach (var kv in _lobbies)
        {
            var lobby = kv.Value;
            lock (lobby)
            {
                var p = lobby.Players.FirstOrDefault(x => x.ConnectionId == connectionId);
                if (p is null) continue;

                lobby.Players.Remove(p);

                // Si host quitte → on transfère host au 1er joueur restant (si existe)
                if (lobby.HostConnectionId == connectionId)
                {
                    lobby.HostConnectionId = lobby.Players.FirstOrDefault()?.ConnectionId;
                    foreach (var pl in lobby.Players) pl.IsHost = (pl.ConnectionId == lobby.HostConnectionId);
                }

                // Si plus personne → on supprime le lobby
                if (lobby.Players.Count == 0)
                    _lobbies.TryRemove(lobby.Id, out _);

                return lobby;
            }
        }

        throw new InvalidOperationException("Joueur non trouvé dans un lobby.");
    }

    public Lobby StartGame(string lobbyId, string callerConnectionId)
    {
        var lobby = GetLobby(lobbyId) ?? throw new InvalidOperationException("Lobby introuvable.");

        lock (lobby)
        {
            if (lobby.HostConnectionId != callerConnectionId)
                throw new InvalidOperationException("Seul le host peut lancer la partie.");

            if (lobby.Status != LobbyStatus.Waiting)
                throw new InvalidOperationException("Partie déjà démarrée.");

            if (lobby.Players.Count != lobby.MaxPlayers)
                throw new InvalidOperationException("Il manque des joueurs.");

            // Assigner X/O
            lobby.Players[0].Symbol = "X";
            lobby.Players[1].Symbol = "O";

            lobby.Game = new Morpion();
            lobby.Status = LobbyStatus.Started;

            return lobby;
        }
    }

    public Lobby PlayMove(string lobbyId, string callerConnectionId, int index)
    {
        var lobby = GetLobby(lobbyId) ?? throw new InvalidOperationException("Lobby introuvable.");

        lock (lobby)
        {
            if (lobby.Status != LobbyStatus.Started)
                throw new InvalidOperationException("La partie n'a pas commencé.");

            var player = lobby.Players.FirstOrDefault(p => p.ConnectionId == callerConnectionId)
                         ?? throw new InvalidOperationException("Joueur introuvable dans ce lobby.");

            // Tour du joueur : Morpion.CurrentPlayer vaut "X" ou "O"
            if (player.Symbol != lobby.Game.CurrentPlayer)
                throw new InvalidOperationException("Ce n'est pas ton tour.");

            lobby.Game.Play(index);
            if (lobby.Game.Finished)
                lobby.Status = LobbyStatus.Finished;

            return lobby;
        }
    }

    public Lobby ResetGame(string lobbyId, string callerConnectionId)
    {
        var lobby = GetLobby(lobbyId) ?? throw new InvalidOperationException("Lobby introuvable.");

        lock (lobby)
        {
            if (lobby.HostConnectionId != callerConnectionId)
                throw new InvalidOperationException("Seul le host peut relancer.");

            lobby.Game = new Morpion();
            lobby.Status = LobbyStatus.Started;
            return lobby;
        }
    }
    
    public Lobby StartSpeedTyping(string lobbyId, string callerConnectionId, string textToType, int durationSeconds)
    {
        var lobby = GetLobby(lobbyId) ?? throw new InvalidOperationException("Lobby introuvable.");
        lock (lobby)
        {
            if (lobby.HostConnectionId != callerConnectionId)
                throw new InvalidOperationException("Seul le host peut lancer la partie.");

            if (lobby.Status != LobbyStatus.Waiting)
                throw new InvalidOperationException("Partie déjà démarrée.");

            if (lobby.Players.Count < 2)
                throw new InvalidOperationException("Il faut au moins 2 joueurs.");

            lobby.SpeedTypingGame ??= new SpeedTypingGame();
            lobby.SpeedTypingGame.Init(
                textToType,
                durationSeconds,
                lobby.Players.Select(p => (p.ConnectionId, p.Pseudo))
            );
            lobby.SpeedTypingGame.Start();

            lobby.Status = LobbyStatus.Started;
            return lobby;
        }
    }
    
    public Lobby UpdateSpeedTypingProgress(string lobbyId, string callerConnectionId, int totalTyped, int correctChars)
    {
        var lobby = GetLobby(lobbyId) ?? throw new InvalidOperationException("Lobby introuvable.");
        lock (lobby)
        {
            if (lobby.GameType != "SpeedTyping")
                throw new InvalidOperationException("Mauvais type de jeu.");
            if (lobby.Status != LobbyStatus.Started)
                throw new InvalidOperationException("La partie n'a pas commencé.");

            lobby.SpeedTypingGame ??= new SpeedTypingGame();
            lobby.SpeedTypingGame.UpdateProgress(callerConnectionId, totalTyped, correctChars);

            if (lobby.SpeedTypingGame.Finished)
                lobby.Status = LobbyStatus.Finished;

            return lobby;
        }
    }
}
