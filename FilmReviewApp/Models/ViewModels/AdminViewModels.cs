using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FilmReviewApp.Models.ViewModels;

public class AdminDashboardViewModel
{
    public int MovieCount { get; set; }
    public int PendingReviewCount { get; set; }
    public int UserCount { get; set; }
    public int WatchlistCount { get; set; }
    public List<PendingReviewItem> RecentPending { get; set; } = new();
}

public class PendingReviewItem
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AdminMovieFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tytuł jest wymagany.")]
    [StringLength(200)]
    [Display(Name = "Tytuł")]
    public string Title { get; set; } = string.Empty;

    [Range(1888, 2100, ErrorMessage = "Podaj poprawny rok.")]
    [Display(Name = "Rok")]
    public int Year { get; set; } = DateTime.UtcNow.Year;

    [StringLength(100)]
    [Display(Name = "Reżyser")]
    public string? Director { get; set; }

    [StringLength(2000)]
    [Display(Name = "Opis")]
    public string? Description { get; set; }

    [StringLength(300)]
    [Display(Name = "Gatunki (oddzielone przecinkami)")]
    public string? Genres { get; set; }

    [Display(Name = "Plakat (URL)")]
    public string? PosterUrl { get; set; }

    public int? TmdbId { get; set; }

    [Display(Name = "Lub prześlij własny plakat")]
    public IFormFile? PosterFile { get; set; }
}

public class SiteContentViewModel
{
    [Display(Name = "Tytuł serwisu")]
    [StringLength(100)]
    public string SiteTitle { get; set; } = string.Empty;

    [Display(Name = "Opis w sekcji hero (strona główna)")]
    [StringLength(2000)]
    public string HeroDescription { get; set; } = string.Empty;

    [Display(Name = "Tekst stopki")]
    [StringLength(2000)]
    public string FooterText { get; set; } = string.Empty;
}

public class AdminUserItem
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsLockedOut { get; set; }
    public int ReviewCount { get; set; }
}
