using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesTmdb.Data;
using MoviesTmdb.Services;

namespace MoviesTmdb.Controllers
{
    public class MoviesController : Controller
    {
        private readonly MoviesTmdbDbContext _db;
        private readonly TmdbService _tmdb;

        public MoviesController(MoviesTmdbDbContext db, TmdbService tmdb)
        {
            _db = db;
            _tmdb = tmdb;
        }

        public IActionResult Index()
        {
            var movies = _db.Movies
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .Include(m => m.MovieActors).ThenInclude(ma => ma.Actor)
                .Include(m => m.Director)
                .ToList();
            return View(movies);
        }

        [HttpPost]
        public async Task<IActionResult> FetchFromTmdb()
        {
            var movies = await _tmdb.GetPopularMoviesAsync();
            _db.Movies.AddRange(movies);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAll()
        {
            _db.Movies.RemoveRange(_db.Movies);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public IActionResult Analysis()
        {
            var topDirectors = _db.Directors.OrderByDescending(d => d.Movies.Count).Take(5).ToList();
            var topActors = _db.Actors.OrderByDescending(a => a.MovieActors.Count).Take(5).ToList();

            var genreByDecade = _db.Movies
                .Where(m => m.ReleaseDate.HasValue)
                .GroupBy(m => m.ReleaseDate.Value.Year / 10 * 10)
                .Select(g => new {
                    Decade = g.Key,
                    TopGenre = g.SelectMany(m => m.MovieGenres)
                                .GroupBy(mg => mg.Genre.Name)
                                .OrderByDescending(grp => grp.Count())
                                .Select(grp => grp.Key)
                                .FirstOrDefault()
                }).ToList();

            ViewBag.TopDirectors = topDirectors;
            ViewBag.TopActors = topActors;
            ViewBag.GenreByDecade = genreByDecade;
            return View();
        }
    }
}
