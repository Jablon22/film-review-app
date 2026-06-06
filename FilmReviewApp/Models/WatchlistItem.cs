using System.ComponentModel.DataAnnotations;

namespace FilmReviewApp.Models;

public class WatchlistItem
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public int MovieId { get; set; }
    public Movie? Movie { get; set; }

    [Display(Name = "Status")]
    public WatchStatus Status { get; set; } = WatchStatus.WantToWatch;

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
