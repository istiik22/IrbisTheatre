using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IrbisTheatre.Models;

namespace IrbisTheatre.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ApiController : ControllerBase
{
    private readonly TheatreContext _context;

    public ApiController(TheatreContext context)
    {
        _context = context;
    }

    [HttpGet("plays/upcoming")]
    public async Task<IActionResult> GetUpcomingPlays()
    {
        var performances = await _context.Performances
            .Include(p => p.Play)
                .ThenInclude(pl => pl.Author)
            .Include(p => p.Play)
                .ThenInclude(pl => pl.Genre)
            .Where(p => p.Datetime > DateTime.Now && p.Status == "запланирован")
            .OrderBy(p => p.Datetime)
            .Take(6)
            .Select(p => new
            {
                p.Id,
                p.Play.Title,
                p.Play.Description,
                p.Datetime,
                Author = new { p.Play.Author.Fio },
                Genre = new { p.Play.Genre.Name },
                PerformanceId = p.Id
            })
            .ToListAsync();

        return Ok(performances);
    }

    [HttpGet("performances/{id}/seats")]
    public async Task<IActionResult> GetSeats(int id)
    {
        var performance = await _context.Performances.FindAsync(id);
        if (performance == null)
            return NotFound();

        var seats = await _context.Seats
            .Where(s => s.HallId == 1)
            .Take(50)
            .Select(s => new
            {
                s.Id,
                s.RowNumber,
                s.SeatNumber,
                s.Category,
                IsBooked = _context.Tickets.Any(t => t.PerformanceId == id && t.SeatId == s.Id && t.Status != "free")
            })
            .ToListAsync();

        return Ok(new { performance, seats });
    }

    [HttpGet("plays/all")]
    public async Task<IActionResult> GetAllPlays()
    {
        var plays = await _context.Plays
            .Include(p => p.Author)
            .Include(p => p.Genre)
            .OrderBy(p => p.Title)
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Description,
                p.TargetAudience,
                p.PremiereDate,
                Author = new { p.Author.Fio },
                Genre = new { p.Genre.Name }
            })
            .ToListAsync();

        return Ok(plays);
    }

    [HttpGet("plays/{id}")]
    public async Task<IActionResult> GetPlayById(int id)
    {
        var play = await _context.Plays
            .Include(p => p.Author)
            .Include(p => p.Genre)
            .Include(p => p.Roles)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (play == null)
            return NotFound();

        return Ok(new
        {
            play.Id,
            play.Title,
            play.Description,
            play.TargetAudience,
            play.PremiereDate,
            Author = new { play.Author.Fio, play.Author.Country, play.Author.Biography },
            Genre = new { play.Genre.Name },
            Roles = play.Roles.Select(r => new { r.Id, r.Name, r.GenderRequirement, r.AgeRange })
        });
    }
}