using Microsoft.EntityFrameworkCore;
using OnlineCinema.Backend.Data;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Services.Helpers;

namespace OnlineCinema.Backend.Services;

// Генерит демо-данные для ручного тестирования и защиты диплома.
// Запускается если SEED_DEMO_DATA=true после миграций.
// Только развилка данных которых ещё нет - повторный запуск ничего не дублирует.
public class DemoSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly IStorageService _storage;

    public DemoSeeder(ApplicationDbContext db, IStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task SeedAsync()
    {
        if (await _db.Movies.AnyAsync() || await _db.Series.AnyAsync())
        {
            Console.WriteLine("[seed] база не пустая, пропускаю");
            return;
        }

        Console.WriteLine("[seed] заполняю жанрами...");
        var genres = new List<Genre>
        {
            new() { Name = "Боевик" },
            new() { Name = "Комедия" },
            new() { Name = "Драма" },
            new() { Name = "Фантастика" },
            new() { Name = "Ужасы" },
            new() { Name = "Мелодрама" },
            new() { Name = "Триллер" },
            new() { Name = "Фэнтези" },
        };
        _db.Genres.AddRange(genres);
        await _db.SaveChangesAsync();

        Console.WriteLine("[seed] актёры...");
        var actors = new List<Actor>
        {
            new() { FirstName = "Киану", LastName = "Ривз", BirthDate = new DateOnly(1964, 9, 2), Biography = "Канадский актёр, известен по трилогии 'Матрица' и 'Джон Уик'." },
            new() { FirstName = "Роберт", LastName = "Дауни-мл.", BirthDate = new DateOnly(1965, 4, 4) },
            new() { FirstName = "Скарлетт", LastName = "Йоханссон", BirthDate = new DateOnly(1984, 11, 22) },
            new() { FirstName = "Том", LastName = "Хэнкс", BirthDate = new DateOnly(1956, 7, 9) },
            new() { FirstName = "Леонардо", LastName = "ДиКаприо", BirthDate = new DateOnly(1974, 11, 11) },
            new() { FirstName = "Марго", LastName = "Робби", BirthDate = new DateOnly(1990, 7, 2) },
            new() { FirstName = "Мэттью", LastName = "Макконахи", BirthDate = new DateOnly(1969, 11, 4) },
            new() { FirstName = "Джессика", LastName = "Честейн", BirthDate = new DateOnly(1977, 3, 24) },
            new() { FirstName = "Тимоти", LastName = "Шаламе", BirthDate = new DateOnly(1995, 12, 27) },
            new() { FirstName = "Расс", LastName = "Кроу", BirthDate = new DateOnly(1964, 4, 7) },
            new() { FirstName = "Галь", LastName = "Гадот", BirthDate = new DateOnly(1985, 4, 30) },
            new() { FirstName = "Джон", LastName = "Траволта", BirthDate = new DateOnly(1954, 2, 18) },
        };
        _db.Actors.AddRange(actors);

        var comedy = genres[1];
        var drama = genres[2];
        var scifi = genres[3];

        Console.WriteLine("[seed] фильмы...");
        var movies = new List<Movie>
        {
            new() { Title = "Джон Уик", ReleaseYear = 2014, Duration = 101, Description = "Бывший наёмный убийца выходит на тропу войны из-за убитой собаки.", Genres = new List<Genre> { genres[0], genres[6] }, Actors = new List<Actor> { actors[0] } },
            new() { Title = "Матрица", ReleaseYear = 1999, Duration = 136, Description = "Хакер Нео узнаёт, что весь мир — симуляция.", Genres = new List<Genre> { scifi, genres[0] }, Actors = new List<Actor> { actors[0] } },
            new() { Title = "Мстители: Финал", ReleaseYear = 2019, Duration = 181, Description = "Финальная битва против Таноса.", Genres = new List<Genre> { scifi, genres[0] }, Actors = new List<Actor> { actors[1], actors[2] } },
            new() { Title = "Форрест Гамп", ReleaseYear = 1994, Duration = 142, Description = "Жизнь простодушного добряка на фоне истории США.", Genres = new List<Genre> { drama, comedy }, Actors = new List<Actor> { actors[3] } },
            new() { Title = "Интерстеллар", ReleaseYear = 2014, Duration = 169, Description = "Группа астронавтов ищет новый дом для человечества.", Genres = new List<Genre> { scifi, drama }, Actors = new List<Actor> { actors[6], actors[7] } },
            new() { Title = "Начало", ReleaseYear = 2010, Duration = 148, Description = "Вор проникает в сны, чтобы красть секреты.", Genres = new List<Genre> { scifi, genres[6] }, Actors = new List<Actor> { actors[4] } },
            new() { Title = "Волк с Уолл-стрит", ReleaseYear = 2013, Duration = 180, Description = "Взлёт и падение брокера Джордана Белфорта.", Genres = new List<Genre> { drama, comedy, genres[2] }, Actors = new List<Actor> { actors[4] } },
            new() { Title = "Отступники", ReleaseYear = 2006, Duration = 151, Description = "Крот в полиции и полицейский в мафии.", Genres = new List<Genre> { genres[6], drama }, Actors = new List<Actor> { actors[4] } },
            new() { Title = "Гладиатор", ReleaseYear = 2000, Duration = 155, Description = "Римский полководец становится гладиатором.", Genres = new List<Genre> { drama, genres[0] }, Actors = new List<Actor> { actors[9] } },
            new() { Title = "Дюна", ReleaseYear = 2021, Duration = 155, Description = "Молодой герцог Пол Атрейдес втянут в войну за специю.", Genres = new List<Genre> { scifi }, Actors = new List<Actor> { actors[8], actors[1] } },
            new() { Title = "Дюна: Часть 2", ReleaseYear = 2024, Duration = 166, Description = "Пол объединяет фрименов против Харконненов.", Genres = new List<Genre> { scifi }, Actors = new List<Actor> { actors[8], actors[2] } },
        };

        // добираем до 30 фильмов шаблонными названиями с привязкой к жанру
        var extraTitles = new[] { "Тень прошлого", "Разлом", "Полярная звезда", "Последний рубеж", "Шёпот ветра", "Хроники бури", "Город огней", "Чёрный горизонт", "Мёртвый сезон", "За гранью", "Эхо войны", "Невидимая нить", "Красный туман", "Бегущий по лезвию 2049", "Легенда о драконе", "Тихий океан", "Полночный экспресс", "Стеклянный дом", "Игра теней" };
        for (int i = 0; i < extraTitles.Length; i++)
        {
            movies.Add(new Movie
            {
                Title = extraTitles[i],
                ReleaseYear = 2005 + (i * 13 % 20),
                Duration = 90 + (i * 7 % 90),
                Description = $"Демо-фильм #{i + 1}. Сюжетную линию добавим позже.",
                Genres = new List<Genre> { genres[i % genres.Count] },
                Actors = new List<Actor> { actors[i % actors.Count] }
            });
        }

        _db.Movies.AddRange(movies);

        Console.WriteLine("[seed] добавляю пользователей, оценки и обсуждение...");
        var demoUsers = new List<User>();
        foreach (var (username, email, firstName) in new[]
        {
            ("demo_alex", "alex.demo@onlinecinema.local", "Алексей"),
            ("demo_maria", "maria.demo@onlinecinema.local", "Мария"),
            ("demo_nikita", "nikita.demo@onlinecinema.local", "Никита")
        })
        {
            var salt = PasswordHasher.GenerateSalt();
            demoUsers.Add(new User
            {
                Username = username,
                Email = email,
                FirstName = firstName,
                PasswordSalt = salt,
                PasswordHash = PasswordHasher.HashPassword("DemoPassword123!", salt)
            });
        }
        _db.Users.AddRange(demoUsers);

        Console.WriteLine("[seed] сериалы...");
        var seriesList = new List<(string Title, int Year, Genre g1, Genre g2, int Seasons, int EpsPerSeason)>
        {
            ("Во все тяжкие", 2008, drama, genres[6], 5, 9),
            ("Игра престолов", 2011, genres[7], drama, 8, 8),
            ("Очень странные дела", 2016, scifi, genres[0], 4, 8),
            ("Друзья", 1994, comedy, drama, 10, 8),
            ("Чернобыль", 2019, drama, genres[7], 1, 5),
            ("Мандалорец", 2019, scifi, genres[0], 3, 8),
            ("Шерлок", 2010, drama, genres[6], 4, 3),
            ("Чёрное зеркало", 2011, scifi, genres[6], 6, 4),
            ("Аркейн", 2021, genres[7], drama, 2, 9),
        };
        foreach (var (title, year, g1, g2, sCnt, eps) in seriesList)
        {
            var s = new Series { Title = title, ReleaseYear = year, Description = $"Сериал {title}.", Genres = new List<Genre> { g1, g2 } };
            for (int seasonNum = 1; seasonNum <= sCnt; seasonNum++)
            {
                var season = new Season { SeasonNumber = seasonNum, ReleaseYear = year + seasonNum - 1 };
                for (int ep = 1; ep <= eps; ep++)
                {
                    season.Episodes.Add(new Episode
                    {
                        EpisodeNumber = ep,
                        Title = $"Эпизод {ep}",
                        Description = $"Серия {ep} сезона {seasonNum}.",
                        Duration = 40 + (ep % 12)
                    });
                }
                s.Seasons.Add(season);
            }
            _db.Series.Add(s);
        }

        await _db.SaveChangesAsync();

        // Небольшая, но живая выборка: статистика и отзывы видны сразу после первого старта.
        _db.Ratings.AddRange(
            new Rating { UserId = demoUsers[0].Id, MovieId = movies[0].Id, RatingValue = 9 },
            new Rating { UserId = demoUsers[0].Id, MovieId = movies[1].Id, RatingValue = 10 },
            new Rating { UserId = demoUsers[1].Id, MovieId = movies[1].Id, RatingValue = 9 },
            new Rating { UserId = demoUsers[1].Id, MovieId = movies[4].Id, RatingValue = 10 },
            new Rating { UserId = demoUsers[2].Id, MovieId = movies[5].Id, RatingValue = 8 },
            new Rating { UserId = demoUsers[2].Id, SeriesId = (await _db.Series.OrderBy(x => x.Id).FirstAsync()).Id, RatingValue = 9 });

        var rootComment = new Comment
        {
            UserId = demoUsers[0].Id,
            MovieId = movies[1].Id,
            Text = "Пересмотрел перед защитой — до сих пор отлично работает."
        };
        _db.Comments.Add(rootComment);
        await _db.SaveChangesAsync();
        _db.Comments.Add(new Comment
        {
            UserId = demoUsers[1].Id,
            MovieId = movies[1].Id,
            ParentId = rootComment.Id,
            Text = "Согласна, особенно сцена выбора таблетки."
        });

        await SeedDemoMediaAsync(movies, await _db.Series.OrderBy(x => x.Id).ToListAsync());
        await _db.SaveChangesAsync();
        Console.WriteLine("[seed] готово");
    }

    private async Task SeedDemoMediaAsync(IReadOnlyList<Movie> movies, IReadOnlyList<Series> series)
    {
        // SVG is tiny and deterministic, so the first Docker start needs no Internet/TMDB key.
        foreach (var movie in movies.Take(12))
        {
            var key = $"posters/demo/movie-{movie.Id}.svg";
            await using var content = BuildPoster(movie.Title, "ФИЛЬМ", movie.ReleaseYear);
            movie.PosterUrl = await _storage.SaveAsync(content, key, "image/svg+xml");
        }

        foreach (var show in series)
        {
            var key = $"posters/demo/series-{show.Id}.svg";
            await using var content = BuildPoster(show.Title, "СЕРИАЛ", show.ReleaseYear);
            show.PosterUrl = await _storage.SaveAsync(content, key, "image/svg+xml");
        }
    }

    private static MemoryStream BuildPoster(string title, string label, int? year)
    {
        static string Escape(string value) => System.Security.SecurityElement.Escape(value) ?? value;
        var svg = $"""
            <svg xmlns="http://www.w3.org/2000/svg" width="600" height="900" viewBox="0 0 600 900">
              <defs><linearGradient id="g" x1="0" y1="0" x2="1" y2="1"><stop stop-color="#111827"/><stop offset="1" stop-color="#7c3aed"/></linearGradient></defs>
              <rect width="600" height="900" fill="url(#g)"/>
              <circle cx="470" cy="170" r="150" fill="#ec4899" opacity=".25"/>
              <text x="48" y="690" fill="#a78bfa" font-family="Arial" font-size="28" font-weight="700">{label} · {year}</text>
              <text x="48" y="760" fill="white" font-family="Arial" font-size="42" font-weight="700">{Escape(title)}</text>
              <text x="48" y="825" fill="#d1d5db" font-family="Arial" font-size="22">OnlineCinema demo collection</text>
            </svg>
            """;
        return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(svg));
    }
}
