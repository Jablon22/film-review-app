using FilmReviewApp.Data;
using FilmReviewApp.Models;
using FilmReviewApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FilmReviewApp.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (_signInManager.IsSignedIn(User))
        {
            return RedirectToAction("Index", "Home");
        }
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, DataSeeder.UserRole);
            await _signInManager.SignInAsync(user, isPersistent: false);
            TempData["Success"] = "Konto zostało utworzone. Witaj!";
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        return View(model);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (_signInManager.IsSignedIn(User))
        {
            return RedirectToAction("Index", "Home");
        }
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Konto jest zablokowane. Skontaktuj się z administratorem.");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Nieprawidłowy e-mail lub hasło.");
        }
        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        TempData["Success"] = "Wylogowano pomyślnie.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var reviewCount = await _context.Reviews.CountAsync(r => r.UserId == user.Id);
        var watchlistCount = await _context.WatchlistItems.CountAsync(w => w.UserId == user.Id);

        var recent = await _context.Reviews
            .Include(r => r.Movie)
            .Where(r => r.UserId == user.Id)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .Select(r => new ProfileReviewItem
            {
                ReviewId = r.Id,
                MovieId = r.MovieId,
                MovieTitle = r.Movie!.Title,
                Rating = r.Rating,
                Content = r.Content,
                CreatedAt = r.CreatedAt,
                IsApproved = r.IsApproved
            })
            .ToListAsync();

        var model = new ProfileViewModel
        {
            Email = user.Email ?? "",
            Initials = MoviesController.GetInitials(user.Email),
            CreatedAt = user.CreatedAt,
            ReviewCount = reviewCount,
            WatchlistCount = watchlistCount,
            RecentReviews = recent
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();
}
