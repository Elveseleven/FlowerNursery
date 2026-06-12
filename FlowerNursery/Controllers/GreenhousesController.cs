using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlowerNursery.Data;
using FlowerNursery.Models;
using System.Security.Claims;

namespace FlowerNursery.Controllers
{
    [Authorize]
    public class GreenhousesController : Controller
    {
        private readonly NurseryDbContext _context;

        public GreenhousesController(NurseryDbContext context)
        {
            _context = context;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET: Greenhouses
        public async Task<IActionResult> Index()
        {
            var greenhouses = await _context.Greenhouses
                .Where(g => g.UserId == GetUserId())
                .Include(g => g.FlowerGroups)
                .OrderBy(g => g.Name)
                .ToListAsync();
            return View(greenhouses);
        }

        // GET: Greenhouses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var greenhouse = await _context.Greenhouses
                .Where(g => g.UserId == GetUserId())
                .Include(g => g.FlowerGroups)
                    .ThenInclude(fg => fg.WateringSchedules)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (greenhouse == null) return NotFound();

            return View(greenhouse);
        }

        // GET: Greenhouses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Greenhouses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Location,Notes")] Greenhouse greenhouse)
        {
            if (ModelState.IsValid)
            {
                greenhouse.UserId = GetUserId();
                _context.Add(greenhouse);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Greenhouse \"{greenhouse.Name}\" was created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(greenhouse);
        }

        // GET: Greenhouses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var greenhouse = await _context.Greenhouses
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == GetUserId());
            if (greenhouse == null) return NotFound();

            return View(greenhouse);
        }

        // POST: Greenhouses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Location,Notes")] Greenhouse greenhouse)
        {
            if (id != greenhouse.Id) return NotFound();

            // Make sure this greenhouse belongs to the current user
            var existing = await _context.Greenhouses
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == GetUserId());
            if (existing == null) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    existing.Name = greenhouse.Name;
                    existing.Location = greenhouse.Location;
                    existing.Notes = greenhouse.Notes;
                    _context.Update(existing);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Greenhouse \"{greenhouse.Name}\" was updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GreenhouseExists(greenhouse.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(greenhouse);
        }

        // GET: Greenhouses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var greenhouse = await _context.Greenhouses
                .Where(g => g.UserId == GetUserId())
                .Include(g => g.FlowerGroups)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (greenhouse == null) return NotFound();

            return View(greenhouse);
        }

        // POST: Greenhouses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var greenhouse = await _context.Greenhouses
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == GetUserId());
            if (greenhouse != null)
            {
                _context.Greenhouses.Remove(greenhouse);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Greenhouse \"{greenhouse.Name}\" was deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool GreenhouseExists(int id)
        {
            return _context.Greenhouses.Any(e => e.Id == id);
        }
    }
}
