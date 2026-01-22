using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;
using System.Security.Claims;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TextsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly VigenereCipherService _cipherService;

        public TextsController(ApplicationDbContext context)
        {
            _context = context;
            _cipherService = new VigenereCipherService();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Text>>> GetTexts()
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var texts = await _context.Texts
                .Where(t => t.UserId == userId)
                .ToListAsync();

            return Ok(texts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Text>> GetText(int id)
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var text = await _context.Texts
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (text == null)
                return NotFound("Текст не найден или не принадлежит вам.");

            return Ok(text);
        }

        [HttpPost]
        public async Task<ActionResult<Text>> PostText(CreateTextModel model)
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            if (string.IsNullOrWhiteSpace(model.Title))
                model.Title = "Без названия";

            if (model.EncryptNow)
            {
                if (string.IsNullOrEmpty(model.Content) || !_cipherService.ContainsAtLeastOneLetter(model.Content))
                    return BadRequest("Данную запись невозможно зашифровать.");
            }

            var text = new Text
            {
                Title = model.Title,
                Content = model.Content ?? string.Empty,
                UserId = userId,
                IsEncrypted = false,
                EncryptCon = string.Empty
            };

            if (model.EncryptNow)
            {
                text.EncryptCon = _cipherService.Encrypt(model.Content, model.Key);
                text.IsEncrypted = true;
            }

            _context.Texts.Add(text);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetText), new { id = text.Id }, text);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchText(int id, UpdateTextModel model)
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var text = await _context.Texts
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (text == null)
                return NotFound("Текст не найден или не принадлежит вам.");

            if (!string.IsNullOrEmpty(model.Title))
                text.Title = model.Title;

            if (!string.IsNullOrEmpty(model.Content))
            {
                text.Content = model.Content;
                if (text.IsEncrypted)
                {
                    text.EncryptCon = _cipherService.Encrypt(model.Content);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Texts.Any(e => e.Id == id && e.UserId == userId))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteText(int id)
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var text = await _context.Texts
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (text == null)
                return NotFound("Текст не найден или не принадлежит вам.");

            _context.Texts.Remove(text);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id}/encrypt")]
        public async Task<ActionResult<Text>> EncryptText(int id, [FromBody] KeyRequest? request = null)
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var text = await _context.Texts
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (text == null)
                return NotFound("Текст не найден или не принадлежит вам.");

            if (text.IsEncrypted)
                return BadRequest("Текст уже зашифрован.");

            string key = request?.Key ?? string.Empty;
            if (!_cipherService.ValidateKey(key))
                return BadRequest("Неверный ключ шифрования. Ключ должен содержать только буквы.");

            if (!_cipherService.ContainsAtLeastOneLetter(text.Content))
                return BadRequest("Данную запись невозможно зашифровать.");

            text.EncryptCon = _cipherService.Encrypt(text.Content, key);
            text.IsEncrypted = true;

            await _context.SaveChangesAsync();
            return Ok(text);
        }

        [HttpPost("{id}/decrypt")]
        public async Task<ActionResult<Text>> DecryptText(int id, [FromBody] KeyRequest? request = null)
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var text = await _context.Texts
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (text == null)
                return NotFound("Текст не найден или не принадлежит вам.");

            if (!text.IsEncrypted || string.IsNullOrEmpty(text.EncryptCon))
                return BadRequest("Текст не зашифрован.");

            string key = request?.Key ?? string.Empty;
            if (!_cipherService.ValidateKey(key))
                return BadRequest("Неверный ключ дешифрования. Ключ должен содержать только буквы.");

            text.Content = _cipherService.Decrypt(text.EncryptCon, key);
            text.IsEncrypted = false;
            text.EncryptCon = string.Empty;

            await _context.SaveChangesAsync();
            return Ok(text);
        }

        [HttpPost("encrypt-text")]
        public ActionResult<EncryptDecryptResponse> EncryptText([FromBody] TextKeyRequest request)
        {
            if (string.IsNullOrEmpty(request.Text))
                return BadRequest("Текст обязателен.");

            if (!_cipherService.ValidateKey(request.Key))
                return BadRequest("Неверный ключ шифрования. Ключ должен содержать только буквы.");

            var result = _cipherService.Encrypt(request.Text, request.Key);
            return Ok(new EncryptDecryptResponse { Result = result });
        }

        [HttpPost("decrypt-text")]
        public ActionResult<EncryptDecryptResponse> DecryptText([FromBody] TextKeyRequest request)
        {
            if (string.IsNullOrEmpty(request.Text))
                return BadRequest("Текст обязателен.");

            if (!_cipherService.ValidateKey(request.Key))
                return BadRequest("Неверный ключ дешифрования. Ключ должен содержать только буквы.");

            var result = _cipherService.Decrypt(request.Text, request.Key);
            return Ok(new EncryptDecryptResponse { Result = result });
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }
    }

    public class CreateTextModel
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public bool EncryptNow { get; set; } = false;
        public string? Key { get; set; }
    }

    public class UpdateTextModel
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
    }

    public class KeyRequest
    {
        public string? Key { get; set; }
    }

    public class TextKeyRequest
    {
        public string Text { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
    }

    public class EncryptDecryptResponse
    {
        public string Result { get; set; } = string.Empty;
    }
}