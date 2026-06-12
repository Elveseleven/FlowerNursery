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
    public class FlowerGroupsController : Controller
    {
        private readonly NurseryDbContext _context;

        public FlowerGroupsController(NurseryDbContext context)
        {
            _context = context;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET: FlowerGroups
        public async Task<IActionResult> Index(int? greenhouseId)
        {
            var userId = GetUserId();

            var query = _context.FlowerGroups
                .Where(fg => fg.Greenhouse!.UserId == userId)
                .Include(fg => fg.Greenhouse)
                .Include(fg => fg.WateringSchedules)
                .AsQueryable();

            if (greenhouseId.HasValue)
            {
                query = query.Where(fg => fg.GreenhouseId == greenhouseId.Value);
                var gh = await _context.Greenhouses
                    .FirstOrDefaultAsync(g => g.Id == greenhouseId.Value && g.UserId == userId);
                ViewBag.FilterGreenhouse = gh?.Name;
                ViewBag.FilterGreenhouseId = greenhouseId.Value;
            }

            var groups = await query.OrderBy(fg => fg.Species).ToListAsync();
            ViewBag.Greenhouses = await _context.Greenhouses
                .Where(g => g.UserId == userId)
                .OrderBy(g => g.Name)
                .ToListAsync();
            return View(groups);
        }

        // GET: FlowerGroups/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var flowerGroup = await _context.FlowerGroups
                .Where(fg => fg.Greenhouse!.UserId == GetUserId())
                .Include(fg => fg.Greenhouse)
                .Include(fg => fg.WateringSchedules)
                .FirstOrDefaultAsync(fg => fg.Id == id);

            if (flowerGroup == null) return NotFound();

            return View(flowerGroup);
        }

        // GET: FlowerGroups/Create
        public async Task<IActionResult> Create()
        {
            await PopulateGreenhousesDropdown(null);
            return View();
        }

        // POST: FlowerGroups/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Species,Color,Quantity,Notes,GreenhouseId")] FlowerGroup flowerGroup)
        {
            // Ensure the selected greenhouse belongs to the current user
            var userId = GetUserId();
            var greenhouse = await _context.Greenhouses
                .FirstOrDefaultAsync(g => g.Id == flowerGroup.GreenhouseId && g.UserId == userId);

            if (greenhouse == null)
                ModelState.AddModelError("GreenhouseId", "Invalid greenhouse selected.");

            if (ModelState.IsValid)
            {
                _context.Add(flowerGroup);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Flower group \"{flowerGroup.Species}\" was created successfully.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateGreenhousesDropdown(flowerGroup.GreenhouseId);
            return View(flowerGroup);
        }

        // GET: FlowerGroups/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var flowerGroup = await _context.FlowerGroups
                .Where(fg => fg.Greenhouse!.UserId == GetUserId())
                .FirstOrDefaultAsync(fg => fg.Id == id);
            if (flowerGroup == null) return NotFound();

            await PopulateGreenhousesDropdown(flowerGroup.GreenhouseId);
            return View(flowerGroup);
        }

        // POST: FlowerGroups/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Species,Color,Quantity,Notes,GreenhouseId")] FlowerGroup flowerGroup)
        {
            if (id != flowerGroup.Id) return NotFound();

            var userId = GetUserId();

            // Verify ownership
            var existing = await _context.FlowerGroups
                .Where(fg => fg.Greenhouse!.UserId == userId)
                .FirstOrDefaultAsync(fg => fg.Id == id);
            if (existing == null) return NotFound();

            // Verify the target greenhouse belongs to user
            var greenhouse = await _context.Greenhouses
                .FirstOrDefaultAsync(g => g.Id == flowerGroup.GreenhouseId && g.UserId == userId);
            if (greenhouse == null)
                ModelState.AddModelError("GreenhouseId", "Invalid greenhouse selected.");

            if (ModelState.IsValid)
            {
                try
                {
                    existing.Species = flowerGroup.Species;
                    existing.Color = flowerGroup.Color;
                    existing.Quantity = flowerGroup.Quantity;
                    existing.Notes = flowerGroup.Notes;
                    existing.GreenhouseId = flowerGroup.GreenhouseId;
                    _context.Update(existing);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Flower group \"{flowerGroup.Species}\" was updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FlowerGroupExists(flowerGroup.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateGreenhousesDropdown(flowerGroup.GreenhouseId);
            return View(flowerGroup);
        }

        // GET: FlowerGroups/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var flowerGroup = await _context.FlowerGroups
                .Where(fg => fg.Greenhouse!.UserId == GetUserId())
                .Include(fg => fg.Greenhouse)
                .Include(fg => fg.WateringSchedules)
                .FirstOrDefaultAsync(fg => fg.Id == id);

            if (flowerGroup == null) return NotFound();

            return View(flowerGroup);
        }

        // POST: FlowerGroups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var flowerGroup = await _context.FlowerGroups
                .Where(fg => fg.Greenhouse!.UserId == GetUserId())
                .FirstOrDefaultAsync(fg => fg.Id == id);
            if (flowerGroup != null)
            {
                _context.FlowerGroups.Remove(flowerGroup);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Flower group \"{flowerGroup.Species}\" was deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateGreenhousesDropdown(int? selectedId)
        {
            ViewBag.GreenhouseId = new SelectList(
                await _context.Greenhouses
                    .Where(g => g.UserId == GetUserId())
                    .OrderBy(g => g.Name)
                    .ToListAsync(),
                "Id", "Name", selectedId);
        }

        private bool FlowerGroupExists(int id)
        {
            return _context.FlowerGroups.Any(e => e.Id == id);
        }
    }
}
