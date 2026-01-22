using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HistoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        //History
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequestHistory>>> GetHistory()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var history = await _context.RequestHistories
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();

            return Ok(history);
        }

        //History
        [HttpDelete]
        public async Task<IActionResult> ClearHistory()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var userHistory = await _context.RequestHistories
                .Where(h => h.UserId == userId)
                .ToListAsync();

            _context.RequestHistories.RemoveRange(userHistory);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "История запросов очищена" });
        }

        //History/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHistoryItem(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var historyItem = await _context.RequestHistories
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

            if (historyItem == null)
            {
                return NotFound();
            }

            _context.RequestHistories.Remove(historyItem);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Запись истории удалена" });
        }
    }
}