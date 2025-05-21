namespace MoviesTmdb.Models
{
    public class Actor
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<MovieActor> MovieActors { get; set; } = null!;
    }

}
