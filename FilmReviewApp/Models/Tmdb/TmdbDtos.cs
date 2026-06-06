using System.Text.Json.Serialization;

namespace FilmReviewApp.Models.Tmdb;

public class TmdbSearchResponse
{
    [JsonPropertyName("results")]
    public List<TmdbMovieResult> Results { get; set; } = new();
}

public class TmdbMovieResult
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("release_date")]
    public string? ReleaseDate { get; set; }

    [JsonPropertyName("overview")]
    public string? Overview { get; set; }

    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }

    [JsonPropertyName("vote_average")]
    public double VoteAverage { get; set; }

    public int Year =>
        DateTime.TryParse(ReleaseDate, out var d) ? d.Year : 0;

    public string? FullPosterUrl =>
        string.IsNullOrEmpty(PosterPath) ? null : $"https://image.tmdb.org/t/p/w500{PosterPath}";
}

public class TmdbMovieDetail
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("release_date")]
    public string? ReleaseDate { get; set; }

    [JsonPropertyName("overview")]
    public string? Overview { get; set; }

    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }

    [JsonPropertyName("genres")]
    public List<TmdbGenre> Genres { get; set; } = new();

    [JsonPropertyName("credits")]
    public TmdbCredits? Credits { get; set; }

    public int Year =>
        DateTime.TryParse(ReleaseDate, out var d) ? d.Year : 0;

    public string? FullPosterUrl =>
        string.IsNullOrEmpty(PosterPath) ? null : $"https://image.tmdb.org/t/p/w500{PosterPath}";

    public string GenresCsv => string.Join(",", Genres.Select(g => g.Name));

    public string? Director =>
        Credits?.Crew?.FirstOrDefault(c => c.Job == "Director")?.Name;
}

public class TmdbGenre
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class TmdbCredits
{
    [JsonPropertyName("crew")]
    public List<TmdbCrewMember> Crew { get; set; } = new();
}

public class TmdbCrewMember
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("job")]
    public string Job { get; set; } = string.Empty;
}
