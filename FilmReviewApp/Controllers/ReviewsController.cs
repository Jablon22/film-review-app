using FilmReviewApp.Data;
using FilmReviewApp.Models;
using FilmReviewApp.Models.ViewModels;
using FilmReviewApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FilmReviewApp.Controllers;

[Authorize]
public class ReviewsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRatingService _ratingService;

    public ReviewsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IRatingService ratingService)
    {
        _context = context;
        _userManager = userManager;
        _ratingService = ratingService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NewReviewViewModel model)
    {
        var movie = await _context.Movies.FindAsync(model.MovieId);
        if (movie is null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User)!;

        var alreadyReviewed = await _context.Reviews
            .AnyAsync(r => r.MovieId == model.MovieId && r.UserId == userId);
        if (alreadyReviewed)
        {
            TempData["Error"] = "Możesz dodać tylko jedną recenzję do tego filmu.";
            return RedirectToAction("Details", "Movies", new { id = model.MovieId });
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Recenzja zawiera błędy. Sprawdź ocenę i treść.";
            return RedirectToAction("Details", "Movies", new { id = model.MovieId });
        }

        _context.Reviews.Add(new Review
        {
            MovieId = model.MovieId,
            UserId = userId,
            Rating = model.Rating,
            Content = model.Content,
            CreatedAt = DateTime.UtcNow,
            IsApproved = false
        });
        await _context.SaveChangesAsync();

        TempData["Success"] = "Recenzja została dodana i oczekuje na zatwierdzenie przez administratora.";
        return RedirectToAction("Details", "Movies", new { id = model.MovieId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = _userManager.GetUserId(User);
        var review = await _context.Reviews
            .Include(r => r.Movie)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review is null)
        {
            return NotFound();
        }
        if (review.UserId != userId)
        {
            return Forbid();
        }

        return View(new EditReviewViewModel
        {
            Id = review.Id,
            MovieId = review.MovieId,
            MovieTitle = review.Movie?.Title ?? "",
            Rating = review.Rating,
            Content = review.Content
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditReviewViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = _userManager.GetUserId(User);
        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == model.Id);
        if (review is null)
        {
            return NotFound();
        }
        if (review.UserId != userId)
        {
            return Forbid();
        }

        review.Rating = model.Rating;
        review.Content = model.Content;
        review.IsApproved = false; // edited reviews require re-approval
        review.CreatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await _ratingService.RecalculateAsync(review.MovieId);

        TempData["Success"] = "Recenzja została zaktualizowana i ponownie oczekuje na zatwierdzenie.";
        return RedirectToAction("Details", "Movies", new { id = review.MovieId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _userManager.GetUserId(User);
        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id);
        if (review is null)
        {
            return NotFound();
        }
        if (review.UserId != userId)
        {
            return Forbid();
        }

        var movieId = review.MovieId;
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        await _ratingService.RecalculateAsync(movieId);

        TempData["Success"] = "Recenzja została usunięta.";
        var returnUrl = Request.Form["returnUrl"].ToString();
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Details", "Movies", new { id = movieId });
    }
}
