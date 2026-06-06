using FilmReviewApp.Data;
using FilmReviewApp.Models;
using FilmReviewApp.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FilmReviewApp.Controllers;

public class MoviesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private const int PageSize = 12;

    public MoviesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, string? genre, int? year, int page = 1)
    {
        if (page < 1) page = 1;

        var query = _context.Movies.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(m => m.Title.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(genre))
        {
            query = query.Where(m => m.Genres != null && m.Genres.Contains(genre));
        }

        if (year.HasValue)
        {
            query = query.Where(m => m.Year == year.Value);
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

        var movies = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        var allGenres = await GetAllGenresAsync();
        var allYears = await _context.Movies
            .Select(m => m.Year)
            .Distinct()
            .OrderByDescending(y => y)
            .ToListAsync();

        await PopulateWatchlistStateAsync();

        var model = new MovieCatalogViewModel
        {
            Movies = movies,
            AllGenres = allGenres,
            AllYears = allYears,
            SelectedGenre = genre,
            SelectedYear = year,
            Search = search,
            CurrentPage = page,
            TotalPages = totalPages,
            PageSize = PageSize,
            TotalCount = totalCount
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Search(string? q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
        {
            return Json(Array.Empty<object>());
        }

        var term = q.Trim();
        var results = await _context.Movies
            .AsNoTracking()
            .Where(m => m.Title.Contains(term))
            .OrderByDescending(m => m.AverageRating)
            .Take(8)
            .Select(m => new
            {
                id = m.Id,
                title = m.Title,
                year = m.Year,
                posterUrl = m.PosterUrl,
                rating = m.AverageRating
            })
            .ToListAsync();

        return Json(results);
    }

    private async Task<List<string>> GetAllGenresAsync()
    {
        var rawGenres = await _context.Movies
            .Where(m => m.Genres != null && m.Genres != "")
            .Select(m => m.Genres!)
            .ToListAsync();

        return rawGenres
            .SelectMany(g => g.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Select(g => g.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g)
            .ToList();
    }

    private async Task PopulateWatchlistStateAsync()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            var userId = _userManager.GetUserId(User);
            var ids = await _context.WatchlistItems
                .Where(w => w.UserId == userId)
                .Select(w => w.MovieId)
                .ToListAsync();
            ViewData["InWatchlist"] = ids.ToHashSet();
        }
        else
        {
            ViewData["InWatchlist"] = new HashSet<int>();
        }
    }
}
