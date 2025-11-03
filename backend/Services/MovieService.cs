using Live_Movies.Data;
using Live_Movies.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Live_Movies.Services
{
    public interface IMovieService
    {
        Task<List<Movie>> GetAllMoviesAsync();
        Task<Movie?> GetMovieByIdAsync(int id);
        Task<List<Movie>> SearchMoviesAsync(string searchTerm);
        Task<Movie> AddMovieAsync(MovieDto movieDto);
        Task UpdateMovieAsync(int id, MovieDto movieDto);
        Task DeleteMovieAsync(int id);
    }
    public class MovieDto
    {
        public string Name { get; set; } = string.Empty;
        public string Release_Year { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public List<string> Genre { get; set; } = new();
        public string Rating { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Cast { get; set; } = new();
        public string Image_url { get; set; } = string.Empty;
        public string Watch_url { get; set; } = string.Empty;
    }

    // Separate DTO for form data with file upload
    public class MovieWithImageDto : MovieDto
    {
        public IFormFile? ImageFile { get; set; }
    }
    public class MovieService : IMovieService
    {
        private readonly MovieDbContext _context;

        public MovieService(MovieDbContext context)
        {
            _context = context;
        }

        public async Task<Movie?> GetMovieByIdAsync(int id)
        {
            return await _context.Movies
                .Include(m => m.MovieGenres)
                    .ThenInclude(mg => mg.Genre)
                .Include(m => m.MovieCasts)
                    .ThenInclude(mc => mc.CastMember)
                .FirstOrDefaultAsync(m => m.id == id);
        }

        public async Task<List<Movie>> SearchMoviesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllMoviesAsync();

            searchTerm = searchTerm.ToLower().Trim();

            // Get movies without including navigation properties to avoid circular references
            var movies = await _context.Movies.ToListAsync();

            // Filter using computed properties that don't cause circular references
            var results = movies.Where(m =>
                m.Name.ToLower().Contains(searchTerm) ||
                m.Language.ToLower().Contains(searchTerm) ||
                m.Release_Year.Contains(searchTerm) ||
                m.Description.ToLower().Contains(searchTerm) ||
                m.Rating.ToLower().Contains(searchTerm) ||
                m.Genre.Any(g => g.ToLower().Contains(searchTerm)) ||
                m.Cast.Any(c => c.ToLower().Contains(searchTerm))
            ).ToList();

            return results;
        }

        public async Task<List<Movie>> GetAllMoviesAsync()
        {
            // Get movies without including navigation properties to avoid circular references
            return await _context.Movies.ToListAsync();
        }


        public async Task<Movie> AddMovieAsync(MovieDto movieDto)
        {
            var movie = new Movie
            {
                Name = movieDto.Name,
                Release_Year = movieDto.Release_Year,
                Language = movieDto.Language,
                Rating = movieDto.Rating,
                Description = movieDto.Description,
                Image_url = movieDto.Image_url,
                Watch_url = movieDto.Watch_url,
                GenreJson = JsonSerializer.Serialize(movieDto.Genre ?? new List<string>()),
                CastJson = JsonSerializer.Serialize(movieDto.Cast ?? new List<string>())
            };
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            //Handle Genres
            foreach (var genreName in movieDto.Genre ?? new List<string>())
            {
                var genre = await GetOrCreateGenreAsync(genreName.Trim());
                _context.MovieGenres.Add(new MovieGenre { MovieId = movie.id, GenreId = genre.Id });
            }

            // Handle Cast Member
            foreach (var castName in movieDto.Cast ?? new List<string>())
            {
                var castMember = await GetOrCreateCastMemberAsync(castName.Trim());
                _context.MovieCasts.Add(new MovieCast { MovieId = movie.id, CastMemberId = castMember.Id });
            }

            await _context.SaveChangesAsync();

            var result = await GetMovieByIdAsync(movie.id);
            if (result is null)
                throw new InvalidOperationException("Movie was not found after creation.");
            return result;
        }

        public async Task UpdateMovieAsync(int id, MovieDto movieDto)
        {
            var movie = await _context.Movies
                               .Include(m => m.MovieGenres)
                               .Include(m => m.MovieCasts)
                               .FirstOrDefaultAsync(m => m.id == id);
            if (movie == null)
                throw new ArgumentException("Movie not found.");

            //Update Basic Properties
            movie.Name = movieDto.Name;
            movie.Release_Year = movieDto.Release_Year;
            movie.Language = movieDto.Language;
            movie.Rating = movieDto.Rating;
            movie.Description = movieDto.Description;
            movie.Image_url = movieDto.Image_url;
            movie.Watch_url = movieDto.Watch_url;
            movie.GenreJson = JsonSerializer.Serialize(movieDto.Genre ?? new List<string>());
            movie.CastJson = JsonSerializer.Serialize(movieDto.Cast ?? new List<string>());

            // Update Genres
            _context.MovieGenres.RemoveRange(movie.MovieGenres);

            foreach (var genreName in movieDto.Genre ?? new List<string>())
            {
                var genre = await GetOrCreateGenreAsync(genreName.Trim());
                _context.MovieGenres.Add(new MovieGenre { MovieId = movie.id, GenreId = genre.Id });
            }

            //Update Cast Members
            _context.MovieCasts.RemoveRange(movie.MovieCasts); // ADD THIS LINE to remove existing cast

            foreach (var castName in movieDto.Cast ?? new List<string>())
            {
                var castMember = await GetOrCreateCastMemberAsync(castName.Trim());
                _context.MovieCasts.Add(new MovieCast { MovieId = movie.id, CastMemberId = castMember.Id });
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteMovieAsync(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();
            }
        }

        private async Task<Genre> GetOrCreateGenreAsync(string genreName)
        {
            var genre = await _context.Genres
                .FirstOrDefaultAsync(g => g.Name == genreName);

            if (genre == null)
            {
                genre = new Genre { Name = genreName };
                _context.Genres.Add(genre);
                await _context.SaveChangesAsync();
            }

            return genre;
        }

        private async Task<CastMember> GetOrCreateCastMemberAsync(string castName)
        {
            var castMember = await _context.CastMembers
                .FirstOrDefaultAsync(c => c.Name == castName);

            if (castMember == null)
            {
                castMember = new CastMember { Name = castName };
                _context.CastMembers.Add(castMember);
                await _context.SaveChangesAsync();
            }

            return castMember;
        }
    }
}
