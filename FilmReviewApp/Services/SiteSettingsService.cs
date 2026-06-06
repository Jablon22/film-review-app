using FilmReviewApp.Data;
using Microsoft.EntityFrameworkCore;

namespace FilmReviewApp.Services;

public interface ISiteSettingsService
{
    Task<Dictionary<string, string?>> GetAllAsync();
    Task<string> GetAsync(string key, string fallback = "");
    Task SetAsync(string key, string? value);
    void Invalidate();
}

public class SiteSettingsService : ISiteSettingsService
{
    private readonly ApplicationDbContext _context;
    private static Dictionary<string, string?>? _cache;
    private static readonly object Lock = new();

    public SiteSettingsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Dictionary<string, string?>> GetAllAsync()
    {
        if (_cache is not null)
        {
            return _cache;
        }

        var data = await _context.SiteSettings
            .AsNoTracking()
            .ToDictionaryAsync(s => s.Key, s => s.Value);

        lock (Lock)
        {
            _cache = data;
        }

        return data;
    }

    public async Task<string> GetAsync(string key, string fallback = "")
    {
        var all = await GetAllAsync();
        return all.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value!
            : fallback;
    }

    public async Task SetAsync(string key, string? value)
    {
        var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting is null)
        {
            _context.SiteSettings.Add(new Models.SiteSetting { Key = key, Value = value });
        }
        else
        {
            setting.Value = value;
        }

        await _context.SaveChangesAsync();
        Invalidate();
    }

    public void Invalidate()
    {
        lock (Lock)
        {
            _cache = null;
        }
    }
}
