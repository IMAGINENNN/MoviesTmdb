namespace MoviesTmdb.Models
{
    public class Director
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<Movie> Movies { get; set; } = null!;
    }

}
