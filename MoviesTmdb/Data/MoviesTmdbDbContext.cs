using Microsoft.EntityFrameworkCore;
using MoviesTmdb.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MoviesTmdb.Data
{
    public class MoviesTmdbDbContext : DbContext
    {
        public MoviesTmdbDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<Director> Directors { get; set; }
        public DbSet<MovieGenre> MovieGenres { get; set; }
        public DbSet<MovieActor> MovieActors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MovieGenre>().HasKey(mg => new { mg.MovieId, mg.GenreId });
            modelBuilder.Entity<MovieActor>().HasKey(ma => new { ma.MovieId, ma.ActorId });
        }
    }
}
