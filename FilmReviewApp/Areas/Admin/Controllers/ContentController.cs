using FilmReviewApp.Models.ViewModels;
using FilmReviewApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FilmReviewApp.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ContentController : Controller
{
    private readonly ISiteSettingsService _settings;

    public ContentController(ISiteSettingsService settings)
    {
        _settings = settings;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "content";

        var model = new SiteContentViewModel
        {
            SiteTitle = await _settings.GetAsync("SiteTitle", "FilmReview"),
            HeroDescription = await _settings.GetAsync("HeroDescription", ""),
            FooterText = await _settings.GetAsync("FooterText", "")
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SiteContentViewModel model)
    {
        ViewData["ActiveMenu"] = "content";
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _settings.SetAsync("SiteTitle", model.SiteTitle);
        await _settings.SetAsync("HeroDescription", model.HeroDescription);
        await _settings.SetAsync("FooterText", model.FooterText);

        TempData["Success"] = "Treść strony została zaktualizowana.";
        return RedirectToAction(nameof(Index));
    }
}
