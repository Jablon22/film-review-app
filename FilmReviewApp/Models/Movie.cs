using System.ComponentModel.DataAnnotations;

namespace FilmReviewApp.Models;

public class Movie
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tytuł jest wymagany.")]
    [StringLength(200)]
    [Display(Name = "Tytuł")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Rok")]
    [Range(1888, 2100, ErrorMessage = "Podaj poprawny rok produkcji.")]
    public int Year { get; set; }

    [StringLength(100)]
    [Display(Name = "Reżyser")]
    public string? Director { get; set; }

    [StringLength(2000)]
    [Display(Name = "Opis")]
    public string? Description { get; set; }

    [Display(Name = "Plakat (URL)")]
    public string? PosterUrl { get; set; }

    [StringLength(300)]
    [Display(Name = "Gatunki")]
    public string? Genres { get; set; }

    [Display(Name = "TMDB Id")]
    public int? TmdbId { get; set; }

    [Display(Name = "Średnia ocena")]
    public double AverageRating { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<WatchlistItem> WatchlistItems { get; set; } = new List<WatchlistItem>();

    public IEnumerable<string> GenreList =>
        string.IsNullOrWhiteSpace(Genres)
            ? Enumerable.Empty<string>()
            : Genres.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
