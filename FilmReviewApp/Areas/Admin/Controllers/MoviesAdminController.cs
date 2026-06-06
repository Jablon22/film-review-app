using FilmReviewApp.Data;
using FilmReviewApp.Models;
using FilmReviewApp.Models.ViewModels;
using FilmReviewApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FilmReviewApp.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
[Route("Admin/Movies")]
public class MoviesAdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ITmdbService _tmdb;
    private readonly IWebHostEnvironment _env;

    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    public MoviesAdminController(ApplicationDbContext context, ITmdbService tmdb, IWebHostEnvironment env)
    {
        _context = context;
        _tmdb = tmdb;
        _env = env;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? search)
    {
        ViewData["ActiveMenu"] = "movies";
        var query = _context.Movies.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(m => m.Title.Contains(search));
        }
        ViewData["Search"] = search;
        var movies = await query.OrderByDescending(m => m.CreatedAt).ToListAsync();
        return View(movies);
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        ViewData["ActiveMenu"] = "movies";
        return View(new AdminMovieFormViewModel());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminMovieFormViewModel model)
    {
        ViewData["ActiveMenu"] = "movies";
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var posterUrl = model.PosterUrl;
        if (model.PosterFile is not null)
        {
            var saved = await SavePosterAsync(model.PosterFile);
            if (saved is null)
            {
                ModelState.AddModelError(nameof(model.PosterFile), "Nieprawidłowy plik. Dozwolone: jpg, jpeg, png, webp.");
                return View(model);
            }
            posterUrl = saved;
        }

        _context.Movies.Add(new Movie
        {
            Title = model.Title,
            Year = model.Year,
            Director = model.Director,
            Description = model.Description,
            Genres = model.Genres,
            PosterUrl = posterUrl,
            TmdbId = model.TmdbId,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        TempData["Success"] = "Film został dodany.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["ActiveMenu"] = "movies";
        var movie = await _context.Movies.FindAsync(id);
        if (movie is null)
        {
            return NotFound();
        }

        return View(new AdminMovieFormViewModel
        {
            Id = movie.Id,
            Title = movie.Title,
            Year = movie.Year,
            Director = movie.Director,
            Description = movie.Description,
            Genres = movie.Genres,
            PosterUrl = movie.PosterUrl,
            TmdbId = movie.TmdbId
        });
    }

    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminMovieFormViewModel model)
    {
        ViewData["ActiveMenu"] = "movies";
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var movie = await _context.Movies.FindAsync(id);
        if (movie is null)
        {
            return NotFound();
        }

        if (model.PosterFile is not null)
        {
            var saved = await SavePosterAsync(model.PosterFile);
            if (saved is null)
            {
                ModelState.AddModelError(nameof(model.PosterFile), "Nieprawidłowy plik. Dozwolone: jpg, jpeg, png, webp.");
                return View(model);
            }
            movie.PosterUrl = saved;
        }
        else
        {
            movie.PosterUrl = model.PosterUrl;
        }

        movie.Title = model.Title;
        movie.Year = model.Year;
        movie.Director = model.Director;
        movie.Description = model.Description;
        movie.Genres = model.Genres;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Film został zaktualizowany.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var movie = await _context.Movies.FindAsync(id);
        if (movie is null)
        {
            return NotFound();
        }

        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Usunięto film „{movie.Title}”.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("TmdbSearch")]
    public async Task<IActionResult> TmdbSearch(string? q)
    {
        var results = await _tmdb.SearchAsync(q ?? string.Empty);
        return Json(results.Select(r => new
        {
            id = r.Id,
            title = r.Title,
            year = r.Year,
            overview = r.Overview,
            posterUrl = r.FullPosterUrl
        }));
    }

    [HttpPost("ImportTmdb")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportTmdb(int tmdbId)
    {
        var existing = await _context.Movies.AnyAsync(m => m.TmdbId == tmdbId);
        if (existing)
        {
            return Json(new { success = false, message = "Film już istnieje w bazie." });
        }

        var detail = await _tmdb.GetDetailAsync(tmdbId);
        if (detail is null)
        {
            return Json(new { success = false, message = "Nie znaleziono filmu w TMDB." });
        }

        var movie = _tmdb.MapToMovie(detail);
        _context.Movies.Add(movie);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Zaimportowano.", id = movie.Id });
    }

    private async Task<string?> SavePosterAsync(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(extension) || file.Length == 0 || file.Length > 5 * 1024 * 1024)
        {
            return null;
        }

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);

        // Sanitized, server-generated file name to prevent path traversal.
        var safeName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploadsDir, safeName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/{safeName}";
    }
}
