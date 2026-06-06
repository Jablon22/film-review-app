using System.ComponentModel.DataAnnotations;

namespace FilmReviewApp.Models;

public class SiteSetting
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Key { get; set; } = string.Empty;

    [StringLength(2000)]
    [Display(Name = "Wartość")]
    public string? Value { get; set; }

    [StringLength(200)]
    [Display(Name = "Opis")]
    public string? Description { get; set; }
}
