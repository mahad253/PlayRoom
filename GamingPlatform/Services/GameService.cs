using System;
using System.Collections.Generic;
using GamingPlatform.Data;
using GamingPlatform.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GamingPlatform.Services
{
    public class GameService 
    {
        private readonly GamingPlatformContext _context;

        public GameService(GamingPlatformContext context)
        {
            _context = context;
        }

        
        public List<Game> GetAvailableGames()
        {
            return _context.Game.ToList();
        }
        
        public Game? GetGameById(Guid id)
        {
            return _context.Game.Find(id);
        }

        public Game? GetGameByCode(string code)
        {
            return _context.Game.SingleOrDefault(g => g.Code == code);
        }

        
    }

}
