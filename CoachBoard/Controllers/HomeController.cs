using System.Diagnostics;
using CoachBoard.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoachBoard.Data;

namespace CoachBoard.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _db;

        public HomeController(ILogger<HomeController> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var naechste = await _db.Trainingseinheiten
                .Where(t => t.Beginn >= DateTime.Now)
                .OrderBy(t => t.Beginn)
                .FirstOrDefaultAsync();

            return View(naechste);   // kann null sein
        }
    }
}
