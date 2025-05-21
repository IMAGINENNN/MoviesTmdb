namespace MoviesTmdb.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int TmdbId { get; set; }

        public ICollection<MovieGenre> MovieGenres { get; set; }
        public ICollection<MovieActor> MovieActors { get; set; }

        public int? DirectorId { get; set; }
        public Director Director { get; set; }
    }
}
