using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using ProjectForm.Models;
using ProjectForm.Views.Home_Page;
using System.Diagnostics;

namespace ProjectForm.Controllers
{
    public class HomeController : Controller
    {
        private readonly InternDBcontext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, InternDBcontext context)
        {
            _logger = logger;
            _context = context;
        }
        
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Index(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.email == email && u.password == password);
            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.id);
                return RedirectToAction("HomePage");
            }
            ViewBag.ErrorMessage = "Email veya şifre hatalı!";
            return View();
        }   
        
        public IActionResult Register()
        {
            var model = new UsersModel();
            return View("~/Views/Register/Register.cshtml", model);
        }
        
        [HttpPost]
        public IActionResult Register(UsersModel model)
        {
            if (ModelState.IsValid)
            {
                _context.Users.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(model);
        }
        public IActionResult HomePage()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index");
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

            return View("~/Views/HomePage/HomePage.cshtml", viewModel);
        }
        [HttpPost]
        public IActionResult HomePage(HomePageViewModel viewModel)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index");
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
                return RedirectToAction("HomePage");
            }

            var tasks = _context.Users_Task
                .Where(t => t.user_id == userId.Value)
                .OrderBy(t => t.end_date)
                .ToList();

            viewModel.Tasks = tasks;

            return View("~/Views/HomePage/HomePage.cshtml", viewModel);
        }
        [HttpPost]
        public JsonResult Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null)
                return Json(new { ok = false, error = "unauthorized" });

            var task = _context.Users_Task.FirstOrDefault(t => t.id == id && t.user_id == userId.Value);

            if (task is null)
                return Json(new { ok = false, Error = "not found" });
            _context.Users_Task.Remove(task);
            _context.SaveChanges();
            return Json(new{ ok = true});
        }
        [HttpPost]
        public JsonResult Edit(int id, string task)
        {
            var user_Id = HttpContext.Session.GetInt32("UserId");
            if (user_Id is null)
                return Json(new { ok = false, Error = "unauthorized" });

            var item = _context.Users_Task.FirstOrDefault(t => t.id == id && t.user_id == user_Id.Value);
            if (item is null)
                return Json(new { ok = false, error = "not found" });

            if (string.IsNullOrWhiteSpace(task))
                return Json(new { ok = false, error = "empty task" });
    
            item.task = task.Trim();
            _context.SaveChanges();
            return Json(new { ok = true });

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
}
