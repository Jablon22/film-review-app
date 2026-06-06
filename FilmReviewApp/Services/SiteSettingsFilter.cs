using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FilmReviewApp.Services;

public class SiteSettingsFilter : IAsyncActionFilter
{
    private readonly ISiteSettingsService _settings;

    public SiteSettingsFilter(ISiteSettingsService settings)
    {
        _settings = settings;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.Controller is Controller controller)
        {
            controller.ViewData["SiteTitle"] = await _settings.GetAsync("SiteTitle", "FilmReview");
            controller.ViewData["HeroDescription"] = await _settings.GetAsync("HeroDescription", "Odkrywaj, oceniaj i recenzuj filmy.");
            controller.ViewData["FooterText"] = await _settings.GetAsync("FooterText", "FilmReview");
        }

        await next();
    }
}
