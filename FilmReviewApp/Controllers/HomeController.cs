using System.Diagnostics;
using FilmReviewApp.Data;
using FilmReviewApp.Models;
using FilmReviewApp.Models.ViewModels;
using FilmReviewApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FilmReviewApp.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ISiteSettingsService _settings;

    public HomeController(ApplicationDbContext context, ISiteSettingsService settings)
    {
        _context = context;
        _settings = settings;
    }

    public async Task<IActionResult> Index()
    {
        var movieCount = await _context.Movies.CountAsync();

        Movie? featured = null;
        if (movieCount > 0)
        {
            var skip = Random.Shared.Next(movieCount);
            featured = await _context.Movies.OrderBy(m => m.Id).Skip(skip).FirstOrDefaultAsync();
        }

        var model = new HomeIndexViewModel
        {
            FeaturedMovie = featured,
            TopRated = await _context.Movies
                .Where(m => m.AverageRating > 0)
                .OrderByDescending(m => m.AverageRating)
                .ThenByDescending(m => m.CreatedAt)
                .Take(6)
                .ToListAsync(),
            RecentlyAdded = await _context.Movies
                .OrderByDescending(m => m.CreatedAt)
                .Take(6)
                .ToListAsync(),
            MovieCount = movieCount,
            ReviewCount = await _context.Reviews.CountAsync(r => r.IsApproved),
            UserCount = await _context.Users.CountAsync(),
            HeroDescription = await _settings.GetAsync("HeroDescription", "Odkrywaj, oceniaj i recenzuj filmy.")
        };

        return View(model);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
