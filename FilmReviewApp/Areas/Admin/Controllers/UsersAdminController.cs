using FilmReviewApp.Data;
using FilmReviewApp.Models;
using FilmReviewApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FilmReviewApp.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
[Route("Admin/Users")]
public class UsersAdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public UsersAdminController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "users";

        var users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
        var reviewCounts = await _context.Reviews
            .GroupBy(r => r.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);

        var items = new List<AdminUserItem>();
        foreach (var u in users)
        {
            items.Add(new AdminUserItem
            {
                Id = u.Id,
                Email = u.Email ?? "",
                CreatedAt = u.CreatedAt,
                IsAdmin = await _userManager.IsInRoleAsync(u, DataSeeder.AdminRole),
                IsLockedOut = u.LockoutEnd is not null && u.LockoutEnd > DateTimeOffset.UtcNow,
                ReviewCount = reviewCounts.TryGetValue(u.Id, out var c) ? c : 0
            });
        }

        return View(items);
    }

    [HttpPost("ToggleRole/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleRole(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["Error"] = "Nie możesz zmienić własnej roli.";
            return RedirectToAction(nameof(Index));
        }

        if (await _userManager.IsInRoleAsync(user, DataSeeder.AdminRole))
        {
            await _userManager.RemoveFromRoleAsync(user, DataSeeder.AdminRole);
            await _userManager.AddToRoleAsync(user, DataSeeder.UserRole);
            TempData["Success"] = $"Użytkownik {user.Email} jest teraz zwykłym użytkownikiem.";
        }
        else
        {
            await _userManager.AddToRoleAsync(user, DataSeeder.AdminRole);
            TempData["Success"] = $"Użytkownik {user.Email} ma teraz rolę administratora.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("ToggleLock/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLock(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["Error"] = "Nie możesz zablokować własnego konta.";
            return RedirectToAction(nameof(Index));
        }

        var isLocked = user.LockoutEnd is not null && user.LockoutEnd > DateTimeOffset.UtcNow;
        if (isLocked)
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
            TempData["Success"] = $"Konto {user.Email} zostało odblokowane.";
        }
        else
        {
            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
            TempData["Success"] = $"Konto {user.Email} zostało zablokowane.";
        }

        return RedirectToAction(nameof(Index));
    }
}
