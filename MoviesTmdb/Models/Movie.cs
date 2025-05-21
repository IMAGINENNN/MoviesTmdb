namespace MoviesTmdb.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public DateTime? ReleaseDate { get; set; }
        public int TmdbId { get; set; }

        public ICollection<MovieGenre> MovieGenres { get; set; } = null!;
        public ICollection<MovieActor> MovieActors { get; set; } = null!;

        public int? DirectorId { get; set; }
        public Director Director { get; set; } = null!;
    }
}
