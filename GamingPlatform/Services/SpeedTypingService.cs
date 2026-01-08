using GamingPlatform.Data;
using GamingPlatform.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class SpeedTypingService
{
    private readonly GamingPlatformContext _context;

    public SpeedTypingService(GamingPlatformContext context)
    {
        _context = context;
    }

    public async Task<List<Sentence>> GetAllSentencesAsync()
    {
        return await _context.Sentences
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Sentence?> GetRandomSentenceAsync(int difficulty, string language = "FR")
    {
        return await _context.Sentences
            .Where(s => s.Difficulty == difficulty && s.Language == language)
            .OrderBy(s => Guid.NewGuid())
            .FirstOrDefaultAsync();
    }
}