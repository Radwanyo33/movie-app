using System.ComponentModel.DataAnnotations;

namespace Live_Movies.Models
{
    public class CastMember
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public virtual ICollection<MovieCast> MovieCasts { get; set; } = new List<MovieCast>();
    }
}
