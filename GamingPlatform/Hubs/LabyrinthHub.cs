
using GamingPlatform.Data;
using GamingPlatform.Models;
using GamingPlatform.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GamingPlatform.Hubs
{

    public class LabyrinthHub : Hub
    {
        private readonly LabyrinthService _labyrinthService;
        public LabyrinthHub(LabyrinthService labyrinthService)
        {
            _labyrinthService = labyrinthService;
        }
        public async Task SaveLabyrinth(Guid lobbyId, string player1, string player2,  string labyrinthJson)
        {
            try
            {
                // Créer et sauvegarder le labyrinthe
                var labyrinth = await _labyrinthService.CreateLabyrinthAsync(lobbyId, player1, player2, labyrinthJson);

                // Envoi d'un message à tous les clients
                await Clients.All.SendAsync("LabyrinthSaved", "Le labyrinthe a été sauvegardé avec succès !");
            }
            catch (Exception ex)
            {
                // En cas d'erreur, envoyer un message d'erreur à tous les clients
                await Clients.All.SendAsync("LabyrinthSaved", $"Erreur lors de la sauvegarde du labyrinthe : {ex.Message}");
            }
        }

        public async Task SendPlayerMovement(int playerId, int newCell)
        {
            await Clients.Others.SendAsync("ReceivePlayerMovement", playerId, newCell);
        }

        public async Task SendGameEnd(string message)
        {
            await Clients.All.SendAsync("EndGame", message);
        }

    }

}
