using MoviesTmdb.Models;
using System.Text.Json;

namespace MoviesTmdb.Services
{
    public class TmdbService
    {
        private readonly HttpClient _client;
        private const string ApiKey = "53e1dc66f9856493c65dcde5137ad7bb";

        public TmdbService(HttpClient client)
        {
            _client = client;
            _client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
        }

        public async Task<List<Movie>> GetPopularMoviesAsync()
        {
            var response = await _client.GetFromJsonAsync<JsonElement>($"movie/popular?api_key={ApiKey}");
            var results = response.GetProperty("results");

            var movies = new List<Movie>();
            foreach (var item in results.EnumerateArray())
            {
                movies.Add(new Movie
                {
                    TmdbId = item.GetProperty("id").GetInt32(),
                    Title = item.GetProperty("title").GetString(),
                    ReleaseDate = item.TryGetProperty("release_date", out var date) &&
                        DateTime.TryParse(date.GetString(), out var parsedDate) ? parsedDate : null
                });
            }
            return movies;
        }
    }
}
