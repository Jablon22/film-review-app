using FilmReviewApp.Data;
using Microsoft.EntityFrameworkCore;

namespace FilmReviewApp.Services;

public interface IRatingService
{
    Task RecalculateAsync(int movieId);
}

public class RatingService : IRatingService
{
    private readonly ApplicationDbContext _context;

    public RatingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task RecalculateAsync(int movieId)
    {
        var movie = await _context.Movies.FindAsync(movieId);
        if (movie is null)
        {
            return;
        }

        var ratings = await _context.Reviews
            .Where(r => r.MovieId == movieId && r.IsApproved)
            .Select(r => r.Rating)
            .ToListAsync();

        movie.AverageRating = ratings.Count > 0 ? Math.Round(ratings.Average(), 1) : 0;
        await _context.SaveChangesAsync();
    }
}
