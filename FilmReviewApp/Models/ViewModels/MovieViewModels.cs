using System.ComponentModel.DataAnnotations;

namespace FilmReviewApp.Models.ViewModels;

public class MovieCatalogViewModel
{
    public List<Movie> Movies { get; set; } = new();
    public List<string> AllGenres { get; set; } = new();
    public List<int> AllYears { get; set; } = new();

    public string? SelectedGenre { get; set; }
    public int? SelectedYear { get; set; }
    public string? Search { get; set; }

    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int PageSize { get; set; } = 12;
    public int TotalCount { get; set; }
}

public class MovieDetailViewModel
{
    public Movie Movie { get; set; } = null!;
    public List<ReviewDisplayItem> Reviews { get; set; } = new();
    public int ReviewCount { get; set; }
    public bool CanReview { get; set; }
    public bool AlreadyReviewed { get; set; }
    public bool InWatchlist { get; set; }
    public NewReviewViewModel NewReview { get; set; } = new();
}

public class ReviewDisplayItem
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Initials { get; set; } = "??";
    public int Rating { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsOwn { get; set; }
}

public class NewReviewViewModel
{
    public int MovieId { get; set; }

    [Range(1, 10, ErrorMessage = "Ocena musi być w zakresie 1-10.")]
    [Display(Name = "Ocena (1-10)")]
    public int Rating { get; set; } = 8;

    [Required(ErrorMessage = "Treść recenzji jest wymagana.")]
    [StringLength(3000, MinimumLength = 3, ErrorMessage = "Recenzja musi mieć od 3 do 3000 znaków.")]
    [Display(Name = "Twoja recenzja")]
    public string Content { get; set; } = string.Empty;
}

public class EditReviewViewModel
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;

    [Range(1, 10, ErrorMessage = "Ocena musi być w zakresie 1-10.")]
    [Display(Name = "Ocena (1-10)")]
    public int Rating { get; set; }

    [Required(ErrorMessage = "Treść recenzji jest wymagana.")]
    [StringLength(3000, MinimumLength = 3)]
    [Display(Name = "Treść recenzji")]
    public string Content { get; set; } = string.Empty;
}
