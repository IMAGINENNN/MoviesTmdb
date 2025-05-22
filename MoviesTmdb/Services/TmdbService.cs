using Microsoft.EntityFrameworkCore;
using MoviesTmdb.Data;
using MoviesTmdb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MoviesTmdb.Services
{
    public class TmdbService
    {
        private readonly HttpClient _client;
        private readonly MoviesTmdbDbContext _dbContext;
        private readonly string _apiKey;

        public TmdbService(HttpClient client, MoviesTmdbDbContext dbContext, IConfiguration configuration)
        {
            _client = client;
            _client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
            _dbContext = dbContext;
            _apiKey = configuration["TMDB:ApiKey"] ?? throw new ArgumentNullException("TMDB:ApiKey is missing in configuration.");
        }

        public async Task<List<Movie>> GetPopularMoviesAsync(int totalMovies = 100)
        {
            var movies = new List<Movie>();
            int moviesPerPage = 20;
            int totalPages = (int)Math.Ceiling((double)totalMovies / moviesPerPage);

            // Fetch genres
            var genres = await GetGenresAsync();
            var genreMap = genres.ToDictionary(g => g.Id, g => g.Name);

            for (int page = 1; page <= totalPages && movies.Count < totalMovies; page++)
            {
                var response = await _client.GetAsync($"movie/popular?api_key={_apiKey}&page={page}");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var movieResponse = JsonSerializer.Deserialize<JsonElement>(content);
                var results = movieResponse.GetProperty("results");

                foreach (var item in results.EnumerateArray())
                {
                    if (movies.Count >= totalMovies) break;

                    int tmdbId = item.GetProperty("id").GetInt32();

                    // Check if movie already exists in the database
                    var existingMovie = await _dbContext.Movies
                        .Include(m => m.MovieGenres)
                        .Include(m => m.MovieActors)
                        .Include(m => m.Director)
                        .FirstOrDefaultAsync(m => m.TmdbId == tmdbId);

                    if (existingMovie != null)
                    {
                        movies.Add(existingMovie);
                        continue;
                    }

                    var movie = new Movie
                    {
                        TmdbId = tmdbId,
                        Title = item.GetProperty("title").GetString(),
                        ReleaseDate = item.TryGetProperty("release_date", out var date) &&
                            DateTime.TryParse(date.GetString(), out var parsedDate) ? parsedDate : null,
                        Popularity = item.GetProperty("popularity").GetDouble(),
                        MovieGenres = new List<MovieGenre>(),
                        MovieActors = new List<MovieActor>()
                    };

                    // Fetch credits
                    var credits = await GetMovieCreditsAsync(tmdbId);

                    // Director
                    var directorName = credits.Crew.FirstOrDefault(c => c.Job == "Director")?.Name;
                    if (!string.IsNullOrEmpty(directorName))
                    {
                        var director = await _dbContext.Directors
                            .FirstOrDefaultAsync(d => d.Name == directorName);
                        if (director == null)
                        {
                            director = new Director { Name = directorName, Movies = new List<Movie>() };
                            _dbContext.Directors.Add(director);
                        }
                        movie.Director = director;
                    }

                    // Genres
                    if (item.TryGetProperty("genre_ids", out var genreIds))
                    {
                        foreach (var genreId in genreIds.EnumerateArray())
                        {
                            int id = genreId.GetInt32();
                            if (genreMap.ContainsKey(id))
                            {
                                var genre = await _dbContext.Genres
                                    .FirstOrDefaultAsync(g => g.Name == genreMap[id])
                                    ?? new Genre { Name = genreMap[id], MovieGenres = new List<MovieGenre>() };
                                if (!_dbContext.Genres.Local.Any(g => g.Name == genre.Name))
                                    _dbContext.Genres.Add(genre);
                                movie.MovieGenres.Add(new MovieGenre { Genre = genre });
                            }
                        }
                    }

                    // Actors (limit to top 5 to avoid overloading)
                    foreach (var cast in credits.Cast.Take(5))
                    {
                        var actor = await _dbContext.Actors
                            .FirstOrDefaultAsync(a => a.Name == cast.Name)
                            ?? new Actor { Name = cast.Name, MovieActors = new List<MovieActor>() };
                        if (!_dbContext.Actors.Local.Any(a => a.Name == actor.Name))
                            _dbContext.Actors.Add(actor);
                        movie.MovieActors.Add(new MovieActor { Actor = actor });
                    }

                    _dbContext.Movies.Add(movie);
                    movies.Add(movie);
                }

                await _dbContext.SaveChangesAsync();
                // Add delay to respect TMDb rate limits (50 requests/second)
                await Task.Delay(100);
            }

            return movies;
        }

        private async Task<List<ApiGenre>> GetGenresAsync()
        {
            var existingGenres = await _dbContext.Genres.ToListAsync();
            if (existingGenres.Any())
                return existingGenres.Select(g => new ApiGenre { Id = g.Id, Name = g.Name }).ToList();

            var response = await _client.GetAsync($"genre/movie/list?api_key={_apiKey}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var genreResponse = JsonSerializer.Deserialize<JsonElement>(content);
            var genres = genreResponse.GetProperty("genres").EnumerateArray()
                .Select(g => new ApiGenre
                {
                    Id = g.GetProperty("id").GetInt32(),
                    Name = g.GetProperty("name").GetString()
                })
                .ToList();

            foreach (var genre in genres)
            {
                if (!_dbContext.Genres.Any(g => g.Name == genre.Name))
                    _dbContext.Genres.Add(new Genre { Name = genre.Name });
            }
            await _dbContext.SaveChangesAsync();
            return genres;
        }

        private async Task<Credits> GetMovieCreditsAsync(int movieId)
        {
            var response = await _client.GetAsync($"movie/{movieId}/credits?api_key={_apiKey}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var credits = JsonSerializer.Deserialize<Credits>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return credits ?? new Credits { Cast = new List<Cast>(), Crew = new List<Crew>() };
        }

        private class ApiGenre
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class Credits
        {
            public List<Cast> Cast { get; set; }
            public List<Crew> Crew { get; set; }
        }

        private class Cast
        {
            public string Name { get; set; }
        }

        private class Crew
        {
            public string Name { get; set; }
            public string Job { get; set; }
        }
    }
}