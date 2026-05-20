using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IrbisTheatre.Models;

namespace IrbisTheatre.Controllers;

public class TicketsController : Controller
{
    private readonly TheatreContext _context;

    public TicketsController(TheatreContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> PurchaseByPlay(int? playId)
    {
        // Получаем все спектакли для выпадающего списка
        ViewBag.Plays = await _context.Plays
            .OrderBy(p => p.Title)
            .ToListAsync();

        if (playId.HasValue)
        {
            // Получаем выбранный спектакль
            var selectedPlay = await _context.Plays
                .FirstOrDefaultAsync(p => p.Id == playId);

            if (selectedPlay != null)
            {
                // Получаем доступные показы для этого спектакля
                var performances = await _context.Performances
                    .Include(p => p.Play)
                    .Where(p => p.PlayId == playId && p.Datetime > DateTime.Now && p.Status == "запланирован")
                    .OrderBy(p => p.Datetime)
                    .ToListAsync();

                ViewBag.SelectedPlay = selectedPlay;
                ViewBag.Performances = performances;

                // Если есть показы, автоматически выбираем первый
                if (performances.Any())
                {
                    var firstPerformance = performances.First();
                    return RedirectToAction("PurchaseByPerformance", new { performanceId = firstPerformance.Id });
                }
                else
                {
                    ViewBag.NoPerformances = true;
                }
            }
        }

        return View("Purchase");
    }

    [HttpGet]
    public async Task<IActionResult> PurchaseByPerformance(int? performanceId)
    {
        if (performanceId.HasValue)
        {
            var performance = await _context.Performances
                .Include(p => p.Play)
                .FirstOrDefaultAsync(p => p.Id == performanceId);

            if (performance != null)
            {
                ViewBag.SelectedPerformance = performance;

                var seats = await _context.Seats
                    .Where(s => s.HallId == 1)
                    .Take(50)
                    .ToListAsync();

                var bookedSeats = await _context.Tickets
                    .Where(t => t.PerformanceId == performanceId && t.Status != "free")
                    .Select(t => t.SeatId)
                    .ToListAsync();

                ViewBag.Seats = seats;
                ViewBag.BookedSeats = bookedSeats;
            }
        }

        // Получаем список всех спектаклей для выпадающего списка
        ViewBag.Performances = await _context.Performances
            .Include(p => p.Play)
            .Where(p => p.Datetime > DateTime.Now && p.Status == "запланирован")
            .OrderBy(p => p.Datetime)
            .ToListAsync();

        return View("Purchase");  // 👈 Указываем имя вьюхи
    }

    [HttpGet]
    public async Task<IActionResult> Purchase(int? playId, int? performanceId)
    {
        ViewBag.Plays = await _context.Plays.OrderBy(p => p.Title).ToListAsync();

        if (performanceId.HasValue)
        {
            var performance = await _context.Performances
                .Include(p => p.Play)
                .FirstOrDefaultAsync(p => p.Id == performanceId);

            if (performance != null)
            {
                ViewBag.SelectedPerformance = performance;

                var seats = await _context.Seats.Take(50).ToListAsync();
                var bookedSeats = await _context.Tickets
                    .Where(t => t.PerformanceId == performanceId && t.Status != "free")
                    .Select(t => t.SeatId)
                    .ToListAsync();

                ViewBag.Seats = seats;
                ViewBag.BookedSeats = bookedSeats;
            }
        }
        else if (playId.HasValue)
        {
            var play = await _context.Plays.FindAsync(playId);
            if (play != null)
            {
                ViewBag.SelectedPlay = play;
                ViewBag.Performances = await _context.Performances
                    .Include(p => p.Play)
                    .Where(p => p.PlayId == playId && p.Datetime > DateTime.Now && p.Status == "запланирован")
                    .OrderBy(p => p.Datetime)
                    .ToListAsync();
            }
        }

        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Purchase(int performanceId, string selectedSeats, string customerName, string customerEmail)
    {
        if (string.IsNullOrEmpty(selectedSeats))
        {
            TempData["Error"] = "Выберите хотя бы одно место";
            return RedirectToAction("Purchase", new { performanceId });
        }

        var seatIds = selectedSeats.Split(',').Select(int.Parse).ToList();
        var performance = await _context.Performances
            .Include(p => p.Play)
            .FirstOrDefaultAsync(p => p.Id == performanceId);

        if (performance == null)
        {
            TempData["Error"] = "Спектакль не найден";
            return RedirectToAction("Index", "Home");
        }

        var tickets = new List<Ticket>();
        var uniqueNumber = DateTime.Now.ToString("yyyyMMddHHmmss");
        var random = new Random();

        foreach (var seatId in seatIds)
        {
            var seat = await _context.Seats.FindAsync(seatId);
            var price = performance.BasePrice;

            if (seat?.Category == "Партер-люкс")
                price *= 3;
            else if (seat?.Category == "Партер")
                price *= 2;
            else if (seat?.Category == "Бельэтаж")
                price *= 1.5m;

            tickets.Add(new Ticket
            {
                UniqueNumber = $"{uniqueNumber}{random.Next(1000, 9999)}-{seatId}",
                Price = price,
                Status = "sold",
                PerformanceId = performanceId,
                SeatId = seatId
            });
        }

        _context.Tickets.AddRange(tickets);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Билеты успешно куплены! Количество: {tickets.Count}";
        var ticketNumbers = string.Join(", ", tickets.Select(t => t.UniqueNumber));
        TempData["TicketNumbers"] = ticketNumbers;

        return RedirectToAction("Success");
    }

    [HttpGet]
    public IActionResult Success()
    {
        return View();
    }
}