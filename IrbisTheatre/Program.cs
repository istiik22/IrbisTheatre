using IrbisTheatre;
using IrbisTheatre.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы
builder.Services.AddControllersWithViews();

// Добавляем аутентификацию через cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

// Подключение к PostgreSQL
string connectionString = "Host=localhost;Port=5432;Database=IrbisTheatre;Username=admin;Password=ADMIN";

builder.Services.AddDbContext<TheatreContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();


// Настройки для DateTime
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

// Инициализация БД (только при первом запуске)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TheatreContext>();
    var dbInitializer = new DatabaseInitializer(connectionString);

    // Проверяем, есть ли БД
    try
    {
        context.Database.OpenConnection();
        context.Database.CloseConnection();
        Console.WriteLine("База данных уже существует");
    }
    catch
    {
        dbInitializer.FullRecreate();
        var seeder = new TestDataSeeder(context);
        await seeder.SeedAsync();
    }
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();  // ВАЖНО: порядок важен!
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllers();  // 👈 ЭТО ВАЖНО ДЛЯ API

app.Run();
