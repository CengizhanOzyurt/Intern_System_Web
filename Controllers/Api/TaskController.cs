using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectForm.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectForm.Controllers.Api
{
    // ======== DTO'lar: Swagger şeması için üst seviye (nested değil) ========

    public class AddTaskDto
    {
        public int User_Id { get; set; }                 // DB'de user_id zorunluysa gerekli
        public string Task { get; set; } = "";
        public DateTime? Start_Date { get; set; }
        public DateTime? End_Date   { get; set; }
    }

    public class EditTaskDto
    {
        public string Task { get; set; } = "";
        public DateTime? Start_Date { get; set; }
        public DateTime? End_Date   { get; set; }
    }


    [ApiController]
    [Route("api/[controller]")] 
    [IgnoreAntiforgeryToken] 
    public class TaskController : ControllerBase
    {
        private readonly InternDBcontext _context;
        private readonly ILogger<TaskController> _logger;

        public TaskController(InternDBcontext context, ILogger<TaskController> logger)
        {
            _context = context;
            _logger  = logger;
        }


        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UsersTaskModel), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Add([FromBody] AddTaskDto dto)
        {
            if (dto == null)
                return BadRequest(new { ok = false, error = "Veri alınamadı." });

            if (string.IsNullOrWhiteSpace(dto.Task))
                return BadRequest(new { ok = false, error = "Task boş olamaz." });

            // tarih doğrulama ve UTC normalize
            static DateTime? AsUtc(DateTime? d) =>
                d.HasValue ? DateTime.SpecifyKind(d.Value.Date, DateTimeKind.Utc) : null;

            var startUtc = AsUtc(dto.Start_Date);
            var endUtc   = AsUtc(dto.End_Date);

            if (startUtc.HasValue && endUtc.HasValue && endUtc < startUtc)
                return BadRequest(new { ok = false, error = "Bitiş, başlangıçtan önce olamaz." });

            // DB entity oluştur
            var entity = new UsersTaskModel
            {
                task       = dto.Task.Trim(),
                start_date = startUtc,
                end_date   = endUtc,
                user_id    = dto.User_Id
            };

            try
            {
                _context.Users_Task.Add(entity);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    ok   = true,
                    id   = entity.id,
                    task = entity.task,
                    start = entity.start_date?.ToString("yyyy-MM-dd"),
                    end   = entity.end_date?.ToString("yyyy-MM-dd"),
                    user  = entity.user_id
                });
            }
            catch (DbUpdateException dbEx)
            {
                var baseMsg = dbEx.GetBaseException()?.Message ?? dbEx.Message;
                string userMsg =
                      baseMsg.Contains("23502") ? "DB: Zorunlu alan boş (NOT NULL)."
                    : baseMsg.Contains("23514") ? "DB: Kural ihlali (CHECK)."
                    : baseMsg.Contains("23503") ? "DB: Yabancı anahtar (FK) hatası."
                    : baseMsg.Contains("22001") ? "DB: Metin çok uzun."
                    : baseMsg.Contains("22P02") ? "DB: Tip/format uyumsuzluğu."
                    : "DB hata: " + baseMsg;

                _logger.LogError(dbEx, "AddTask DbUpdateException: {Msg}", baseMsg);
                return StatusCode(500, new { ok = false, error = userMsg });
            }
            catch (Exception ex)
            {
                var baseMsg = ex.GetBaseException()?.Message ?? ex.Message;
                _logger.LogError(ex, "AddTask Exception: {Msg}", baseMsg);
                return StatusCode(500, new { ok = false, error = "Sunucu hata: " + baseMsg });
            }
        }

        // ---- READ: GET /api/Task/{id} ----
        [HttpGet("{id:int}")]
        [AllowAnonymous] 
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetTask(int id)
        {
            var t = await _context.Users_Task
                .Where(x => x.id == id)
                .Select(x => new
                {
                    id   = x.id,
                    task = x.task,
                    start = x.start_date,
                    end   = x.end_date
                })
                .FirstOrDefaultAsync();

            if (t == null)
                return NotFound(new { ok = false, error = "not found" });

            return Ok(new
            {
                ok   = true,
                id   = t.id,
                task = t.task,
                start = t.start?.ToString("yyyy-MM-dd"),
                end   = t.end?.ToString("yyyy-MM-dd")
            });
        }

        // ---- UPDATE: PUT /api/Task/{id} ----
        [HttpPut("{id:int}")]
        [AllowAnonymous] 
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> EditTask(int id, [FromBody] EditTaskDto dto)
        {
            if (dto == null)
                return BadRequest(new { ok = false, error = "Veri alınamadı." });

            if (string.IsNullOrWhiteSpace(dto.Task))
                return BadRequest(new { ok = false, error = "Task boş olamaz." });

            if (!dto.Start_Date.HasValue || !dto.End_Date.HasValue)
                return BadRequest(new { ok = false, error = "Tarih alanları zorunlu." });

            static DateTime ToUtcDate(DateTime d) => DateTime.SpecifyKind(d.Date, DateTimeKind.Utc);
            var startUtc = ToUtcDate(dto.Start_Date.Value);
            var endUtc   = ToUtcDate(dto.End_Date.Value);

            if (endUtc < startUtc)
                return BadRequest(new { ok = false, error = "Bitiş, başlangıçtan önce olamaz." });

            var entity = await _context.Users_Task.FirstOrDefaultAsync(t => t.id == id);
            if (entity == null)
                return NotFound(new { ok = false, error = "Kayıt bulunamadı." });

            if (dto.Task.Length > 255)
                return BadRequest(new { ok = false, error = "Task en fazla 255 karakter olmalı." });

            entity.task       = dto.Task.Trim();
            entity.start_date = startUtc;
            entity.end_date   = endUtc;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    ok   = true,
                    id   = entity.id,
                    task = entity.task,
                    start = entity.start_date?.ToString("yyyy-MM-dd"),
                    end   = entity.end_date?.ToString("yyyy-MM-dd")
                });
            }
            catch (DbUpdateException dbEx)
            {
                var baseMsg = dbEx.GetBaseException()?.Message ?? dbEx.Message;
                string userMsg =
                      baseMsg.Contains("23502") ? "DB: Zorunlu alan boş (NOT NULL)."
                    : baseMsg.Contains("23514") ? "DB: Kural ihlali (CHECK)."
                    : baseMsg.Contains("23503") ? "DB: Yabancı anahtar (FK) hatası."
                    : baseMsg.Contains("22001") ? "DB: Metin çok uzun."
                    : baseMsg.Contains("22P02") ? "DB: Tip/format uyumsuzluğu."
                    : "DB hata: " + baseMsg;

                _logger.LogError(dbEx, "EditTask DbUpdateException: {Msg}", baseMsg);
                return StatusCode(500, new { ok = false, error = userMsg });
            }
            catch (Exception ex)
            {
                var baseMsg = ex.GetBaseException()?.Message ?? ex.Message;
                _logger.LogError(ex, "EditTask Exception: {Msg}", baseMsg);
                return StatusCode(500, new { ok = false, error = "Sunucu hata: " + baseMsg });
            }
        }

        [HttpDelete("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult Delete(int id)
        {
            var task = _context.Users_Task.FirstOrDefault(t => t.id == id);
            if (task is null)
                return NotFound(new { ok = false, error = "not found" });

            _context.Users_Task.Remove(task);
            _context.SaveChanges();
            return NoContent();
        }
    }
}