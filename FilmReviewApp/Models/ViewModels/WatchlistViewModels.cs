namespace FilmReviewApp.Models.ViewModels;

public class WatchlistViewModel
{
    public List<WatchlistEntry> WantToWatch { get; set; } = new();
    public List<WatchlistEntry> Watched { get; set; } = new();
}

public class WatchlistEntry
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public int Year { get; set; }
    public double AverageRating { get; set; }
    public WatchStatus Status { get; set; }
    public DateTime AddedAt { get; set; }
}
