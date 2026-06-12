using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlowerNursery.Data;
using FlowerNursery.Models;
using System.Security.Claims;

namespace FlowerNursery.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly NurseryDbContext _context;

        public HomeController(NurseryDbContext context)
        {
            _context = context;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();

            var upcomingWaterings = await _context.WateringSchedules
                .Where(ws => !ws.IsCompleted
                    && ws.ScheduledDate >= DateTime.Today
                    && ws.FlowerGroup!.Greenhouse!.UserId == userId)
                .OrderBy(ws => ws.ScheduledDate)
                .Include(ws => ws.FlowerGroup)
                    .ThenInclude(fg => fg!.Greenhouse)
                .Take(10)
                .ToListAsync();

            var overdueWaterings = await _context.WateringSchedules
                .Where(ws => !ws.IsCompleted
                    && ws.ScheduledDate < DateTime.Today
                    && ws.FlowerGroup!.Greenhouse!.UserId == userId)
                .OrderBy(ws => ws.ScheduledDate)
                .Include(ws => ws.FlowerGroup)
                    .ThenInclude(fg => fg!.Greenhouse)
                .ToListAsync();

            ViewBag.GreenhouseCount = await _context.Greenhouses.CountAsync(g => g.UserId == userId);
            ViewBag.FlowerGroupCount = await _context.FlowerGroups.CountAsync(fg => fg.Greenhouse!.UserId == userId);
            ViewBag.PendingCount = await _context.WateringSchedules.CountAsync(ws =>
                !ws.IsCompleted && ws.FlowerGroup!.Greenhouse!.UserId == userId);
            ViewBag.OverdueWaterings = overdueWaterings;

            return View(upcomingWaterings);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
