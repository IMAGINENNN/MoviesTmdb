namespace MoviesTmdb.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime ReleaseDate { get; set; } = null!;

        public ICollection<Director> Directors { get; set; } = null!;
        public ICollection<Genre> Genres { get; set; } = null!;
        public ICollection<MovieActor> MovieActors { get; set; } = null!;
    }

}
