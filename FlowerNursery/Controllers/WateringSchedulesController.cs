using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FlowerNursery.Data;
using FlowerNursery.Models;
using System.Security.Claims;

namespace FlowerNursery.Controllers
{
    [Authorize]
    public class WateringSchedulesController : Controller
    {
        private readonly NurseryDbContext _context;

        public WateringSchedulesController(NurseryDbContext context)
        {
            _context = context;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET: WateringSchedules  (upcoming tasks)
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var upcoming = await _context.WateringSchedules
                .Where(ws => !ws.IsCompleted && ws.FlowerGroup!.Greenhouse!.UserId == userId)
                .OrderBy(ws => ws.ScheduledDate)
                .Include(ws => ws.FlowerGroup)
                    .ThenInclude(fg => fg!.Greenhouse)
                .ToListAsync();

            return View(upcoming);
        }

        // GET: WateringSchedules/History/5
        public async Task<IActionResult> History(int? flowerGroupId)
        {
            if (flowerGroupId == null) return NotFound();

            var userId = GetUserId();
            var flowerGroup = await _context.FlowerGroups
                .Where(fg => fg.Greenhouse!.UserId == userId)
                .Include(fg => fg.Greenhouse)
                .FirstOrDefaultAsync(fg => fg.Id == flowerGroupId);

            if (flowerGroup == null) return NotFound();

            var history = await _context.WateringSchedules
                .Where(ws => ws.FlowerGroupId == flowerGroupId)
                .OrderByDescending(ws => ws.ScheduledDate)
                .ToListAsync();

            ViewBag.FlowerGroup = flowerGroup;
            return View(history);
        }

        // GET: WateringSchedules/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = GetUserId();
            var schedule = await _context.WateringSchedules
                .Where(ws => ws.FlowerGroup!.Greenhouse!.UserId == userId)
                .Include(ws => ws.FlowerGroup)
                    .ThenInclude(fg => fg!.Greenhouse)
                .FirstOrDefaultAsync(ws => ws.Id == id);

            if (schedule == null) return NotFound();

            return View(schedule);
        }

        // GET: WateringSchedules/Create
        public async Task<IActionResult> Create(int? flowerGroupId)
        {
            await PopulateFlowerGroupsDropdown(flowerGroupId);
            var schedule = new WateringSchedule
            {
                ScheduledDate = DateTime.Today.AddDays(1),
                FlowerGroupId = flowerGroupId ?? 0
            };
            return View(schedule);
        }

        // POST: WateringSchedules/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FlowerGroupId,ScheduledDate,Notes")] WateringSchedule schedule)
        {
            var userId = GetUserId();
            var fg = await _context.FlowerGroups
                .Where(f => f.Greenhouse!.UserId == userId)
                .FirstOrDefaultAsync(f => f.Id == schedule.FlowerGroupId);
            if (fg == null)
                ModelState.AddModelError("FlowerGroupId", "Invalid flower group selected.");

            if (ModelState.IsValid)
            {
                schedule.IsCompleted = false;
                _context.Add(schedule);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Watering task was scheduled successfully.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateFlowerGroupsDropdown(schedule.FlowerGroupId);
            return View(schedule);
        }

        // GET: WateringSchedules/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userId = GetUserId();
            var schedule = await _context.WateringSchedules
                .Where(ws => ws.FlowerGroup!.Greenhouse!.UserId == userId)
                .FirstOrDefaultAsync(ws => ws.Id == id);
            if (schedule == null) return NotFound();

            await PopulateFlowerGroupsDropdown(schedule.FlowerGroupId);
            return View(schedule);
        }

        // POST: WateringSchedules/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FlowerGroupId,ScheduledDate,Notes,IsCompleted,CompletedDate")] WateringSchedule schedule)
        {
            if (id != schedule.Id) return NotFound();

            var userId = GetUserId();
            var existing = await _context.WateringSchedules
                .Where(ws => ws.FlowerGroup!.Greenhouse!.UserId == userId)
                .FirstOrDefaultAsync(ws => ws.Id == id);
            if (existing == null) return NotFound();

            var fg = await _context.FlowerGroups
                .Where(f => f.Greenhouse!.UserId == userId)
                .FirstOrDefaultAsync(f => f.Id == schedule.FlowerGroupId);
            if (fg == null)
                ModelState.AddModelError("FlowerGroupId", "Invalid flower group selected.");

            if (ModelState.IsValid)
            {
                try
                {
                    existing.FlowerGroupId = schedule.FlowerGroupId;
                    existing.ScheduledDate = schedule.ScheduledDate;
                    existing.Notes = schedule.Notes;
                    existing.IsCompleted = schedule.IsCompleted;
                    existing.CompletedDate = schedule.CompletedDate;
                    _context.Update(existing);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Watering task updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WateringScheduleExists(schedule.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateFlowerGroupsDropdown(schedule.FlowerGroupId);
            return View(schedule);
        }

        // POST: WateringSchedules/MarkComplete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkComplete(int id, string? returnUrl)
        {
            var userId = GetUserId();
            var schedule = await _context.WateringSchedules
                .Where(ws => ws.FlowerGroup!.Greenhouse!.UserId == userId)
                .FirstOrDefaultAsync(ws => ws.Id == id);
            if (schedule == null) return NotFound();

            schedule.IsCompleted = true;
            schedule.CompletedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Watering task marked as completed.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        // GET: WateringSchedules/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = GetUserId();
            var schedule = await _context.WateringSchedules
                .Where(ws => ws.FlowerGroup!.Greenhouse!.UserId == userId)
                .Include(ws => ws.FlowerGroup)
                    .ThenInclude(fg => fg!.Greenhouse)
                .FirstOrDefaultAsync(ws => ws.Id == id);

            if (schedule == null) return NotFound();

            return View(schedule);
        }

        // POST: WateringSchedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetUserId();
            var schedule = await _context.WateringSchedules
                .Where(ws => ws.FlowerGroup!.Greenhouse!.UserId == userId)
                .FirstOrDefaultAsync(ws => ws.Id == id);
            if (schedule != null)
            {
                _context.WateringSchedules.Remove(schedule);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Watering task was deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateFlowerGroupsDropdown(int? selectedId)
        {
            var userId = GetUserId();
            var groups = await _context.FlowerGroups
                .Where(fg => fg.Greenhouse!.UserId == userId)
                .Include(fg => fg.Greenhouse)
                .OrderBy(fg => fg.Greenhouse!.Name)
                .ThenBy(fg => fg.Species)
                .ToListAsync();

            ViewBag.FlowerGroupId = new SelectList(
                groups.Select(fg => new
                {
                    fg.Id,
                    Display = $"{fg.Species} ({fg.Greenhouse?.Name})"
                }),
                "Id", "Display", selectedId);
        }

        private bool WateringScheduleExists(int id)
        {
            return _context.WateringSchedules.Any(e => e.Id == id);
        }
    }
}
