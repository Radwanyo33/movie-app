using Live_Movies.Models;
using Live_Movies.Data;
using Live_Movies.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Live_Movies.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly IImageService _imageService;
        private readonly ILogger<MoviesController> _logger;
        public MoviesController(IMovieService movieService, IImageService imageService, ILogger<MoviesController> logger)
        {
            _movieService = movieService;
            _imageService = imageService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMovies()
        {
            try
            {
                var movies = await _movieService.GetAllMoviesAsync();
                return Ok(movies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all movies");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchMovies([FromQuery] string q)
        {
            try
            {
                var movies = await _movieService.SearchMoviesAsync(q ?? "");
                return Ok(movies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching movies");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddMovie([FromBody] MovieDto movieDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var movie = await _movieService.AddMovieAsync(movieDto);
                return Ok(new { message = "Movie added successfully", movie });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding movie");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("with-image")]
        public async Task<IActionResult> AddMovieWithImage([FromForm] MovieWithImageDto movieDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Handle image upload if present
                if (movieDto.ImageFile != null && movieDto.ImageFile.Length > 0)
                {
                    movieDto.Image_url = await _imageService.SaveImageAsync(movieDto.ImageFile);
                }

                var processedGenres = ProcessListWithCommas(movieDto.Genre);
                var processedCasts = ProcessListWithCommas(movieDto.Cast);
                // Convert to regular MovieDto for service
                var regularMovieDto = new MovieDto
                {
                    Name = movieDto.Name,
                    Release_Year = movieDto.Release_Year,
                    Language = movieDto.Language,
                    Genre = processedGenres,       // Make sure this is passed
                    Rating = movieDto.Rating,
                    Description = movieDto.Description,
                    Cast = processedCasts,          // Make sure this is passed
                    Image_url = movieDto.Image_url,
                    Watch_url = movieDto.Watch_url
                };

                var movie = await _movieService.AddMovieAsync(regularMovieDto);
                return Ok(new { message = "Movie added successfully", movie });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding movie with image");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        private List<string> ProcessListWithCommas(List<string> inputList)
        {
            if (inputList == null) return new List<string>();

            return inputList.Select(item =>
            {
                // Split by comma and trim each part, then join with comma + space
                var parts = item.Split(',', StringSplitOptions.RemoveEmptyEntries)
                               .Select(part => part.Trim())
                               .Where(part => !string.IsNullOrWhiteSpace(part));

                return string.Join(", ", parts);
            }).ToList();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMovie(int id, [FromBody] MovieDto movieDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _movieService.UpdateMovieAsync(id, movieDto);
                return Ok(new { message = "Movie updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating movie");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            try
            {
                await _movieService.DeleteMovieAsync(id);
                return Ok(new { message = "Movie deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting movie");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}