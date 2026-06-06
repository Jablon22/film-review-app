using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmReviewApp.Models;

public class Review
{
    public int Id { get; set; }

    [Display(Name = "Film")]
    public int MovieId { get; set; }
    public Movie? Movie { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    [Range(1, 10, ErrorMessage = "Ocena musi być w zakresie 1-10.")]
    [Display(Name = "Ocena")]
    public int Rating { get; set; }

    [Required(ErrorMessage = "Treść recenzji jest wymagana.")]
    [StringLength(3000)]
    [Display(Name = "Treść recenzji")]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Zatwierdzona")]
    public bool IsApproved { get; set; } = false;
}
