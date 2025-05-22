using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesTmdb.Data;
using MoviesTmdb.Services;
using System.Linq;
using System.Threading.Tasks;

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
            var movies = await _tmdb.GetPopularMoviesAsync(20); // Fetch data from API
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAll()
        {
            _db.Movies.RemoveRange(_db.Movies);
            _db.MovieGenres.RemoveRange(_db.MovieGenres);
            _db.MovieActors.RemoveRange(_db.MovieActors);
            _db.Directors.RemoveRange(_db.Directors);
            _db.Actors.RemoveRange(_db.Actors);
            _db.Genres.RemoveRange(_db.Genres);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult AnalyzeDirectorsByFilms()
        {
            var topDirectors = _db.Directors
                .Include(d => d.Movies)
                .OrderByDescending(d => d.Movies.Count)
                .Take(5)
                .Select(d => new { Director = d.Name, Films = d.Movies.Count })
                .ToList();

            return Json(topDirectors);
        }

        [HttpGet]
        public IActionResult AnalyzeDirectorsByWatches()
        {
            var topDirectors = _db.Directors
                .Include(d => d.Movies)
                .OrderByDescending(d => d.Movies.Sum(m => m.Popularity))
                .Take(5)
                .Select(d => new { Director = d.Name, Watches = d.Movies.Sum(m => m.Popularity) })
                .ToList();

            return Json(topDirectors);
        }

        [HttpGet]
        public IActionResult AnalyzeActors()
        {
            var topActors = _db.Actors
                .Include(a => a.MovieActors)
                .OrderByDescending(a => a.MovieActors.Count)
                .Take(5)
                .Select(a => new { Actor = a.Name, Appearances = a.MovieActors.Count })
                .ToList();

            return Json(topActors);
        }

        [HttpGet]
        public IActionResult AnalyzeGenresByDecade()
        {
            var genreByDecade = _db.Movies
                .Where(m => m.ReleaseDate.HasValue)
                .GroupBy(m => m.ReleaseDate.Value.Year / 10 * 10)
                .Select(g => new
                {
                    Decade = g.Key,
                    TopGenres = g.SelectMany(m => m.MovieGenres)
                                .GroupBy(mg => mg.Genre.Name)
                                .OrderByDescending(grp => grp.Count())
                                .Take(3)
                                .Select(grp => new { Name = grp.Key, Count = grp.Count() })
                })
                .OrderBy(g => g.Decade)
                .ToList();

            return Json(genreByDecade);
        }

        public IActionResult Analysis()
        {
            var topDirectors = _db.Directors
                .OrderByDescending(d => d.Movies.Count)
                .Take(5)
                .ToList();
            var topActors = _db.Actors
                .OrderByDescending(a => a.MovieActors.Count)
                .Take(5)
                .ToList();
            var genreByDecade = _db.Movies
                .Where(m => m.ReleaseDate.HasValue)
                .GroupBy(m => m.ReleaseDate.Value.Year / 10 * 10)
                .Select(g => new
                {
                    Decade = g.Key,
                    TopGenre = g.SelectMany(m => m.MovieGenres)
                                .GroupBy(mg => mg.Genre.Name)
                                .OrderByDescending(grp => grp.Count())
                                .Select(grp => grp.Key)
                                .FirstOrDefault()
                })
                .OrderBy(g => g.Decade)
                .ToList();

            ViewBag.TopDirectors = topDirectors;
            ViewBag.TopActors = topActors;
            ViewBag.GenreByDecade = genreByDecade;
            return View();
        }
    }
}