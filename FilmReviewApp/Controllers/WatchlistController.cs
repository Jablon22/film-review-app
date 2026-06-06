using FilmReviewApp.Data;
using FilmReviewApp.Models;
using FilmReviewApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FilmReviewApp.Controllers;

[Authorize]
public class WatchlistController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public WatchlistController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        var entries = await _context.WatchlistItems
            .AsNoTracking()
            .Include(w => w.Movie)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.AddedAt)
            .Select(w => new WatchlistEntry
            {
                Id = w.Id,
                MovieId = w.MovieId,
                MovieTitle = w.Movie!.Title,
                PosterUrl = w.Movie.PosterUrl,
                Year = w.Movie.Year,
                AverageRating = w.Movie.AverageRating,
                Status = w.Status,
                AddedAt = w.AddedAt
            })
            .ToListAsync();

        var model = new WatchlistViewModel
        {
            WantToWatch = entries.Where(e => e.Status == WatchStatus.WantToWatch).ToList(),
            Watched = entries.Where(e => e.Status == WatchStatus.Watched).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int movieId)
    {
        var movie = await _context.Movies.FindAsync(movieId);
        if (movie is null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User)!;
        var exists = await _context.WatchlistItems.AnyAsync(w => w.MovieId == movieId && w.UserId == userId);
        if (!exists)
        {
            _context.WatchlistItems.Add(new WatchlistItem
            {
                MovieId = movieId,
                UserId = userId,
                Status = WatchStatus.WantToWatch,
                AddedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Dodano „{movie.Title}” do watchlisty.";
        }
        else
        {
            TempData["Error"] = "Ten film jest już na Twojej watchliście.";
        }

        var returnUrl = Request.Form["returnUrl"].ToString();
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Details", "Movies", new { id = movieId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, WatchStatus status)
    {
        var userId = _userManager.GetUserId(User);
        var item = await _context.WatchlistItems.FirstOrDefaultAsync(w => w.Id == id);
        if (item is null)
        {
            return NotFound();
        }
        if (item.UserId != userId)
        {
            return Forbid();
        }

        item.Status = status;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Zaktualizowano status filmu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id)
    {
        var userId = _userManager.GetUserId(User);
        var item = await _context.WatchlistItems.FirstOrDefaultAsync(w => w.Id == id);
        if (item is null)
        {
            return NotFound();
        }
        if (item.UserId != userId)
        {
            return Forbid();
        }

        _context.WatchlistItems.Remove(item);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Usunięto film z watchlisty.";
        return RedirectToAction(nameof(Index));
    }
}
