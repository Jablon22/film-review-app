using Microsoft.Playwright;

// Ensure the Chromium browser is downloaded before use.
var exit = Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });
if (exit != 0)
{
    Console.WriteLine($"Playwright install failed: {exit}");
    return exit;
}

const string baseUrl = "http://localhost:5081";
var outDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "docs", "manual-images");
outDir = Path.GetFullPath(outDir);
Directory.CreateDirectory(outDir);
Console.WriteLine($"Saving screenshots to: {outDir}");

using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
var context = await browser.NewContextAsync(new() { ViewportSize = new() { Width = 1366, Height = 900 } });
var page = await context.NewPageAsync();

async Task Shot(string name, string url, bool fullPage = true)
{
    await page.GotoAsync(baseUrl + url, new() { WaitUntil = WaitUntilState.NetworkIdle });
    await page.WaitForTimeoutAsync(700);
    await page.ScreenshotAsync(new() { Path = Path.Combine(outDir, name), FullPage = fullPage });
    Console.WriteLine($"  saved {name}  ({url})");
}

// ---- Public pages ----
await Shot("01-home.png", "/");
await Shot("02-catalog.png", "/Movies");
await Shot("03-movie-details.png", "/Movies/Details/1");
await Shot("04-register.png", "/Account/Register");
await Shot("05-login.png", "/Account/Login");

// ---- Log in as admin ----
await page.GotoAsync(baseUrl + "/Account/Login", new() { WaitUntil = WaitUntilState.NetworkIdle });
await page.FillAsync("#Email", "admin@filmapp.pl");
await page.FillAsync("#Password", "Admin123!");
await page.ClickAsync("button[type=submit]");
await page.WaitForURLAsync($"{baseUrl}/", new() { WaitUntil = WaitUntilState.NetworkIdle });
Console.WriteLine("  logged in as admin");

// ---- Authenticated user pages ----
await Shot("06-profile.png", "/Account/Profile");
await Shot("07-watchlist.png", "/Watchlist");

// ---- Admin panel ----
await Shot("08-admin-dashboard.png", "/Admin");
await Shot("09-admin-movies.png", "/Admin/Movies");
await Shot("10-admin-movie-create.png", "/Admin/Movies/Create");
await Shot("11-admin-reviews.png", "/Admin/Reviews?showAll=true");
await Shot("12-admin-users.png", "/Admin/Users");
await Shot("13-admin-content.png", "/Admin/Content");

// ---- TMDB import demo: type a query and trigger search ----
await page.GotoAsync(baseUrl + "/Admin/Movies/Create", new() { WaitUntil = WaitUntilState.NetworkIdle });
await page.FillAsync("#tmdbQuery", "Matrix");
await page.ClickAsync("#tmdbSearchBtn");
try { await page.WaitForSelectorAsync(".js-tmdb-import", new() { Timeout = 8000 }); } catch { /* ignore */ }
await page.WaitForTimeoutAsync(500);
await page.ScreenshotAsync(new() { Path = Path.Combine(outDir, "14-admin-tmdb-search.png"), FullPage = true });
Console.WriteLine("  saved 14-admin-tmdb-search.png (TMDB search demo)");

Console.WriteLine("Done.");
return 0;
