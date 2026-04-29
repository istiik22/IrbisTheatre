using IrbisTheatre.Models;

namespace IrbisTheatre;

public class TestDataSeeder
{
    private readonly TheatreContext _context;

    public TestDataSeeder(TheatreContext context)
    {
        _context = context;
    }

    public void Seed()
    {
        if (_context.Authors.Any())
        {
            Console.WriteLine("Данные уже существуют, пропускаем заполнение...");
            return;
        }

        Console.WriteLine("Начинаем заполнение тестовыми данными...");

        var authors = new List<Author>
        {
            new Author { Fio = "Михаил Юрьевич Лермонтов", Country = "Россия", YearsOfLife = "1814-1841",
                Biography = "Русский поэт, прозаик, драматург. Много писал о Кавказе, где служил и воевал." },
            new Author { Fio = "Александр Сергеевич Грибоедов", Country = "Россия", YearsOfLife = "1795-1829",
                Biography = "Русский дипломат, поэт, драматург. Автор 'Горя от ума'. Служил на Кавказе и погиб в Тегеране." },
            new Author { Fio = "Лев Николаевич Толстой", Country = "Россия", YearsOfLife = "1828-1910",
                Biography = "Великий русский писатель. Служил на Кавказе, написал 'Кавказского пленника' и 'Хаджи-Мурата'." },
            new Author { Fio = "Фазу Алиева", Country = "Дагестан", YearsOfLife = "1932-2016",
                Biography = "Известная дагестанская поэтесса, писала на аварском и русском языках." },
            new Author { Fio = "Расул Гамзатов", Country = "Дагестан", YearsOfLife = "1923-2003",
                Biography = "Великий аварский поэт, автор знаменитых 'Журавлей'." },
            new Author { Fio = "Коста Хетагуров", Country = "Осетия", YearsOfLife = "1859-1906",
                Biography = "Основоположник осетинской литературы, поэт, художник, просветитель." },
            
            new Author { Fio = "Антон Павлович Чехов", Country = "Россия", YearsOfLife = "1860-1904",
                Biography = "Великий русский писатель, драматург, врач по образованию." },
            new Author { Fio = "Николай Васильевич Гоголь", Country = "Россия", YearsOfLife = "1809-1852",
                Biography = "Русский прозаик, драматург, поэт, критик, публицист." },
            new Author { Fio = "Фёдор Михайлович Достоевский", Country = "Россия", YearsOfLife = "1821-1881",
                Biography = "Великий русский писатель, мыслитель, философ." },
            new Author { Fio = "Уильям Шекспир", Country = "Англия", YearsOfLife = "1564-1616",
                Biography = "Английский поэт и драматург, величайший писатель в мировой литературе." }
        };
        _context.Authors.AddRange(authors);
        _context.SaveChanges();

        var genres = new List<Genre>
        {
            new Genre { Name = "Драма" },
            new Genre { Name = "Комедия" },
            new Genre { Name = "Трагедия" },
            new Genre { Name = "Мелодрама" },
            new Genre { Name = "Сатира" },
            new Genre { Name = "Трагикомедия" },
            new Genre { Name = "Притча" },
            new Genre { Name = "Поэтическая драма" }
        };
        _context.Genres.AddRange(genres);
        _context.SaveChanges();

        var halls = new List<Hall>
        {
            new Hall { Name = "Большая сцена" },
            new Hall { Name = "Малая сцена" },
            new Hall { Name = "Камерный зал" },
            new Hall { Name = "Зимний сад" }
        };
        _context.Halls.AddRange(halls);
        _context.SaveChanges();

        var seats = new List<Seat>();

        foreach (var hall in halls)
        {
            for (short row = 1; row <= 5; row++)
            {
                for (short seatNum = 1; seatNum <= 10; seatNum++)
                {
                    string category = row <= 2 ? "Партер" : row <= 4 ? "Бельэтаж" : "Балкон";
                    seats.Add(new Seat
                    {
                        RowNumber = row,
                        SeatNumber = seatNum,
                        Category = category,
                        HallId = hall.Id
                    });
                }
            }
        }
        _context.Seats.AddRange(seats);
        _context.SaveChanges();

        var plays = new List<Play>
        {
            new Play
            {
                Title = "Мцыри",
                Description = "Поэма о юном послушнике, который сбегает из монастыря в горы Кавказа, чтобы обрести свободу. За три дня он проживает целую жизнь, полную опасностей, любви к природе и борьбы с барсом.",
                TargetAudience = "12+",
                PremiereDate = new DateTime(2024, 3, 15),
                AuthorId = authors.First(a => a.Fio.Contains("Лермонтов")).Id,
                GenreId = genres.First(g => g.Name == "Поэтическая драма").Id
            },
            new Play
            {
                Title = "Герой нашего времени",
                Description = "История 'лишнего человека' Григория Печорина, его любовных приключений и дуэлей на Кавказе. Психологический портрет молодого человека 19 века.",
                TargetAudience = "16+",
                PremiereDate = new DateTime(2024, 5, 20),
                AuthorId = authors.First(a => a.Fio.Contains("Лермонтов")).Id,
                GenreId = genres.First(g => g.Name == "Драма").Id
            },
            new Play
            {
                Title = "Горе от ума (Кавказская история)",
                Description = "Знаменитая комедия о столкновении 'века нынешнего' и 'века минувшего', перенесенная на кавказскую почву. Чацкий возвращается на Кавказ и сталкивается с местным 'фамусовским' обществом.",
                TargetAudience = "12+",
                PremiereDate = new DateTime(2024, 7, 10),
                AuthorId = authors.First(a => a.Fio.Contains("Грибоедов")).Id,
                GenreId = genres.First(g => g.Name == "Комедия").Id
            },
            new Play
            {
                Title = "Кавказский пленник",
                Description = "История русского офицера Жилина, который попадает в плен к горцам. О смелости, дружбе и взаимопомощи, преодолевающей национальные барьеры.",
                TargetAudience = "6+",
                PremiereDate = new DateTime(2024, 2, 1),
                AuthorId = authors.First(a => a.Fio.Contains("Толстой")).Id,
                GenreId = genres.First(g => g.Name == "Драма").Id
            },
            new Play
            {
                Title = "Хаджи-Мурат",
                Description = "История легендарного аварского наиба, перешедшего на сторону русских. Трагедия о выборе между честью и предательством, о цене свободы.",
                TargetAudience = "18+",
                PremiereDate = new DateTime(2025, 1, 25),
                AuthorId = authors.First(a => a.Fio.Contains("Толстой")).Id,
                GenreId = genres.First(g => g.Name == "Трагедия").Id
            },
            new Play
            {
                Title = "Журавли",
                Description = "Поэтическая притча о войне, памяти и братстве народов Кавказа. Основано на знаменитом стихотворении Расула Гамзатова.",
                TargetAudience = "6+",
                PremiereDate = new DateTime(2024, 5, 9),
                AuthorId = authors.First(a => a.Fio.Contains("Гамзатов")).Id,
                GenreId = genres.First(g => g.Name == "Притча").Id
            },
            new Play
            {
                Title = "Сын гор",
                Description = "Драма о жизни дагестанского горца, его любви к родине и трагической судьбе в современном мире. По произведениям Фазу Алиевой.",
                TargetAudience = "12+",
                PremiereDate = new DateTime(2024, 10, 15),
                AuthorId = authors.First(a => a.Fio.Contains("Алиева")).Id,
                GenreId = genres.First(g => g.Name == "Драма").Id
            },
            new Play
            {
                Title = "Фатима",
                Description = "Трагедия осетинской девушки, разрывающейся между любовью и долгом перед семьей и народом. Классика осетинской литературы.",
                TargetAudience = "16+",
                PremiereDate = new DateTime(2024, 12, 1),
                AuthorId = authors.First(a => a.Fio.Contains("Хетагуров")).Id,
                GenreId = genres.First(g => g.Name == "Трагедия").Id
            },
            
            new Play
            {
                Title = "Вишневый сад",
                Description = "Последняя пьеса Чехова о дворянском гнезде, которое уходит в прошлое. Лирическая комедия о потерях и надеждах.",
                TargetAudience = "12+",
                PremiereDate = new DateTime(2024, 9, 1),
                AuthorId = authors.First(a => a.Fio.Contains("Чехов")).Id,
                GenreId = genres.First(g => g.Name == "Комедия").Id
            },
            new Play
            {
                Title = "Ревизор",
                Description = "Знаменитая сатирическая комедия о чиновниках уездного города, принявших мелкого вора за важного ревизора.",
                TargetAudience = "12+",
                PremiereDate = new DateTime(2024, 4, 1),
                AuthorId = authors.First(a => a.Fio.Contains("Гоголь")).Id,
                GenreId = genres.First(g => g.Name == "Сатира").Id
            },
            new Play
            {
                Title = "Преступление и наказание",
                Description = "Инсценировка романа Достоевского о студенте Раскольникове, который проверяет теорию о 'право имеющих' на себе.",
                TargetAudience = "18+",
                PremiereDate = new DateTime(2024, 11, 20),
                AuthorId = authors.First(a => a.Fio.Contains("Достоевский")).Id,
                GenreId = genres.First(g => g.Name == "Драма").Id
            },
            new Play
            {
                Title = "Гамлет",
                Description = "Бессмертная трагедия Шекспира о датском принце, который пытается отомстить за убийство отца.",
                TargetAudience = "16+",
                PremiereDate = new DateTime(2024, 8, 15),
                AuthorId = authors.First(a => a.Fio.Contains("Шекспир")).Id,
                GenreId = genres.First(g => g.Name == "Трагедия").Id
            }
        };
        _context.Plays.AddRange(plays);
        _context.SaveChanges();

        var roles = new List<Role>();

        var mtsyri = plays.First(p => p.Title == "Мцыри");
        roles.AddRange(new[]
        {
            new Role { Name = "Мцыри", GenderRequirement = "male", AgeRange = "16-25", VoiceRequirements = "тенор, эмоциональный", PlayId = mtsyri.Id },
            new Role { Name = "Старый монах", GenderRequirement = "male", AgeRange = "50-70", VoiceRequirements = "бас, размеренный", PlayId = mtsyri.Id },
            new Role { Name = "Грузинка", GenderRequirement = "female", AgeRange = "18-25", VoiceRequirements = "сопрано, нежное", PlayId = mtsyri.Id },
            new Role { Name = "Барс", GenderRequirement = "any", AgeRange = "25-40", VoiceRequirements = "сильный, звериный", PlayId = mtsyri.Id }
        });

        var prisoner = plays.First(p => p.Title == "Кавказский пленник");
        roles.AddRange(new[]
        {
            new Role { Name = "Жилин", GenderRequirement = "male", AgeRange = "30-45", VoiceRequirements = "баритон, уверенный", PlayId = prisoner.Id },
            new Role { Name = "Костылин", GenderRequirement = "male", AgeRange = "30-45", VoiceRequirements = "тенор, трусливый", PlayId = prisoner.Id },
            new Role { Name = "Дина", GenderRequirement = "female", AgeRange = "13-16", VoiceRequirements = "сопрано, звонкое", PlayId = prisoner.Id },
            new Role { Name = "Абдул-Мурат", GenderRequirement = "male", AgeRange = "40-55", VoiceRequirements = "бас, властный", PlayId = prisoner.Id }
        });

        var cherry = plays.First(p => p.Title == "Вишневый сад");
        roles.AddRange(new[]
        {
            new Role { Name = "Раневская Любовь Андреевна", GenderRequirement = "female", AgeRange = "40-50", VoiceRequirements = "контральто, эмоциональное", PlayId = cherry.Id },
            new Role { Name = "Лопахин Ермолай Алексеевич", GenderRequirement = "male", AgeRange = "35-45", VoiceRequirements = "баритон, энергичный", PlayId = cherry.Id },
            new Role { Name = "Трофимов Петр Сергеевич", GenderRequirement = "male", AgeRange = "25-30", VoiceRequirements = "тенор, восторженный", PlayId = cherry.Id },
            new Role { Name = "Аня", GenderRequirement = "female", AgeRange = "17-20", VoiceRequirements = "сопрано, юное", PlayId = cherry.Id }
        });

        var hamlet = plays.First(p => p.Title == "Гамлет");
        roles.AddRange(new[]
        {
            new Role { Name = "Гамлет", GenderRequirement = "male", AgeRange = "30-40", VoiceRequirements = "драматический тенор", PlayId = hamlet.Id },
            new Role { Name = "Офелия", GenderRequirement = "female", AgeRange = "20-25", VoiceRequirements = "лирическое сопрано", PlayId = hamlet.Id },
            new Role { Name = "Клавдий", GenderRequirement = "male", AgeRange = "50-60", VoiceRequirements = "бас, зловещий", PlayId = hamlet.Id },
            new Role { Name = "Гертруда", GenderRequirement = "female", AgeRange = "45-55", VoiceRequirements = "меццо-сопрано", PlayId = hamlet.Id }
        });

        _context.Roles.AddRange(roles);
        _context.SaveChanges();

        var employers = new List<Employer>
        {
            new Employer { Fio = "Иван Петрович Смирнов", Gender = "male", BirthDate = new DateTime(1985, 5, 15),
                Position = "Ведущий актер", Salary = 85000, Status = "работает", Contacts = "+7 (999) 123-45-67" },
            new Employer { Fio = "Мария Александровна Волкова", Gender = "female", BirthDate = new DateTime(1990, 3, 22),
                Position = "Актриса", Salary = 75000, Status = "работает", Contacts = "+7 (999) 234-56-78" },
            new Employer { Fio = "Алексей Николаевич Морозов", Gender = "male", BirthDate = new DateTime(1988, 11, 8),
                Position = "Актер", Salary = 70000, Status = "работает", Contacts = "+7 (999) 345-67-89" },
            new Employer { Fio = "Елена Дмитриевна Козлова", Gender = "female", BirthDate = new DateTime(1995, 7, 30),
                Position = "Молодая актриса", Salary = 55000, Status = "работает", Contacts = "+7 (999) 456-78-90" },
            new Employer { Fio = "Сергей Владимирович Новиков", Gender = "male", BirthDate = new DateTime(1982, 1, 12),
                Position = "Режиссер", Salary = 120000, Status = "работает", Contacts = "+7 (999) 567-89-01" },
            new Employer { Fio = "Ольга Павловна Соколова", Gender = "female", BirthDate = new DateTime(1987, 9, 5),
                Position = "Художник по костюмам", Salary = 65000, Status = "работает", Contacts = "+7 (999) 678-90-12" },
            new Employer { Fio = "Дмитрий Андреевич Лебедев", Gender = "male", BirthDate = new DateTime(1992, 4, 18),
                Position = "Актер", Salary = 60000, Status = "работает", Contacts = "+7 (999) 789-01-23" },
            new Employer { Fio = "Тамерлан Асланович Караев", Gender = "male", BirthDate = new DateTime(1989, 8, 25),
                Position = "Актер (кавказские роли)", Salary = 80000, Status = "работает", Contacts = "+7 (999) 890-12-34" },
            new Employer { Fio = "Замира Расуловна Гамзатова", Gender = "female", BirthDate = new DateTime(1991, 12, 10),
                Position = "Актриса (национальные роли)", Salary = 75000, Status = "работает", Contacts = "+7 (999) 901-23-45" }
        };
        _context.Employers.AddRange(employers);
        _context.SaveChanges();

        var productionTeams = new List<ProductionTeam>();

        var allRoles = _context.Roles.ToList();
        var allEmployers = _context.Employers.ToList();

        foreach (var role in allRoles.Take(10)) 
        {
            var randomActor = allEmployers.Where(e =>
                (role.GenderRequirement == "any" ||
                 (role.GenderRequirement == "male" && e.Gender == "male") ||
                 (role.GenderRequirement == "female" && e.Gender == "female"))).FirstOrDefault();

            if (randomActor != null)
            {
                productionTeams.Add(new ProductionTeam
                {
                    EmployerId = randomActor.Id,
                    RoleId = role.Id,
                    ParticipationType = "актёр",
                    ProductionPosition = role.Name
                });
            }
        }

        var directors = allEmployers.Where(e => e.Position == "Режиссер").ToList();
        var somePlays = _context.Plays.Take(4).ToList();
        var someRoles = _context.Roles.Where(r => r.Name.Contains("Гамлет") || r.Name.Contains("Раневская")).ToList();

        foreach (var role in someRoles)
        {
            if (directors.Any())
            {
                productionTeams.Add(new ProductionTeam
                {
                    EmployerId = directors.First().Id,
                    RoleId = role.Id,
                    ParticipationType = "режиссёр-постановщик",
                    ProductionPosition = $"Режиссура спектакля {role.Play?.Title ?? "текущего спектакля"}"
                });
            }
        }

        _context.ProductionTeams.AddRange(productionTeams);
        _context.SaveChanges();

        var performances = new List<Performance>();

        foreach (var play in plays)
        {
            var dates = new[]
            {
                DateTime.Now.AddDays(Random.Shared.Next(1, 30)),
                DateTime.Now.AddDays(Random.Shared.Next(31, 60)),
                DateTime.Now.AddDays(Random.Shared.Next(61, 90))
            };

            foreach (var date in dates.Take(play.Title == "Мцыри" || play.Title == "Гамлет" ? 3 : 2))
            {
                performances.Add(new Performance
                {
                    Datetime = date.Date.AddHours(19), 
                    BasePrice = 800 + Random.Shared.Next(0, 500),
                    IsPremiere = play.PremiereDate?.Date == date.Date,
                    Status = "запланирован",
                    PlayId = play.Id
                });
            }
        }

        _context.Performances.AddRange(performances);
        _context.SaveChanges();

        var tickets = new List<Ticket>();
        int ticketCounter = 1;

        foreach (var performance in performances)
        {
            var seatsForPerformance = _context.Seats.Take(30).ToList(); 

            foreach (var seat in seatsForPerformance)
            {
                string status;
                int priceMultiplier = seat.Category == "Партер" ? 2 : seat.Category == "Бельэтаж" ? 1 : 1;

                if (ticketCounter % 3 == 0)
                    status = "sold";
                else if (ticketCounter % 5 == 0)
                    status = "reserved";
                else
                    status = "free";

                tickets.Add(new Ticket
                {
                    UniqueNumber = $"TKT{performance.Datetime:yyyyMMdd}-{ticketCounter:D6}",
                    Price = performance.BasePrice * priceMultiplier,
                    Status = status,
                    PerformanceId = performance.Id,
                    SeatId = seat.Id
                });
                ticketCounter++;
            }
        }

        _context.Tickets.AddRange(tickets);
        _context.SaveChanges();

        var performanceGroups = new List<PerformanceGroup>();
        var allProductionTeams = _context.ProductionTeams.ToList();

        foreach (var performance in performances.Take(5)) 
        {
            var teamForPerformance = allProductionTeams.Take(3); 
            foreach (var team in teamForPerformance)
            {
                performanceGroups.Add(new PerformanceGroup
                {
                    PerformanceId = performance.Id,
                    ProductionTeamId = team.Id
                });
            }
        }

        _context.PerformanceGroups.AddRange(performanceGroups);
        _context.SaveChanges();

        Console.WriteLine($"Тестовые данные успешно добавлены!");
        Console.WriteLine($"Добавлено: {authors.Count} авторов, {genres.Count} жанров, {plays.Count} пьес");
        Console.WriteLine($"Добавлено: {roles.Count} ролей, {employers.Count} сотрудников");
        Console.WriteLine($"Добавлено: {performances.Count} показов, {tickets.Count} билетов");
    }
}