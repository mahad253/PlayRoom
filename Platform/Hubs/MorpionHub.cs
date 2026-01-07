using Microsoft.AspNetCore.SignalR;
using GamingPlatform.Models;

namespace GamingPlatform.Hubs
{
    public class MorpionHub : Hub
    {
        private static Morpion Game = new Morpion();

        public async Task PlayMove(int index)
        {
            Game.Play(index);
            await Clients.All.SendAsync("UpdateGame", Game);
        }

        public async Task ResetGame()
        {
            Game = new Morpion();
            await Clients.All.SendAsync("UpdateGame", Game);
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("UpdateGame", Game);
            await base.OnConnectedAsync();
        }
    }
}
