using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Live_Movies.Models
{
    public class Movie
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(4)]
        [RegularExpression(@"^(19|20)\d{2}$", ErrorMessage = "Release Year Must Be Valid")]
        public string Release_Year { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Language { get; set; } = string.Empty;

        [Required]
        public string Rating { get; set; } = string.Empty;
        public string Description { get; set; } = String.Empty;

        [Required]
        [Url]
        public string Image_url { get; set; } = string.Empty;

        [Required]
        [Url]
        public string Watch_url { get; set; } = string.Empty;

        // Navigation properties for related entities
        public virtual ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
        public virtual ICollection<MovieCast> MovieCasts { get; set; } = new List<MovieCast>();

        // NEW: Store genre and cast as JSON for seeded data
        public string GenreJson { get; set; } = "[]";

        public string CastJson { get; set; } = "[]";

        // UPDATED: Computed property that works for both cases
        [NotMapped]
        public List<string> Genre
        {
            get
            {
                // If we have MovieGenres loaded (new movies), use those
                if (MovieGenres != null && MovieGenres.Any())
                {
                    return MovieGenres.Select(mg => mg.Genre.Name).ToList();
                }
                // Otherwise, use the JSON data (seeded movies)
                else if (!string.IsNullOrEmpty(GenreJson))
                {
                    try
                    {
                        return JsonSerializer.Deserialize<List<string>>(GenreJson) ?? new List<string>();
                    }
                    catch
                    {
                        return new List<string>();
                    }
                }
                return new List<string>();
            }
        }

        [NotMapped]
        public List<string> Cast
        {
            get
            {
                // If we have MovieCasts loaded (new movies), use those
                if (MovieCasts != null && MovieCasts.Any())
                {
                    return MovieCasts.Select(mc => mc.CastMember.Name).ToList();
                }
                // Otherwise, use the JSON data (seeded movies)
                else if (!string.IsNullOrEmpty(CastJson))
                {
                    try
                    {
                        return JsonSerializer.Deserialize<List<string>>(CastJson) ?? new List<string>();
                    }
                    catch
                    {
                        return new List<string>();
                    }
                }
                return new List<string>();
            }
        }
    }
}