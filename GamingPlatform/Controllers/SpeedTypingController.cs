using GamingPlatform.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GamingPlatform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpeedTypingController : ControllerBase
    {
        private readonly SpeedTypingService _speedTypingService;

        public SpeedTypingController(SpeedTypingService speedTypingService)
        {
            _speedTypingService = speedTypingService;
        }

        // GET: /api/SpeedTyping/sentences
        [HttpGet("sentences")]
        public async Task<IActionResult> GetAllSentences()
        {
            var sentences = await _speedTypingService.GetAllSentencesAsync();
            return Ok(sentences); // JSON
        }

        // GET: /api/SpeedTyping/random?difficulty=1&language=FR
        [HttpGet("random")]
        public async Task<IActionResult> GetRandomSentence(int difficulty = 1, string language = "FR")
        {
            var sentence = await _speedTypingService.GetRandomSentenceAsync(difficulty, language);
            if (sentence == null)
                return NotFound(new { message = "Aucune phrase disponible." });

            return Ok(sentence); // JSON
        }
    }
}