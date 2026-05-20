using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IrbisTheatre.Models;

namespace IrbisTheatre.Controllers;

public class PlaysController : Controller
{
    private readonly TheatreContext _context;

    public PlaysController(TheatreContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var play = await _context.Plays
            .Include(p => p.Author)
            .Include(p => p.Genre)
            .Include(p => p.Roles)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (play == null)
            return NotFound();

        // Получаем ближайшие показы
        var upcomingPerformances = await _context.Performances
            .Include(p => p.Play)
            .Where(p => p.PlayId == id && p.Datetime > DateTime.Now && p.Status == "запланирован")
            .OrderBy(p => p.Datetime)
            .Take(5)
            .ToListAsync();

        ViewBag.UpcomingPerformances = upcomingPerformances;
        return View(play);
    }
}