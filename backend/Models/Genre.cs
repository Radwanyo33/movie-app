using System.ComponentModel.DataAnnotations;

namespace Live_Movies.Models
{
    public class Genre
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        public virtual ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
    }
}
