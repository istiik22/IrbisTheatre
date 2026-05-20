using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IrbisTheatre.Models;

namespace IrbisTheatre.Controllers;

public class HomeController : Controller
{
    private readonly TheatreContext _context;

    public HomeController(TheatreContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetPlays()
    {
        var plays = await _context.Plays
            .Include(p => p.Author)
            .Include(p => p.Genre)
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Description,
                Author = p.Author != null ? p.Author.Fio : "Неизвестен",
                Genre = p.Genre != null ? p.Genre.Name : "Не указан"
            })
            .ToListAsync();

        return Ok(plays);
    }

    [HttpGet]
    public async Task<IActionResult> GetHalls()
    {
        var halls = await _context.Halls
            .Select(h => new
            {
                h.Id,
                h.Name,
                // Временно добавим описание залов
                Description = h.Name == "Большая сцена" ? "Вместимость: 500 мест. Идеален для масштабных постановок, опер и балетов" :
                             h.Name == "Малая сцена" ? "Вместимость: 150 мест. Создает камерную атмосферу для драматических спектаклей" :
                             h.Name == "Камерный зал" ? "Вместимость: 80 мест. Для экспериментальных постановок" :
                             "Уютный зал для проведения мероприятий",
                Icon = h.Name.Contains("Большая") ? "🏛️" :
                       h.Name.Contains("Малая") ? "🎭" : "🎪"
            })
            .ToListAsync();

        return Ok(halls);
    }

    [HttpGet]
    public async Task<IActionResult> SearchPlays(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Ok(new List<object>());
        }

        var plays = await _context.Plays
            .Include(p => p.Author)
            .Include(p => p.Genre)
            .Where(p => p.Title.ToLower().Contains(query.ToLower()) ||
                        (p.Author != null && p.Author.Fio.ToLower().Contains(query.ToLower())))
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Description,
                Author = p.Author != null ? p.Author.Fio : "Неизвестен",
                Genre = p.Genre != null ? p.Genre.Name : "Не указан"
            })
            .ToListAsync();

        return Ok(plays);
    }
}