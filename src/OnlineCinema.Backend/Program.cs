using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OnlineCinema.Backend.Data;
using OnlineCinema.Backend.Repositories.Actors;
using OnlineCinema.Backend.Repositories.Admin;
using OnlineCinema.Backend.Repositories.Analytics;
using OnlineCinema.Backend.Repositories.Comments;
using OnlineCinema.Backend.Repositories.Genres;
using OnlineCinema.Backend.Repositories.Movies;
using OnlineCinema.Backend.Repositories.Playback;
using OnlineCinema.Backend.Repositories.Series;
using OnlineCinema.Backend.Repositories.Stats;
using OnlineCinema.Backend.Repositories.UnitOfWork;
using OnlineCinema.Backend.Repositories.UserActions;
using OnlineCinema.Backend.Repositories.Users;
using OnlineCinema.Backend.Services.Actors;
using OnlineCinema.Backend.Services.Analytics;
using OnlineCinema.Backend.Services.Admin;
using OnlineCinema.Backend.Services.Auth;
using OnlineCinema.Backend.Services.Comments;
using OnlineCinema.Backend.Services.Genres;
using OnlineCinema.Backend.Services.Media;
using OnlineCinema.Backend.Services.Movies;
using OnlineCinema.Backend.Services.Playback;
using OnlineCinema.Backend.Services.Series;
using OnlineCinema.Backend.Services.Stats;
using OnlineCinema.Backend.Services.Seeding;
using OnlineCinema.Backend.Services.Storage;
using OnlineCinema.Backend.Services.UsersActions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<ISeriesRepository, SeriesRepository>();
builder.Services.AddScoped<IActorRepository, ActorRepository>();
builder.Services.AddScoped<IGenreRepository, GenreRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserActionRepository, UserActionRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IPlaybackRepository, PlaybackRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IStatsRepository, StatsRepository>();
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IActorService, ActorService>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<ISeriesService, SeriesService>();
builder.Services.AddScoped<IUserActionService, UserActionService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IPlaybackService, PlaybackService>();
builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddScoped<IStatsService, StatsService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<SampleMediaImporter>();
builder.Services.AddSingleton<IStorageService, MinioStorageService>();
builder.Services.AddScoped<IPosterPreviewService, PosterPreviewService>();

// AutoMapper 16 выпилил DI-экстеншн из основного пакета, регистрирую вручную
builder.Services.AddSingleton<IMapper>(sp =>
    new Mapper(new MapperConfiguration(
        cfg => cfg.AddMaps(typeof(Program).Assembly),
        sp.GetRequiredService<ILoggerFactory>())));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OnlineCinema API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введите JWT токен в формате: Bearer {токен}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


var allowedOrigin = builder.Configuration["FRONTEND_URL"] ?? "http://localhost:5173";

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

AddJwtBearerAuthentication(builder);

var app = builder.Build();

MigrateDatabase(app);

// SEED_DEMO_DATA=true заполняет базу тестовым контентом для демо/ручной проверки
if (app.Configuration["SEED_DEMO_DATA"] == "true")
{
    using (var scope = app.Services.CreateScope())
    {
        await new DemoSeeder(
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(),
            scope.ServiceProvider.GetRequiredService<IStorageService>()).SeedAsync();
    }
}

// The checked-in coursework artwork and locally mounted sample films are imported
// idempotently after seeding. This also upgrades an already populated demo database.
if (app.Configuration["IMPORT_SAMPLE_MEDIA"] == "true")
{
    try
    {
        using var scope = app.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<SampleMediaImporter>().ImportAsync();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Sample media import failed; the application will continue without it");
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static void MigrateDatabase(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        try
        {
            dbContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

static void AddJwtBearerAuthentication(WebApplicationBuilder builder)
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "Bearer";
        options.DefaultChallengeScheme = "Bearer";
    })
    .AddJwtBearer("Bearer", options =>
    {
        var jwtSecret = builder.Configuration["JwtSettings:Secret"];
        if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
            throw new InvalidOperationException("JwtSettings:Secret is required and must contain at least 32 characters.");

        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            ValidateLifetime = true
        };
    });
}