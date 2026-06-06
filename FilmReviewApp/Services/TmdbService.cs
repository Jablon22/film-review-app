using FilmReviewApp.Models;
using FilmReviewApp.Models.Tmdb;

namespace FilmReviewApp.Services;

public interface ITmdbService
{
    Task<List<TmdbMovieResult>> SearchAsync(string query);
    Task<TmdbMovieDetail?> GetDetailAsync(int tmdbId);
    Movie MapToMovie(TmdbMovieDetail detail);
}

public class TmdbService : ITmdbService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly ILogger<TmdbService> _logger;

    public TmdbService(HttpClient http, IConfiguration config, ILogger<TmdbService> logger)
    {
        _http = http;
        _logger = logger;
        _apiKey = config["TmdbApiKey"] ?? string.Empty;

        if (_http.BaseAddress is null)
        {
            _http.BaseAddress = new Uri("https://api.themoviedb.org/3/");
        }
    }

    public async Task<List<TmdbMovieResult>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || string.IsNullOrWhiteSpace(_apiKey))
        {
            return new List<TmdbMovieResult>();
        }

        try
        {
            var url = $"search/movie?api_key={_apiKey}&language=pl-PL&query={Uri.EscapeDataString(query)}";
            var response = await _http.GetFromJsonAsync<TmdbSearchResponse>(url);
            return response?.Results ?? new List<TmdbMovieResult>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd wyszukiwania TMDB dla zapytania {Query}", query);
            return new List<TmdbMovieResult>();
        }
    }

    public async Task<TmdbMovieDetail?> GetDetailAsync(int tmdbId)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            return null;
        }

        try
        {
            var url = $"movie/{tmdbId}?api_key={_apiKey}&language=pl-PL&append_to_response=credits";
            return await _http.GetFromJsonAsync<TmdbMovieDetail>(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd pobierania szczegółów TMDB dla id {TmdbId}", tmdbId);
            return null;
        }
    }

    public Movie MapToMovie(TmdbMovieDetail detail)
    {
        return new Movie
        {
            Title = detail.Title,
            Year = detail.Year,
            Director = detail.Director,
            Description = detail.Overview,
            PosterUrl = detail.FullPosterUrl,
            Genres = detail.GenresCsv,
            TmdbId = detail.Id,
            CreatedAt = DateTime.UtcNow
        };
    }
}
