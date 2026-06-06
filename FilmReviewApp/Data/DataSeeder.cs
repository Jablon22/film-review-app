using FilmReviewApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FilmReviewApp.Data;

public static class DataSeeder
{
    public const string AdminRole = "Admin";
    public const string UserRole = "User";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var context = sp.GetRequiredService<ApplicationDbContext>();
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

        await context.Database.MigrateAsync();

        await SeedRolesAsync(roleManager);
        var admin = await SeedAdminAsync(userManager);
        await SeedSiteSettingsAsync(context);
        await SeedMoviesAndReviewsAsync(context, admin);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in new[] { AdminRole, UserRole })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task<ApplicationUser> SeedAdminAsync(UserManager<ApplicationUser> userManager)
    {
        const string email = "admin@filmapp.pl";
        var admin = await userManager.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            await userManager.CreateAsync(admin, "Admin123!");
        }

        if (!await userManager.IsInRoleAsync(admin, AdminRole))
        {
            await userManager.AddToRoleAsync(admin, AdminRole);
        }

        return admin;
    }

    private static async Task SeedSiteSettingsAsync(ApplicationDbContext context)
    {
        var defaults = new Dictionary<string, (string value, string description)>
        {
            ["SiteTitle"] = ("FilmReview", "Tytuł serwisu wyświetlany w nagłówku"),
            ["HeroDescription"] = ("Odkrywaj, oceniaj i recenzuj swoje ulubione filmy.", "Tekst opisu w sekcji hero na stronie głównej"),
            ["FooterText"] = ("FilmReview — projekt zaliczeniowy Technologie Internetowe.", "Tekst wyświetlany w stopce")
        };

        foreach (var (key, (value, description)) in defaults)
        {
            if (!await context.SiteSettings.AnyAsync(s => s.Key == key))
            {
                context.SiteSettings.Add(new SiteSetting { Key = key, Value = value, Description = description });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedMoviesAndReviewsAsync(ApplicationDbContext context, ApplicationUser admin)
    {
        if (await context.Movies.AnyAsync())
        {
            return;
        }

        var movies = new List<Movie>
        {
            new()
            {
                Title = "Skazani na Shawshank",
                Year = 1994,
                Director = "Frank Darabont",
                Description = "Niesłusznie skazany bankier Andy Dufresne trafia do więzienia Shawshank, gdzie zaprzyjaźnia się z Redem i przez lata nie traci nadziei na odzyskanie wolności.",
                PosterUrl = "https://image.tmdb.org/t/p/w500/q6y0Go1tsGEsmtFryDOJo3dEmqu.jpg",
                Genres = "Dramat",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new()
            {
                Title = "Ojciec chrzestny",
                Year = 1972,
                Director = "Francis Ford Coppola",
                Description = "Saga rodziny Corleone — historia starzejącego się patriarcha mafijnego imperium, który przekazuje stery swojemu niechętnemu synowi.",
                PosterUrl = "https://image.tmdb.org/t/p/w500/3bhkrj58Vtu7enYsRolD1fZdja1.jpg",
                Genres = "Dramat,Kryminał",
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            },
            new()
            {
                Title = "Incepcja",
                Year = 2010,
                Director = "Christopher Nolan",
                Description = "Dom Cobb potrafi kraść sekrety z podświadomości podczas snu. Otrzymuje szansę na odkupienie, jeśli dokona rzeczy odwrotnej — zaszczepi ideę w cudzym umyśle.",
                PosterUrl = "https://image.tmdb.org/t/p/w500/9gk7adHYeDvHkCSEqAvQNLV5Uge.jpg",
                Genres = "Akcja,Sci-Fi,Thriller",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };

        context.Movies.AddRange(movies);
        await context.SaveChangesAsync();

        var reviews = new List<Review>
        {
            new()
            {
                MovieId = movies[0].Id,
                UserId = admin.Id,
                Rating = 10,
                Content = "Ponadczasowy dramat o nadziei i przyjaźni. Jeden z najlepszych filmów w historii kina.",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                IsApproved = true
            },
            new()
            {
                MovieId = movies[2].Id,
                UserId = admin.Id,
                Rating = 9,
                Content = "Wizjonerskie kino Nolana. Wielowarstwowa fabuła, która trzyma w napięciu do ostatniej sceny.",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                IsApproved = true
            }
        };

        context.Reviews.AddRange(reviews);
        await context.SaveChangesAsync();

        await RecalculateAverages(context, movies.Select(m => m.Id));
    }

    private static async Task RecalculateAverages(ApplicationDbContext context, IEnumerable<int> movieIds)
    {
        foreach (var movieId in movieIds)
        {
            var movie = await context.Movies.FindAsync(movieId);
            if (movie is null) continue;

            var approved = await context.Reviews
                .Where(r => r.MovieId == movieId && r.IsApproved)
                .ToListAsync();

            movie.AverageRating = approved.Count > 0 ? Math.Round(approved.Average(r => r.Rating), 1) : 0;
        }

        await context.SaveChangesAsync();
    }
}
