using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using IrbisTheatre.Models;
using Microsoft.EntityFrameworkCore;

namespace IrbisTheatre.Controllers;

public class AccountController : Controller
{
    private readonly TheatreContext _context;
    private static Dictionary<string, (string Code, DateTime Expiry)> _verificationCodes = new();

    public AccountController(TheatreContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity.IsAuthenticated)
            return RedirectToAction("Index", "Dashboard");
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Ищем по Email
            var employer = await _context.Employers
                .FirstOrDefaultAsync(e => e.Email != null && e.Email == model.Email && e.Status == "работает");

            if (employer != null)
            {
                // Проверяем пароль из БД (временно без хеширования)
                if (model.Password == employer.Password)  
                {
                    var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, employer.Fio),
                    new Claim(ClaimTypes.Email, model.Email),
                    new Claim(ClaimTypes.Role, GetRoleFromPosition(employer.Position ?? "staff")),
                    new Claim("UserId", employer.Id.ToString())
                };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);

                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    TempData["LoginError"] = "Неверный пароль";
                }
            }
            else
            {
                TempData["LoginError"] = "Сотрудник с таким email не найден";
            }
        }
        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity.IsAuthenticated)
            return RedirectToAction("Index", "Dashboard");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Проверяем код подтверждения
            if (!_verificationCodes.ContainsKey(model.Email) ||
                _verificationCodes[model.Email].Code != model.VerificationCode ||
                _verificationCodes[model.Email].Expiry < DateTime.Now)
            {
                ModelState.AddModelError("VerificationCode", "Неверный или истёкший код подтверждения");
                return View(model);
            }

            // Проверяем, не существует ли уже такой email
            var existingEmployer = await _context.Employers
                .FirstOrDefaultAsync(e => e.Contacts != null && e.Contacts == model.Email);

            if (existingEmployer != null)
            {
                ModelState.AddModelError("", "Пользователь с таким email уже зарегистрирован");
                return View(model);
            }

            // Создаём нового сотрудника
            var employer = new Employer
            {
                Fio = model.FullName,
                Contacts = model.Phone,
                Email = model.Email,
                Password = model.Password,  // 👈 СОХРАНЯЕМ ПАРОЛЬ
                Position = GetPositionName(model.Position),
                Status = "работает",
                Salary = 0
            };

            _context.Employers.Add(employer);
            await _context.SaveChangesAsync();

            // Удаляем использованный код
            _verificationCodes.Remove(model.Email);

            // Автоматически входим после регистрации
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, employer.Fio),
                new Claim(ClaimTypes.Email, model.Email),
                new Claim(ClaimTypes.Role, GetRoleFromPosition(model.Position)),
                new Claim("UserId", employer.Id.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            return RedirectToAction("Index", "Dashboard");
        }
        return View(model);
    }

    [HttpPost]
    public IActionResult SendVerificationCode([FromBody] string email)
    {
        if (string.IsNullOrEmpty(email))
            return Json(new { success = false, message = "Email обязателен" });

        // Проверка корпоративного домена
        if (!email.ToLower().Contains("@irbis-theatre.ru"))
            return Json(new { success = false, message = "Используйте корпоративную почту @irbis-theatre.ru" });

        var random = new Random();
        var code = random.Next(100000, 999999).ToString();

        _verificationCodes[email] = (code, DateTime.Now.AddMinutes(10));

        // В реальной системе здесь отправка email
        return Json(new { success = true, code = code });
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    // Вспомогательные методы
    private string GetRoleFromPosition(string position)
    {
        var positionLower = position.ToLower();
        if (positionLower.Contains("админ") || positionLower == "admin")
            return "Admin";
        if (positionLower.Contains("директор") || positionLower == "director")
            return "Director";
        if (positionLower.Contains("кассир") || positionLower == "cashier")
            return "Cashier";
        if (positionLower.Contains("актёр") || positionLower == "actor")
            return "Actor";
        if (positionLower.Contains("музыкант") || positionLower == "musician")
            return "Musician";
        return "Staff";
    }

    private string GetPositionName(string position)
    {
        return position switch
        {
            "admin" => "Администратор",
            "director" => "Директор",
            "cashier" => "Кассир",
            "actor" => "Актёр",
            "musician" => "Музыкант",
            "staff" => "Служащий",
            _ => position
        };
    }
}