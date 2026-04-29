using IrbisTheatre;
using IrbisTheatre.Models;
using Microsoft.EntityFrameworkCore;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

string connectionString = "Host=localhost;Port=5432;Database=IrbisTheatre;Username=admin;Password=ADMIN";

builder.Services.AddDbContext<TheatreContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Инициализация базы данных и заполнение тестовыми данными
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TheatreContext>();

    // 1. Создаём/пересоздаём БД через DatabaseInitializer
    var dbInitializer = new DatabaseInitializer(connectionString);
    dbInitializer.FullRecreate();  // Это создаст БД и таблицы

    // 2. Заполняем тестовыми данными
    var seeder = new TestDataSeeder(context);
    seeder.Seed();
}

app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();