using System.Collections.Concurrent;
using GamingPlatform.Models.Connect4;

namespace GamingPlatform.Services;

public interface IGameStore
{
    Connect4State GetOrCreate(string lobbyId);
    bool TryGet(string lobbyId, out Connect4State state);
    void Remove(string lobbyId);
}

public class InMemoryGameStore : IGameStore
{
    private readonly ConcurrentDictionary<string, Connect4State> _games = new();

    public Connect4State GetOrCreate(string lobbyId)
        => _games.GetOrAdd(lobbyId, id => new Connect4State(id));

    public bool TryGet(string lobbyId, out Connect4State state)
        => _games.TryGetValue(lobbyId, out state!);

    public void Remove(string lobbyId) => _games.TryRemove(lobbyId, out _);
}
