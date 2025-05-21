namespace MoviesTmdb.Models
{
    public class Genre
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<Movie> Movies { get; set; } = null!;
    }

}
