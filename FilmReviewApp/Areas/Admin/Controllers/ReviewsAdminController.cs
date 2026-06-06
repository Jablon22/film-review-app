using FilmReviewApp.Data;
using FilmReviewApp.Models.ViewModels;
using FilmReviewApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FilmReviewApp.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
[Route("Admin/Reviews")]
public class ReviewsAdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IRatingService _ratingService;

    public ReviewsAdminController(ApplicationDbContext context, IRatingService ratingService)
    {
        _context = context;
        _ratingService = ratingService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(bool showAll = false)
    {
        ViewData["ActiveMenu"] = "reviews";
        ViewData["ShowAll"] = showAll;

        var query = _context.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .AsQueryable();

        if (!showAll)
        {
            query = query.Where(r => !r.IsApproved);
        }

        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
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
            .ToListAsync();

        return View(reviews);
    }

    [HttpPost("Approve/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id);
        if (review is null)
        {
            return NotFound();
        }

        review.IsApproved = true;
        await _context.SaveChangesAsync();
        await _ratingService.RecalculateAsync(review.MovieId);

        TempData["Success"] = "Recenzja została zatwierdzona.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Reject/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id);
        if (review is null)
        {
            return NotFound();
        }

        var movieId = review.MovieId;
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        await _ratingService.RecalculateAsync(movieId);

        TempData["Success"] = "Recenzja została odrzucona i usunięta.";
        return RedirectToAction(nameof(Index));
    }
}
