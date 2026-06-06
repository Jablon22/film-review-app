namespace FilmReviewApp.Models.ViewModels;

public class HomeIndexViewModel
{
    public Movie? FeaturedMovie { get; set; }
    public List<Movie> TopRated { get; set; } = new();
    public List<Movie> RecentlyAdded { get; set; } = new();
    public int MovieCount { get; set; }
    public int ReviewCount { get; set; }
    public int UserCount { get; set; }
    public string HeroDescription { get; set; } = string.Empty;
}
