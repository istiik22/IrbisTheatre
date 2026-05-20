using IrbisTheatre.Models;
using Microsoft.EntityFrameworkCore;

namespace IrbisTheatre;

public class TestDataSeeder
{
    private readonly TheatreContext _context;

    public TestDataSeeder(TheatreContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        if (await _context.Authors.AnyAsync())
        {
            Console.WriteLine("Данные уже существуют, пропускаем заполнение...");
            return;
        }

        Console.WriteLine("Начинаем заполнение тестовыми данными...");

        // ================================================================
        // 1. АВТОРЫ (осетинские и кавказские)
        // ================================================================
        var authors = new List<Author>
        {
            new Author { Fio = "Коста Леванович Хетагуров", Country = "Осетия", YearsOfLife = "1859-1906",
                Biography = "Основоположник осетинской литературы, поэт, художник, просветитель. Писал на осетинском и русском языках." },
            new Author { Fio = "Арсен Борисович Коцоев", Country = "Осетия", YearsOfLife = "1872-1944",
                Biography = "Осетинский писатель, драматург, основоположник осетинской драматургии." },
            new Author { Fio = "Георгий Меркулович Цаголов", Country = "Осетия", YearsOfLife = "1888-1939",
                Biography = "Осетинский писатель, поэт, драматург, один из зачинателей осетинской литературы." },
            new Author { Fio = "Дзахо Алексеевич Гатуев", Country = "Осетия", YearsOfLife = "1865-1938",
                Biography = "Осетинский поэт, прозаик, драматург и публицист." },
            new Author { Fio = "Евгений Иванович Уруймагова", Country = "Осетия", YearsOfLife = "1904-1985",
                Biography = "Осетинская писательница, автор знаменитого романа 'Сослан и Кристина'." },
            new Author { Fio = "Михаил Юрьевич Лермонтов", Country = "Россия", YearsOfLife = "1814-1841",
                Biography = "Русский поэт, прозаик, драматург. Много писал о Кавказе." },
            new Author { Fio = "Лев Николаевич Толстой", Country = "Россия", YearsOfLife = "1828-1910",
                Biography = "Великий русский писатель. Служил на Кавказе." },
            new Author { Fio = "Уильям Шекспир", Country = "Англия", YearsOfLife = "1564-1616",
                Biography = "Английский поэт и драматург." }
        };
        _context.Authors.AddRange(authors);
        await _context.SaveChangesAsync();

        // ================================================================
        // 2. ЖАНРЫ
        // ================================================================
        var genres = new List<Genre>
        {
            new Genre { Name = "Драма" },
            new Genre { Name = "Комедия" },
            new Genre { Name = "Трагедия" },
            new Genre { Name = "Мелодрама" },
            new Genre { Name = "Сатира" },
            new Genre { Name = "Трагикомедия" },
            new Genre { Name = "Притча" },
            new Genre { Name = "Поэтическая драма" },
            new Genre { Name = "Историческая драма" },
            new Genre { Name = "Лирическая комедия" }
        };
        _context.Genres.AddRange(genres);
        await _context.SaveChangesAsync();

        // ================================================================
        // 3. ЗАЛЫ
        // ================================================================
        var halls = new List<Hall>
        {
            new Hall { Name = "Большой зал им. Коста Хетагурова" },
            new Hall { Name = "Малый зал 'Нарт'" },
            new Hall { Name = "Камерный зал 'Амонд'" },
            new Hall { Name = "Зимний сад 'Стыр Ныхас'" }
        };
        _context.Halls.AddRange(halls);
        await _context.SaveChangesAsync();

        // ================================================================
        // 4. МЕСТА В ЗАЛАХ
        // ================================================================
        var seats = new List<Seat>();
        foreach (var hall in halls)
        {
            for (short row = 1; row <= 8; row++)
            {
                for (short seatNum = 1; seatNum <= 12; seatNum++)
                {
                    string category = row <= 2 ? "Партер-люкс" : row <= 4 ? "Партер" : row <= 6 ? "Бельэтаж" : "Балкон";
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
        await _context.SaveChangesAsync();

        // ================================================================
        // 5. СПЕКТАКЛИ (осетинская тематика)
        // ================================================================
        var plays = new List<Play>
        {
            // Осетинские спектакли
            new Play
            {
                Title = "Дуня (Фатима)",
                Description = "Трагедия осетинской девушки, разрывающейся между любовью и долгом перед семьей и народом. Классика осетинской литературы.",
                TargetAudience = "16+",
                PremiereDate = new DateTime(2024, 10, 15),
                AuthorId = authors.First(a => a.Fio.Contains("Хетагуров")).Id,
                GenreId = genres.First(g => g.Name == "Трагедия").Id
            },
            new Play
            {
                Title = "Сослан и Кристина",
                Description = "Знаменитый роман о любви осетинского нарта Сослана и русской девушки Кристины, переплетение судеб и культур.",
                TargetAudience = "12+",
                PremiereDate = new DateTime(2024, 12, 1),
                AuthorId = authors.First(a => a.Fio.Contains("Уруймагова")).Id,
                GenreId = genres.First(g => g.Name == "Драма").Id
            },
            new Play
            {
                Title = "Таранджелоз",
                Description = "Комедия о похождениях героя осетинского фольклора, полная юмора и народной мудрости.",
                TargetAudience = "6+",
                PremiereDate = new DateTime(2024, 11, 20),
                AuthorId = authors.First(a => a.Fio.Contains("Коцоев")).Id,
                GenreId = genres.First(g => g.Name == "Комедия").Id
            },
            new Play
            {
                Title = "Хазби",
                Description = "Драма о судьбе горца, его любви и трагическом выборе между честью и свободой.",
                TargetAudience = "16+",
                PremiereDate = new DateTime(2025, 1, 15),
                AuthorId = authors.First(a => a.Fio.Contains("Цаголов")).Id,
                GenreId = genres.First(g => g.Name == "Трагедия").Id
            },
            new Play
            {
                Title = "Нарт Сослан",
                Description = "Эпическое сказание о великом нартском герое, его подвигах и любви к прекрасной Косер.",
                TargetAudience = "6+",
                PremiereDate = new DateTime(2024, 9, 5),
                AuthorId = authors.First(a => a.Fio.Contains("Гатуев")).Id,
                GenreId = genres.First(g => g.Name == "Поэтическая драма").Id
            },
            new Play
            {
                Title = "Залинка",
                Description = "Лирическая история о первой любви, расцветающей в горах Кавказа. Нежная и трогательная комедия.",
                TargetAudience = "6+",
                PremiereDate = new DateTime(2024, 8, 20),
                AuthorId = authors.First(a => a.Fio.Contains("Коцоев")).Id,
                GenreId = genres.First(g => g.Name == "Лирическая комедия").Id
            },
            new Play
            {
                Title = "Мцыри",
                Description = "Поэма о юном послушнике, который сбегает из монастыря в горы Кавказа, чтобы обрести свободу.",
                TargetAudience = "12+",
                PremiereDate = new DateTime(2024, 3, 15),
                AuthorId = authors.First(a => a.Fio.Contains("Лермонтов")).Id,
                GenreId = genres.First(g => g.Name == "Поэтическая драма").Id
            },
            new Play
            {
                Title = "Кавказский пленник",
                Description = "История русского офицера Жилина, который попадает в плен к горцам.",
                TargetAudience = "6+",
                PremiereDate = new DateTime(2024, 2, 1),
                AuthorId = authors.First(a => a.Fio.Contains("Толстой")).Id,
                GenreId = genres.First(g => g.Name == "Драма").Id
            },
            new Play
            {
                Title = "Гамлет",
                Description = "Бессмертная трагедия Шекспира о датском принце, перенесённая на кавказскую почву.",
                TargetAudience = "16+",
                PremiereDate = new DateTime(2024, 8, 15),
                AuthorId = authors.First(a => a.Fio.Contains("Шекспир")).Id,
                GenreId = genres.First(g => g.Name == "Трагедия").Id
            }
        };
        _context.Plays.AddRange(plays);
        await _context.SaveChangesAsync();

        // ================================================================
        // 6. РОЛИ В СПЕКТАКЛЯХ
        // ================================================================
        var roles = new List<Role>();

        // Роли для "Дуня (Фатима)"
        var fatima = plays.First(p => p.Title.Contains("Дуня"));
        roles.AddRange(new[]
        {
            new Role { Name = "Фатима", GenderRequirement = "female", AgeRange = "20-30", VoiceRequirements = "драматическое сопрано", PlayId = fatima.Id },
            new Role { Name = "Батрадз", GenderRequirement = "male", AgeRange = "25-35", VoiceRequirements = "баритон", PlayId = fatima.Id },
            new Role { Name = "Мать Фатимы", GenderRequirement = "female", AgeRange = "45-60", VoiceRequirements = "контральто", PlayId = fatima.Id },
            new Role { Name = "Князь", GenderRequirement = "male", AgeRange = "40-55", VoiceRequirements = "бас", PlayId = fatima.Id }
        });

        // Роли для "Нарт Сослан"
        var soslan = plays.First(p => p.Title.Contains("Нарт Сослан"));
        roles.AddRange(new[]
        {
            new Role { Name = "Сослан", GenderRequirement = "male", AgeRange = "25-40", VoiceRequirements = "тенор, героический", PlayId = soslan.Id },
            new Role { Name = "Косер", GenderRequirement = "female", AgeRange = "20-30", VoiceRequirements = "лирическое сопрано", PlayId = soslan.Id },
            new Role { Name = "Урызмаг", GenderRequirement = "male", AgeRange = "50-65", VoiceRequirements = "бас, мудрый", PlayId = soslan.Id },
            new Role { Name = "Шатана", GenderRequirement = "female", AgeRange = "40-55", VoiceRequirements = "меццо-сопрано", PlayId = soslan.Id },
            new Role { Name = "Батраз", GenderRequirement = "male", AgeRange = "20-35", VoiceRequirements = "баритон, страстный", PlayId = soslan.Id }
        });

        // Роли для "Таранджелоз"
        var taran = plays.First(p => p.Title.Contains("Таранджелоз"));
        roles.AddRange(new[]
        {
            new Role { Name = "Таранджелоз", GenderRequirement = "male", AgeRange = "25-45", VoiceRequirements = "комический тенор", PlayId = taran.Id },
            new Role { Name = "Залина", GenderRequirement = "female", AgeRange = "20-25", VoiceRequirements = "сопрано", PlayId = taran.Id },
            new Role { Name = "Дзамболат", GenderRequirement = "male", AgeRange = "40-55", VoiceRequirements = "бас, комический", PlayId = taran.Id }
        });

        // Роли для "Хазби"
        var khazbi = plays.First(p => p.Title.Contains("Хазби"));
        roles.AddRange(new[]
        {
            new Role { Name = "Хазби", GenderRequirement = "male", AgeRange = "30-40", VoiceRequirements = "драматический баритон", PlayId = khazbi.Id },
            new Role { Name = "Замира", GenderRequirement = "female", AgeRange = "20-28", VoiceRequirements = "сопрано", PlayId = khazbi.Id },
            new Role { Name = "Старейшина", GenderRequirement = "male", AgeRange = "60-75", VoiceRequirements = "бас", PlayId = khazbi.Id }
        });

        _context.Roles.AddRange(roles);
        await _context.SaveChangesAsync();

        // ================================================================
        // 7. СОТРУДНИКИ ТЕАТРА (ВСЕ РОЛИ - ОСЕТИНСКИЕ ФАМИЛИИ)
        // ================================================================
        var employers = new List<Employer>
        {
            // РУКОВОДСТВО
            new Employer { Fio = "Таймураз Георгиевич Дзарасов", Gender = "male", BirthDate = new DateTime(1970, 5, 15),
                Position = "Директор театра", Salary = 250000, Status = "работает",
                Contacts = "+7 (999) 100-00-01", Email = "director@irbis-theatre.ru", Password = "password123" },

            new Employer { Fio = "Алана Руслановна Догузова", Gender = "female", BirthDate = new DateTime(1980, 8, 22),
                Position = "Художественный руководитель", Salary = 200000, Status = "работает",
                Contacts = "+7 (999) 100-00-02", Email = "art.director@irbis-theatre.ru", Password = "password123" },

            new Employer { Fio = "Сослан Борисович Тедеев", Gender = "male", BirthDate = new DateTime(1975, 3, 10),
                Position = "Главный режиссёр", Salary = 180000, Status = "работает",
                Contacts = "+7 (999) 100-00-03", Email = "chief.director@irbis-theatre.ru", Password = "password123" },

            // РЕЖИССЁРЫ
            new Employer { Fio = "Заурбек Казбекович Дзантиев", Gender = "male", BirthDate = new DateTime(1982, 7, 19),
                Position = "Режиссёр-постановщик", Salary = 150000, Status = "работает",
                Contacts = "+7 (999) 100-00-04", Email = "zaur.dzantiev@irbis-theatre.ru", Password = "password123" },

            new Employer { Fio = "Фатима Муратовна Дзуцева", Gender = "female", BirthDate = new DateTime(1988, 11, 2),
                Position = "Режиссёр", Salary = 130000, Status = "работает",
                Contacts = "+7 (999) 100-00-05", Email = "fatima.dzutseva@irbis-theatre.ru", Password = "password123" },

            // АКТЁРЫ
            new Employer { Fio = "Марат Казбекович Бирагов", Gender = "male", BirthDate = new DateTime(1985, 3, 12),
                Position = "Ведущий артист драмы", Salary = 95000, Status = "работает",
                Contacts = "+7 (999) 100-00-10", Email = "marat.biragov@irbis-theatre.ru", Password = "password123" },

            new Employer { Fio = "Фатима Таймуразовна Хетагурова", Gender = "female", BirthDate = new DateTime(1987, 7, 25),
                Position = "Ведущая артистка", Salary = 95000, Status = "работает",
                Contacts = "+7 (999) 100-00-11", Email = "fatima.khetagurova@irbis-theatre.ru", Password = "password123" },

            new Employer { Fio = "Сослан Эльбрусович Гуцунаев", Gender = "male", BirthDate = new DateTime(1990, 1, 18),
                Position = "Артист драмы", Salary = 75000, Status = "работает",
                Contacts = "+7 (999) 100-00-12", Email = "soslan.gutsunaev@irbis-theatre.ru", Password = "password123" },

            new Employer { Fio = "Диана Ахсарбековна Дзарахохова", Gender = "female", BirthDate = new DateTime(1992, 5, 9),
                Position = "Артистка", Salary = 70000, Status = "работает",
                Contacts = "+7 (999) 100-00-13", Email = "diana.dzarakhokhova@irbis-theatre.ru", Password = "password123" },

            new Employer { Fio = "Ацамаз Батразович Тогоев", Gender = "male", BirthDate = new DateTime(1988, 9, 14),
                Position = "Артист", Salary = 72000, Status = "работает",
                Contacts = "+7 (999) 100-00-14", Email = "atsamaz.togoev@irbis-theatre.ru", Password = "password123" },

            new Employer { Fio = "Зарема Сослановна Джиоева", Gender = "female", BirthDate = new DateTime(1991, 12, 3),
                Position = "Артистка", Salary = 70000, Status = "работает",
                Contacts = "+7 (999) 100-00-15", Email = "zarema.dzhioeva@irbis-theatre.ru", Password = "password123" },

            new Employer { Fio = "Чермен Вячеславович Касаев", Gender = "male", BirthDate = new DateTime(1993, 4, 27),
                Position = "Артист", Salary = 65000, Status = "работает",
                Contacts = "+7 (999) 100-00-16", Email = "chermen.kasaev@irbis-theatre.ru", Password = "password123" },

            new Employer { Fio = "Лаура Азаматовна Бестаева", Gender = "female", BirthDate = new DateTime(1994, 10, 20),
                Position = "Артистка", Salary = 65000, Status = "работает",
                Contacts = "+7 (999) 100-00-17", Email = "laura.bestaeva@irbis-theatre.ru", Password = "password123" },

            // КАССИРЫ
            new Employer { Fio = "Ирина Казбековна Моргоева", Gender = "female", BirthDate = new DateTime(1985, 6, 15),
                Position = "Старший кассир", Salary = 45000, Status = "работает",
                Contacts = "+7 (999) 100-00-20", Email = "irina.morgoeva@irbis-theatre.ru", Password = "password123" },

            new Employer { Fio = "Аслан Георгиевич Царикаев", Gender = "male", BirthDate = new DateTime(1990, 3, 22),
                Position = "Кассир", Salary = 35000, Status = "работает",
                Contacts = "+7 (999) 100-00-21", Email = "aslan.tsarikhaev@irbis-theatre.ru", Password = "password123" },

            // МУЗЫКАНТЫ
            new Employer { Fio = "Алан Черменович Хабалов", Gender = "male", BirthDate = new DateTime(1980, 11, 8),
                Position = "Композитор", Salary = 100000, Status = "работает",
                Contacts = "+7 (999) 100-00-30", Email = "alan.khabalov@irbis-theatre.ru", Password = "password123" },

            new Employer { Fio = "Заурбек Тамерланович Кулумбегов", Gender = "male", BirthDate = new DateTime(1983, 5, 12),
                Position = "Музыкант (фортепиано)", Salary = 55000, Status = "работает",
                Contacts = "+7 (999) 100-00-31", Email = "zaur.kulumbegov@irbis-theatre.ru", Password = "password123" },

            new Employer { Fio = "Мадина Маратовна Газданова", Gender = "female", BirthDate = new DateTime(1987, 9, 25),
                Position = "Музыкант (скрипка)", Salary = 55000, Status = "работает",
                Contacts = "+7 (999) 100-00-32", Email = "madina.gazdanova@irbis-theatre.ru", Password = "password123" },

            // ХУДОЖНИКИ И ПОСТАНОВОЧНАЯ ЧАСТЬ
            new Employer { Fio = "Вадим Эльбрусович Дзагоев", Gender = "male", BirthDate = new DateTime(1982, 4, 17),
                Position = "Художник-постановщик", Salary = 80000, Status = "работает",
                Contacts = "+7 (999) 100-00-40", Email = "vadim.dzagoev@irbis-theatre.ru", Password = "password123" },

            new Employer { Fio = "Мадина Руслановна Каргинова", Gender = "female", BirthDate = new DateTime(1986, 7, 8),
                Position = "Художник по костюмам", Salary = 65000, Status = "работает",
                Contacts = "+7 (999) 100-00-41", Email = "madina.karginova@irbis-theatre.ru", Password = "password123" },

            new Employer { Fio = "Руслан Ахсарбекович Хадиков", Gender = "male", BirthDate = new DateTime(1985, 12, 20),
                Position = "Художник по свету", Salary = 70000, Status = "работает",
                Contacts = "+7 (999) 100-00-42", Email = "ruslan.khadikov@irbis-theatre.ru", Password = "password123" },

            new Employer { Fio = "Георгий Таймуразович Дзампов", Gender = "male", BirthDate = new DateTime(1988, 2, 14),
                Position = "Звукорежиссёр", Salary = 68000, Status = "работает",
                Contacts = "+7 (999) 100-00-43", Email = "georgy.dzampov@irbis-theatre.ru", Password = "password123" },

            // ТЕХНИЧЕСКИЙ ПЕРСОНАЛ
            new Employer { Fio = "Анзор Борисович Хугаев", Gender = "male", BirthDate = new DateTime(1978, 6, 10),
                Position = "Заведующий монтировочной частью", Salary = 60000, Status = "работает",
                Contacts = "+7 (999) 100-00-50", Email = "anzor.khugaev@irbis-theatre.ru", Password = "password123" },

            new Employer { Fio = "Руслан Казбекович Бурнацев", Gender = "male", BirthDate = new DateTime(1985, 9, 18),
                Position = "Монтировщик сцены", Salary = 45000, Status = "работает",
                Contacts = "+7 (999) 100-00-51", Email = "ruslan.burnatsev@irbis-theatre.ru", Password = "password123" },

            new Employer { Fio = "Таймураз Феликсович Сабанов", Gender = "male", BirthDate = new DateTime(1987, 11, 22),
                Position = "Осветитель", Salary = 42000, Status = "работает",
                Contacts = "+7 (999) 100-00-52", Email = "taimuraz.sabanov@irbis-theatre.ru", Password = "password123" },

            // АДМИНИСТРАТИВНО-ХОЗЯЙСТВЕННЫЙ ОТДЕЛ
            new Employer { Fio = "Фатима Георгиевна Демурова", Gender = "female", BirthDate = new DateTime(1975, 3, 25),
                Position = "Администратор", Salary = 55000, Status = "работает",
                Contacts = "+7 (999) 100-00-60", Email = "fatima.demurova@irbis-theatre.ru", Password = "password123" },

            new Employer { Fio = "Светлана Черменовна Цопаева", Gender = "female", BirthDate = new DateTime(1980, 10, 5),
                Position = "Менеджер по работе с посетителями", Salary = 48000, Status = "работает",
                Contacts = "+7 (999) 100-00-61", Email = "svetlana.tsopaeva@irbis-theatre.ru", Password = "password123" },

            new Employer { Fio = "Заур Ирбекович Хамицаев", Gender = "male", BirthDate = new DateTime(1982, 7, 14),
                Position = "Гардеробщик", Salary = 25000, Status = "работает",
                Contacts = "+7 (999) 100-00-70", Email = "zaur.khamitsaev@irbis-theatre.ru", Password = "password123" },

            new Employer { Fio = "Мадина Руслановна Бекоева", Gender = "female", BirthDate = new DateTime(1990, 4, 9),
                Position = "Билетёр", Salary = 22000, Status = "работает",
                Contacts = "+7 (999) 100-00-71", Email = "madina.bekoeva@irbis-theatre.ru", Password = "password123" },

            // УБОРЩИКИ И ОБСЛУЖИВАЮЩИЙ ПЕРСОНАЛ
            new Employer { Fio = "Залина Казбековна Техова", Gender = "female", BirthDate = new DateTime(1975, 12, 1),
                Position = "Уборщица (Большой зал)", Salary = 20000, Status = "работает",
                Contacts = "+7 (999) 100-00-80", Email = "zalina.tekhova@irbis-theatre.ru", Password = "password123" },

            new Employer { Fio = "Татьяна Георгиевна Качмазова", Gender = "female", BirthDate = new DateTime(1978, 6, 20),
                Position = "Уборщица (Малый зал)", Salary = 20000, Status = "работает",
                Contacts = "+7 (999) 100-00-81", Email = "tatiana.kachmazova@irbis-theatre.ru", Password = "password123" },

            new Employer { Fio = "Людмила Ахсарбековна Дряева", Gender = "female", BirthDate = new DateTime(1980, 3, 18),
                Position = "Уборщица (Камерный зал)", Salary = 20000, Status = "работает",
                Contacts = "+7 (999) 100-00-82", Email = "lyudmila.dryaeva@irbis-theatre.ru", Password = "password123" },

            new Employer { Fio = "Тамерлан Ирбекович Тменов", Gender = "male", BirthDate = new DateTime(1982, 8, 25),
                Position = "Дворник", Salary = 20000, Status = "работает",
                Contacts = "+7 (999) 100-00-83", Email = "tamerlan.tmenov@irbis-theatre.ru", Password = "password123" },

            new Employer { Fio = "Асланбек Сосланович Албегов", Gender = "male", BirthDate = new DateTime(1985, 11, 30),
                Position = "Разнорабочий", Salary = 22000, Status = "работает",
                Contacts = "+7 (999) 100-00-84", Email = "aslanbek.albegov@irbis-theatre.ru", Password = "password123" },

            // МАРКЕТИНГ И PR
            new Employer { Fio = "Ирина Сослановна Кабулова", Gender = "female", BirthDate = new DateTime(1986, 9, 28),
                Position = "PR-менеджер", Salary = 60000, Status = "работает",
                Contacts = "+7 (999) 100-00-90", Email = "irina.kabulova@irbis-theatre.ru", Password = "password123" },

            new Employer { Fio = "Асланбек Каурбекович Едзаев", Gender = "male", BirthDate = new DateTime(1988, 2, 11),
                Position = "SMM-менеджер", Salary = 50000, Status = "работает",
                Contacts = "+7 (999) 100-00-91", Email = "aslanbek.edzaev@irbis-theatre.ru", Password = "password123" }
        };
        _context.Employers.AddRange(employers);
        await _context.SaveChangesAsync();

        // ================================================================
        // 8. ПОСТАНОВОЧНАЯ ГРУППА (связь сотрудников со спектаклями)
        // ================================================================
        var productionTeams = new List<ProductionTeam>();

        // Получаем всех сотрудников по ролям
        var allEmployers = await _context.Employers.ToListAsync();
        var allRoles = await _context.Roles.ToListAsync();

        var directors = allEmployers.Where(e => e.Position.Contains("Режиссёр") || e.Position.Contains("режиссёр")).ToList();
        var composers = allEmployers.Where(e => e.Position.Contains("Композитор") || e.Position.Contains("композитор")).ToList();
        var artists = allEmployers.Where(e => e.Position.Contains("Артист") || e.Position.Contains("артист") || e.Position.Contains("Актер") || e.Position.Contains("актер")).ToList();
        var designers = allEmployers.Where(e => e.Position.Contains("Художник")).ToList();
        var soundEngineers = allEmployers.Where(e => e.Position.Contains("Звукорежиссёр")).ToList();

        foreach (var play in plays)
        {
            var rolesForPlay = allRoles.Where(r => r.PlayId == play.Id).ToList();

            // Назначаем режиссёра
            if (directors.Any())
            {
                productionTeams.Add(new ProductionTeam
                {
                    EmployerId = directors.First().Id,
                    RoleId = rolesForPlay.FirstOrDefault()?.Id ?? 1,
                    ParticipationType = "режиссёр-постановщик",
                    ProductionPosition = $"Режиссура спектакля {play.Title}"
                });
            }

            // Назначаем композитора
            if (composers.Any())
            {
                productionTeams.Add(new ProductionTeam
                {
                    EmployerId = composers.First().Id,
                    RoleId = rolesForPlay.FirstOrDefault()?.Id ?? 1,
                    ParticipationType = "композитор",
                    ProductionPosition = $"Музыкальное оформление {play.Title}"
                });
            }

            // Назначаем художника-постановщика
            if (designers.Any())
            {
                productionTeams.Add(new ProductionTeam
                {
                    EmployerId = designers.First().Id,
                    RoleId = rolesForPlay.FirstOrDefault()?.Id ?? 1,
                    ParticipationType = "художник-постановщик",
                    ProductionPosition = $"Сценография {play.Title}"
                });
            }

            // Назначаем артистов на роли
            for (int i = 0; i < rolesForPlay.Count && i < artists.Count; i++)
            {
                productionTeams.Add(new ProductionTeam
                {
                    EmployerId = artists[i % artists.Count].Id,
                    RoleId = rolesForPlay[i].Id,
                    ParticipationType = "актёр",
                    ProductionPosition = rolesForPlay[i].Name
                });
            }
        }

        _context.ProductionTeams.AddRange(productionTeams);
        await _context.SaveChangesAsync();

        // ================================================================
        // 9. ПОКАЗЫ СПЕКТАКЛЕЙ
        // ================================================================
        var performances = new List<Performance>();
        var random = new Random();
        var startDate = DateTime.Now.Date.AddDays(7);

        foreach (var play in plays)
        {
            for (int i = 0; i < 4; i++) // 4 показа на спектакль
            {
                var performanceDate = startDate.AddDays(random.Next(1, 60));
                performances.Add(new Performance
                {
                    Datetime = performanceDate.Date.AddHours(19),
                    BasePrice = random.Next(500, 2000),
                    IsPremiere = i == 0 && play.PremiereDate.HasValue && play.PremiereDate.Value.Year == DateTime.Now.Year,
                    Status = "запланирован",
                    PlayId = play.Id
                });
            }
        }

        _context.Performances.AddRange(performances);
        await _context.SaveChangesAsync();

        // ================================================================
        // 10. БИЛЕТЫ
        // ================================================================
        var tickets = new List<Ticket>();
        int ticketCounter = 1;
        var allSeats = await _context.Seats.Take(100).ToListAsync();

        foreach (var performance in performances)
        {
            foreach (var seat in allSeats)
            {
                string status;
                int priceMultiplier = seat.Category == "Партер-люкс" ? 3 : seat.Category == "Партер" ? 2 : 1;

                if (ticketCounter % 4 == 0)
                    status = "sold";
                else if (ticketCounter % 7 == 0)
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
        await _context.SaveChangesAsync();

        // ================================================================
        // 11. СВЯЗЬ ПОКАЗОВ И ПОСТАНОВОЧНОЙ ГРУППЫ
        // ================================================================
        var performanceGroups = new List<PerformanceGroup>();
        var allProductionTeams = await _context.ProductionTeams.ToListAsync();

        foreach (var performance in performances.Take(20))
        {
            var teamForPerformance = allProductionTeams.Take(5);
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
        await _context.SaveChangesAsync();

        Console.WriteLine($"✅ Тестовые данные успешно добавлены!");
        Console.WriteLine($"📚 Авторов: {authors.Count}");
        Console.WriteLine($"🎭 Жанров: {genres.Count}");
        Console.WriteLine($"🎪 Спектаклей: {plays.Count}");
        Console.WriteLine($"👥 Сотрудников: {employers.Count}");
        Console.WriteLine($"🎯 Ролей: {roles.Count}");
        Console.WriteLine($"📅 Показов: {performances.Count}");
        Console.WriteLine($"🎫 Билетов: {tickets.Count}");
    }
}