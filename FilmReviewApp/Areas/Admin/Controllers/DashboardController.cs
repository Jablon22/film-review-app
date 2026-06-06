using FilmReviewApp.Data;
using FilmReviewApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FilmReviewApp.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "dashboard";

        var model = new AdminDashboardViewModel
        {
            MovieCount = await _context.Movies.CountAsync(),
            PendingReviewCount = await _context.Reviews.CountAsync(r => !r.IsApproved),
            UserCount = await _context.Users.CountAsync(),
            WatchlistCount = await _context.WatchlistItems.CountAsync(),
            RecentPending = await _context.Reviews
                .Include(r => r.Movie)
                .Include(r => r.User)
                .Where(r => !r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .Select(r => new PendingReviewItem
                {
                    Id = r.Id,
                    MovieId = r.MovieId,
                    MovieTitle = r.Movie!.Title,
                    UserName = r.User!.Email ?? "Użytkownik",
                    Rating = r.Rating,
                    Content = r.Content,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync()
        };

        return View(model);
    }
}
