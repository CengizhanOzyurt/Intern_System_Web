using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;              
using ProjectForm.Models;
using System.Diagnostics;

namespace ProjectForm.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : Controller
    {
        private readonly InternDBcontext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, InternDBcontext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Login()
        {
            return View("~/Views/LoginPage/Login.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.email == email && u.password == password);
            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.id);
                return RedirectToAction("Todo");
            }
            ViewBag.ErrorMessage = "Email veya şifre hatalı!";
            return View("~/Views/LoginPage/Login.cshtml");
        }

        public IActionResult Register()
        {
            var model = new UsersModel();
            return View("~/Views/RegisterPage/Register.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(UsersModel model)
        {
            if (ModelState.IsValid)
            {
                _context.Users.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Login");
            }
            return View("~/Views/RegisterPage/Register.cshtml",model);
        }

        public IActionResult Todo()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var tasks = _context.Users_Task
                .Where(t => t.user_id == userId.Value)
                .OrderBy(t => t.start_date)
                .ToList();

            var viewModel = new HomePageViewModel
            {
                TaskModel = new UsersTaskModel(),
                Tasks = tasks
            };

            return View("~/Views/TodoPage/Todo.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Todo(HomePageViewModel viewModel)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var model = viewModel.TaskModel;

            if (model.start_date != null)
                model.start_date = DateTime.SpecifyKind(model.start_date.Value, DateTimeKind.Utc);
            if (model.end_date != null)
                model.end_date = DateTime.SpecifyKind(model.end_date.Value, DateTimeKind.Utc);

            model.user_id = userId.Value;

            if (ModelState.IsValid)
            {
                _context.Users_Task.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Todo");
            }

            var tasks = _context.Users_Task
                .Where(t => t.user_id == userId.Value)
                .OrderBy(t => t.end_date)
                .ToList();

            viewModel.Tasks = tasks;

            return View("~/Views/TodoPage/Todo.cshtml", viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Home/DeleteTask/{id:int}")]
        public JsonResult DeleteTask(int id)
        {
            return Delete(id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null)
                return Json(new { ok = false, error = "unauthorized" });

            var task = _context.Users_Task.FirstOrDefault(t => t.id == id && t.user_id == userId.Value);
            if (task is null)
                return Json(new { ok = false, error = "not found" });

            _context.Users_Task.Remove(task);
            _context.SaveChanges();
            return Json(new { ok = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTask([FromForm] EditTaskDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null)
                return Json(new { ok = false, error = "unauthorized" });

            if (dto == null)
                return Json(new { ok = false, error = "Veri alınamadı." });

            if (string.IsNullOrWhiteSpace(dto.Task))
                return Json(new { ok = false, error = "Task boş olamaz." });

            if (!dto.Start_Date.HasValue || !dto.End_Date.HasValue)
                return Json(new { ok = false, error = "Tarih alanları zorunlu." });

            static DateTime ToUtcDate(DateTime d) => DateTime.SpecifyKind(d.Date, DateTimeKind.Utc);
            var startUtc = ToUtcDate(dto.Start_Date.Value);
            var endUtc = ToUtcDate(dto.End_Date.Value);

            if (endUtc < startUtc)
                return Json(new { ok = false, error = "Bitiş, başlangıçtan önce olamaz." });

            var entity = await _context.Users_Task
                .FirstOrDefaultAsync(t => t.id == dto.Id && t.user_id == userId.Value);

            if (entity == null)
                return Json(new { ok = false, error = "Kayıt bulunamadı." });

            if (dto.Task.Length > 255)
                return Json(new { ok = false, error = "Task en fazla 255 karakter olmalı." });

            entity.task = dto.Task.Trim();
            entity.start_date = startUtc;
            entity.end_date = endUtc;

            try
            {
                await _context.SaveChangesAsync();

                return Json(new
                {
                    ok = true,
                    id = entity.id,
                    task = entity.task,
                    start = entity.start_date?.ToString("yyyy-MM-dd"),
                    end = entity.end_date?.ToString("yyyy-MM-dd")
                });
            }
            catch (DbUpdateException dbEx)
            {
                var baseMsg = dbEx.GetBaseException()?.Message ?? dbEx.Message;

                string userMsg =
                    baseMsg.Contains("23502") ? "DB: Zorunlu alan boş (NOT NULL)." :
                    baseMsg.Contains("23514") ? "DB: Kural ihlali (CHECK). Tarih sırası veya tanımlı bir kural bozuldu." :
                    baseMsg.Contains("23503") ? "DB: Yabancı anahtar (FK) hatası. Kullanıcı/kayıt eşleşmiyor." :
                    baseMsg.Contains("22001") ? "DB: Metin çok uzun. Lütfen Task içeriğini kısaltın." :
                    baseMsg.Contains("22P02") ? "DB: Tip/format uyumsuzluğu. Tarih/sayı formatını kontrol edin." :
                    "DB hata: " + baseMsg;

                _logger.LogError(dbEx, "EditTask DbUpdateException: {Msg}", baseMsg);
                return Json(new { ok = false, error = userMsg });
            }
            catch (Exception ex)
            {
                var baseMsg = ex.GetBaseException()?.Message ?? ex.Message;
                _logger.LogError(ex, "EditTask Exception: {Msg}", baseMsg);
                return Json(new { ok = false, error = "Sunucu hata: " + baseMsg });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> PopulateForm(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null)
                return Json(new { ok = false, error = "unauthorized" });

            var t = await _context.Users_Task
                .Where(x => x.id == id && x.user_id == userId.Value)
                .Select(x => new
                {
                    id = x.id,
                    task = x.task,
                    start = x.start_date,
                    end = x.end_date
                })
                .FirstOrDefaultAsync();

            if (t == null)
                return Json(new { ok = false, error = "not found" });

            return Json(new
            {
                ok = true,
                id = t.id,
                task = t.task,
                start = t.start?.ToString("yyyy-MM-dd"),
                end = t.end?.ToString("yyyy-MM-dd")
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class EditTaskDto
    {
        public int Id { get; set; }
        public string Task { get; set; } = "";
        public DateTime? Start_Date { get; set; }
        public DateTime? End_Date { get; set; }
    }
}