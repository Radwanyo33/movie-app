using Microsoft.AspNetCore.Routing.Constraints;

namespace Live_Movies.Models
{
    public class MovieGenre
    {
        public int MovieId { get; set; }
        public virtual Movie Movie { get; set; } = null!;
        public int GenreId { get; set; }
        public virtual Genre Genre { get; set; } = null!;
    }
}
