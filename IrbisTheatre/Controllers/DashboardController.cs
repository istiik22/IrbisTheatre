using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IrbisTheatre.Models;
using System.Security.Claims;

namespace IrbisTheatre.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly TheatreContext _context;

    public DashboardController(TheatreContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Staff";
        return RedirectToAction(role);
    }

    // ========== ГЛАВНЫЕ МЕТОДЫ ДЛЯ РАЗНЫХ РОЛЕЙ ==========
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Admin()
    {
        ViewBag.EmployeesCount = await _context.Employers.CountAsync();
        ViewBag.PlaysCount = await _context.Plays.CountAsync();
        ViewBag.PerformancesCount = await _context.Performances.CountAsync();
        ViewBag.TicketsSold = await _context.Tickets.CountAsync(t => t.Status == "sold");
        return View();
    }

    [Authorize(Roles = "Director")]
    public async Task<IActionResult> Director()
    {
        ViewBag.Plays = await _context.Plays.Include(p => p.Author).Include(p => p.Genre).ToListAsync();
        ViewBag.UpcomingPerformances = await _context.Performances
            .Include(p => p.Play)
            .Where(p => p.Datetime > DateTime.Now && p.Status == "запланирован")
            .OrderBy(p => p.Datetime)
            .Take(5)
            .ToListAsync();
        return View();
    }

    [Authorize(Roles = "Cashier")]
    public async Task<IActionResult> Cashier()
    {
        ViewBag.Performances = await _context.Performances
            .Include(p => p.Play)
            .Where(p => p.Datetime > DateTime.Now && p.Status == "запланирован")
            .OrderBy(p => p.Datetime)
            .ToListAsync();
        return View();
    }

    [Authorize(Roles = "Actor")]
    public async Task<IActionResult> Actor()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var myRoles = await _context.ProductionTeams
            .Include(pt => pt.Role).ThenInclude(r => r.Play)
            .Where(pt => pt.EmployerId == userId)
            .ToListAsync();

        var mySchedule = await _context.PerformanceGroups
            .Include(pg => pg.Performance).ThenInclude(p => p.Play)
            .Where(pg => pg.ProductionTeam.EmployerId == userId)
            .Select(pg => pg.Performance)
            .Where(p => p.Datetime > DateTime.Now)
            .OrderBy(p => p.Datetime)
            .ToListAsync();

        ViewBag.MyRoles = myRoles;
        ViewBag.MySchedule = mySchedule;
        return View();
    }

    [Authorize(Roles = "Musician")]
    public async Task<IActionResult> Musician()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var mySchedule = await _context.PerformanceGroups
            .Include(pg => pg.Performance).ThenInclude(p => p.Play)
            .Where(pg => pg.ProductionTeam.EmployerId == userId)
            .Select(pg => pg.Performance)
            .Where(p => p.Datetime > DateTime.Now)
            .OrderBy(p => p.Datetime)
            .ToListAsync();

        ViewBag.MySchedule = mySchedule;
        return View();
    }

    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Staff()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var employer = await _context.Employers.FindAsync(userId);
        ViewBag.Employer = employer;
        return View();
    }

    // ========== УПРАВЛЕНИЕ СОТРУДНИКАМИ ==========
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Employees()
    {
        var employees = await _context.Employers.OrderBy(e => e.Fio).ToListAsync();
        ViewBag.Employees = employees;
        return View("~/Views/Admin/Employees/Index.cshtml");
    }

    [Authorize(Roles = "Admin")]
    public IActionResult CreateEmployee()
    {
        return View("~/Views/Admin/Employees/Create.cshtml");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateEmployee(Employer model)
    {
        if (ModelState.IsValid)
        {
            model.Status = "работает";
            _context.Employers.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Сотрудник добавлен";
            return RedirectToAction("Employees");
        }
        return View("~/Views/Admin/Employees/Create.cshtml", model);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EditEmployee(int id)
    {
        var employee = await _context.Employers.FindAsync(id);
        if (employee == null) return NotFound();
        ViewBag.Employee = employee;
        return View("~/Views/Admin/Employees/Edit.cshtml");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> EditEmployee(Employer model)
    {
        if (ModelState.IsValid)
        {
            _context.Employers.Update(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Данные сотрудника обновлены";
            return RedirectToAction("Employees");
        }
        ViewBag.Employee = model;
        return View("~/Views/Admin/Employees/Edit.cshtml", model);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var employee = await _context.Employers.FindAsync(id);
        if (employee != null)
        {
            _context.Employers.Remove(employee);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Сотрудник удалён";
        }
        return RedirectToAction("Employees");
    }

    // ========== УПРАВЛЕНИЕ СПЕКТАКЛЯМИ ==========
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Plays()
    {
        var plays = await _context.Plays
            .Include(p => p.Author)
            .Include(p => p.Genre)
            .OrderBy(p => p.Title)
            .ToListAsync();
        ViewBag.Plays = plays;
        return View("~/Views/Admin/Plays/Index.cshtml");
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreatePlay()
    {
        ViewBag.Authors = await _context.Authors.ToListAsync();
        ViewBag.Genres = await _context.Genres.ToListAsync();
        return View("~/Views/Admin/Plays/Create.cshtml");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreatePlay(Play model)
    {
        if (ModelState.IsValid)
        {
            _context.Plays.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Спектакль добавлен";
            return RedirectToAction("Plays");
        }
        ViewBag.Authors = await _context.Authors.ToListAsync();
        ViewBag.Genres = await _context.Genres.ToListAsync();
        return View("~/Views/Admin/Plays/Create.cshtml", model);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EditPlay(int id)
    {
        var play = await _context.Plays.FindAsync(id);
        if (play == null) return NotFound();
        ViewBag.Play = play;
        ViewBag.Authors = await _context.Authors.ToListAsync();
        ViewBag.Genres = await _context.Genres.ToListAsync();
        return View("~/Views/Admin/Plays/Edit.cshtml");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> EditPlay(Play model)
    {
        if (ModelState.IsValid)
        {
            _context.Plays.Update(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Спектакль обновлён";
            return RedirectToAction("Plays");
        }
        ViewBag.Authors = await _context.Authors.ToListAsync();
        ViewBag.Genres = await _context.Genres.ToListAsync();
        return View("~/Views/Admin/Plays/Edit.cshtml", model);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePlay(int id)
    {
        var play = await _context.Plays.FindAsync(id);
        if (play != null)
        {
            _context.Plays.Remove(play);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Спектакль удалён";
        }
        return RedirectToAction("Plays");
    }

    // ========== УПРАВЛЕНИЕ ПОКАЗАМИ ==========
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Performances()
    {
        var performances = await _context.Performances
            .Include(p => p.Play)
            .OrderByDescending(p => p.Datetime)
            .ToListAsync();
        ViewBag.Performances = performances;
        return View("~/Views/Admin/Performances/Index.cshtml");
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreatePerformance()
    {
        ViewBag.Plays = await _context.Plays.ToListAsync();
        return View("~/Views/Admin/Performances/Create.cshtml");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreatePerformance(Performance model)
    {
        if (ModelState.IsValid)
        {
            _context.Performances.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Показ добавлен";
            return RedirectToAction("Performances");
        }
        ViewBag.Plays = await _context.Plays.ToListAsync();
        return View("~/Views/Admin/Performances/Create.cshtml", model);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EditPerformance(int id)
    {
        var performance = await _context.Performances.FindAsync(id);
        if (performance == null) return NotFound();
        ViewBag.Performance = performance;
        ViewBag.Plays = await _context.Plays.ToListAsync();
        return View("~/Views/Admin/Performances/Edit.cshtml");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> EditPerformance(Performance model)
    {
        if (ModelState.IsValid)
        {
            _context.Performances.Update(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Показ обновлён";
            return RedirectToAction("Performances");
        }
        ViewBag.Plays = await _context.Plays.ToListAsync();
        return View("~/Views/Admin/Performances/Edit.cshtml", model);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePerformance(int id)
    {
        var performance = await _context.Performances.FindAsync(id);
        if (performance != null)
        {
            _context.Performances.Remove(performance);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Показ удалён";
        }
        return RedirectToAction("Performances");
    }

    // ========== ПРОДАННЫЕ БИЛЕТЫ ==========
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Tickets(DateTime? fromDate, DateTime? toDate, int? playId)
    {
        var query = _context.Tickets
            .Include(t => t.Performance).ThenInclude(p => p.Play)
            .Include(t => t.Seat)
            .Where(t => t.Status == "sold");

        if (fromDate.HasValue)
            query = query.Where(t => t.Performance.Datetime.Date >= fromDate.Value.Date);
        if (toDate.HasValue)
            query = query.Where(t => t.Performance.Datetime.Date <= toDate.Value.Date);
        if (playId.HasValue)
            query = query.Where(t => t.Performance.PlayId == playId.Value);

        var tickets = await query
            .OrderByDescending(t => t.Performance.Datetime)
            .ToListAsync();

        ViewBag.Tickets = tickets;
        ViewBag.Plays = await _context.Plays.ToListAsync();
        ViewBag.FromDate = fromDate;
        ViewBag.ToDate = toDate;
        ViewBag.SelectedPlayId = playId;

        return View("~/Views/Admin/Tickets/Index.cshtml");
    }

    // ========== СТАТИСТИКА И АНАЛИТИКА ДЛЯ АДМИНА ==========
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Statistics()
    {
        // 1. Самый популярный спектакль (по продажам)
        var topPlay = await _context.Tickets
            .Include(t => t.Performance)
                .ThenInclude(p => p.Play)
            .Where(t => t.Status == "sold")
            .GroupBy(t => t.Performance.PlayId)
            .Select(g => new
            {
                PlayId = g.Key,
                PlayTitle = g.First().Performance.Play.Title,
                TicketsSold = g.Count()
            })
            .OrderByDescending(x => x.TicketsSold)
            .FirstOrDefaultAsync();

        ViewBag.TopPlay = topPlay;

        // 2. Самый популярный зал
        var topHall = await _context.Tickets
            .Include(t => t.Seat)
                .ThenInclude(s => s.Hall)
            .Where(t => t.Status == "sold" && t.Seat != null && t.Seat.Hall != null)
            .GroupBy(t => t.Seat.HallId)
            .Select(g => new
            {
                HallId = g.Key,
                HallName = g.First().Seat.Hall.Name,
                TicketsSold = g.Count()
            })
            .OrderByDescending(x => x.TicketsSold)
            .FirstOrDefaultAsync();

        ViewBag.TopHall = topHall;

        // 3. Доход по месяцам (последние 6 месяцев)
        var revenueByMonth = await _context.Tickets
            .Where(t => t.Status == "sold" && t.Performance != null)
            .GroupBy(t => new { t.Performance.Datetime.Year, t.Performance.Datetime.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Revenue = g.Sum(t => t.Price)
            })
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .Take(6)
            .ToListAsync();

        ViewBag.RevenueByMonth = revenueByMonth;

        // 4. Количество проданных билетов по дням (последние 7 дней)
        var ticketsByDay = await _context.Tickets
            .Where(t => t.Status == "sold" && t.Performance != null)
            .GroupBy(t => t.Performance.Datetime.Date)
            .Select(g => new
            {
                Date = g.Key,
                Count = g.Count(),
                Revenue = g.Sum(t => t.Price)
            })
            .OrderByDescending(x => x.Date)
            .Take(7)
            .ToListAsync();

        ViewBag.TicketsByDay = ticketsByDay;

        // 5. Общая статистика
        ViewBag.TotalTicketsSold = await _context.Tickets.CountAsync(t => t.Status == "sold");
        ViewBag.TotalRevenue = await _context.Tickets.Where(t => t.Status == "sold").SumAsync(t => t.Price);
        ViewBag.TotalPlays = await _context.Plays.CountAsync();
        ViewBag.TotalEmployees = await _context.Employers.CountAsync();

        // 6. Самый активный кассир (если есть поле CreatedBy в билетах, иначе - просто кто продал)
        // Пока оставим заглушку
        ViewBag.TopCashier = "Ирина Моргоева"; // временно

        // 7. Загрузка залов (процент занятости мест)
        var totalSeats = await _context.Seats.CountAsync();
        var soldTickets = await _context.Tickets.CountAsync(t => t.Status == "sold");
        ViewBag.HallOccupancy = totalSeats > 0 ? Math.Round((double)soldTickets / totalSeats * 100, 1) : 0;

        return View("~/Views/Admin/Statistics.cshtml");

    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> AddEvent(string eventType, int playId, string date, string time, decimal? basePrice, string location)
    {
        var datetime = DateTime.Parse($"{date} {time}");

        if (eventType == "performance")
        {
            // Добавляем показ
            var performance = new Performance
            {
                Datetime = datetime,
                BasePrice = basePrice ?? 1000,
                Status = "запланирован",
                PlayId = playId
            };
            _context.Performances.Add(performance);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Показ успешно добавлен!";
        }
        else if (eventType == "rehearsal")
        {
            // Добавляем репетицию
            var rehearsal = new Rehearsal
            {
                Datetime = datetime,
                Location = location ?? "Большая сцена",
                Description = $"Репетиция спектакля",
                Status = "запланирована",
                PlayId = playId
            };
            _context.Rehearsals.Add(rehearsal);
            await _context.SaveChangesAsync();

            // Добавляем участников (всех актёров, занятых в этом спектакле)
            var actorsInPlay = await _context.ProductionTeams
                .Include(pt => pt.Employer)
                .Where(pt => pt.Role.PlayId == playId && pt.ParticipationType == "актёр")
                .Select(pt => pt.Employer)
                .Distinct()
                .ToListAsync();

            foreach (var actor in actorsInPlay)
            {
                _context.RehearsalParticipants.Add(new RehearsalParticipant
                {
                    RehearsalId = rehearsal.Id,
                    EmployerId = actor.Id,
                    Role = actor.Position ?? "Актёр"
                });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Репетиция успешно добавлена!";
        }

        return RedirectToAction("Schedule");
    }

    // ========== РАСПИСАНИЕ ДЛЯ ВСЕХ РОЛЕЙ ==========
    [Authorize]
    public async Task<IActionResult> Schedule()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Staff";

        List<Performance> performances = new List<Performance>();
        List<Rehearsal> rehearsals = new List<Rehearsal>();

        if (role == "Admin")
        {
            performances = await _context.Performances
                .Include(p => p.Play)
                .Where(p => p.Datetime > DateTime.Now)
                .OrderBy(p => p.Datetime)
                .ToListAsync();

            rehearsals = await _context.Rehearsals
                .Include(r => r.Play)
                .Include(r => r.Participants)
                    .ThenInclude(p => p.Employer)
                .Where(r => r.Datetime > DateTime.Now)
                .OrderBy(r => r.Datetime)
                .ToListAsync();
        }
        else if (role == "Actor" || role == "Musician" || role == "Director")
        {
            // Актёры, музыканты, режиссёры видят только свои репетиции
            var userRehearsals = await _context.RehearsalParticipants
                .Include(rp => rp.Rehearsal)
                    .ThenInclude(r => r.Play)
                .Where(rp => rp.EmployerId == userId && rp.Rehearsal.Datetime > DateTime.Now)
                .Select(rp => rp.Rehearsal)
                .OrderBy(r => r.Datetime)
                .ToListAsync();

            rehearsals = userRehearsals;

            // Показы
            var userPerformances = await _context.PerformanceGroups
                .Include(pg => pg.Performance)
                    .ThenInclude(p => p.Play)
                .Where(pg => pg.ProductionTeam.EmployerId == userId && pg.Performance.Datetime > DateTime.Now)
                .Select(pg => pg.Performance)
                .OrderBy(p => p.Datetime)
                .ToListAsync();

            performances = userPerformances;
        }
        else // Cashier, Staff
        {
            // Кассиры и служащие видят только показы
            performances = await _context.Performances
                .Include(p => p.Play)
                .Where(p => p.Datetime > DateTime.Now)
                .OrderBy(p => p.Datetime)
                .ToListAsync();

            rehearsals = new List<Rehearsal>();
        }

        ViewBag.Performances = performances;
        ViewBag.Rehearsals = rehearsals;
        ViewBag.Plays = await _context.Plays.OrderBy(p => p.Title).ToListAsync();
        ViewBag.UserRole = role;

        return View();
    }
}