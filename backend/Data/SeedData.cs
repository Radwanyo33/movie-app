using Microsoft.EntityFrameworkCore;
using Live_Movies.Models;
using Live_Movies.Services;
using System.Text.Json;

namespace Live_Movies.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<MovieDbContext>();
            var movieService = services.GetRequiredService<IMovieService>();

            // Ensure database is created and migrations are applied
            await context.Database.EnsureCreatedAsync();

            // FIRST: Always populate JSON fields for ALL existing movies
            await PopulateJsonFieldsForExistingMovies(context);

            // THEN: Only seed from JSON if database is completely empty
            if (!context.Movies.Any())
            {
                await SeedMoviesFromJson(context, movieService);
            }

            await context.SaveChangesAsync();
        }

        private static async Task PopulateJsonFieldsForExistingMovies(MovieDbContext context)
        {
            // Get all movies with their relationships
            var movies = await context.Movies
                .Include(m => m.MovieGenres)
                    .ThenInclude(mg => mg.Genre)
                .Include(m => m.MovieCasts)
                    .ThenInclude(mc => mc.CastMember)
                .ToListAsync();

            int populatedCount = 0;

            foreach (var movie in movies)
            {
                bool updated = false;

                // For movies with relationships (like IDs 37, 39), populate from relationships
                if (movie.MovieGenres.Any())
                {
                    var genres = movie.MovieGenres.Select(mg => mg.Genre.Name).ToList();
                    var serializedGenres = JsonSerializer.Serialize(genres);

                    if (movie.GenreJson != serializedGenres)
                    {
                        movie.GenreJson = serializedGenres;
                        updated = true;
                    }
                }

                if (movie.MovieCasts.Any())
                {
                    var cast = movie.MovieCasts.Select(mc => mc.CastMember.Name).ToList();
                    var serializedCast = JsonSerializer.Serialize(cast);

                    if (movie.CastJson != serializedCast)
                    {
                        movie.CastJson = serializedCast;
                        updated = true;
                    }
                }

                // For movies without relationships (IDs 1-34), we need to get data from seriesData.json
                if (!movie.MovieGenres.Any() && !movie.MovieCasts.Any() &&
                    (string.IsNullOrEmpty(movie.GenreJson) || movie.GenreJson == "[]"))
                {
                    // Get genre and cast data from the JSON file
                    var jsonData = await GetMovieDataFromJsonFile(movie.Name);
                    if (jsonData != null)
                    {
                        movie.GenreJson = JsonSerializer.Serialize(jsonData.Genre ?? new List<string>());
                        movie.CastJson = JsonSerializer.Serialize(jsonData.Cast ?? new List<string>());
                        updated = true;
                    }
                }

                if (updated)
                {
                    populatedCount++;
                    Console.WriteLine($"Updated JSON fields for: {movie.Name}");
                }
            }

            if (populatedCount > 0)
            {
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ Populated JSON fields for {populatedCount} movies");
            }
            else
            {
                Console.WriteLine($"✅ All movies already have JSON fields populated");
            }
        }

        private static async Task<MovieJsonDto?> GetMovieDataFromJsonFile(string movieName)
        {
            try
            {
                var jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "seriesData.json");
                if (!File.Exists(jsonFilePath))
                {
                    jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "api", "seriesData.json");
                    if (!File.Exists(jsonFilePath)) return null;
                }

                var jsonData = await File.ReadAllTextAsync(jsonFilePath);
                var moviesFromJson = JsonSerializer.Deserialize<List<MovieJsonDto>>(jsonData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return moviesFromJson?.FirstOrDefault(m =>
                    m.Name.Equals(movieName, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return null;
            }
        }

        private static async Task SeedMoviesFromJson(MovieDbContext context, IMovieService movieService)
        {
            try
            {
                var jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "seriesData.json");
                if (!File.Exists(jsonFilePath))
                {
                    jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "api", "seriesData.json");
                    if (!File.Exists(jsonFilePath))
                    {
                        Console.WriteLine("JSON File not found.");
                        return;
                    }
                }

                var jsonData = await File.ReadAllTextAsync(jsonFilePath);
                var moviesFromJson = JsonSerializer.Deserialize<List<MovieJsonDto>>(jsonData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (moviesFromJson == null || !moviesFromJson.Any())
                {
                    Console.WriteLine("No Movies found in JSON File.");
                    return;
                }

                Console.WriteLine($"Found {moviesFromJson.Count} movies from JSON File. Start Importing....");

                foreach (var movieJson in moviesFromJson)
                {
                    try
                    {
                        var movie = new Movie
                        {
                            Name = movieJson.Name,
                            Release_Year = movieJson.Release_Year,
                            Language = movieJson.Language,
                            Rating = movieJson.Rating,
                            Description = movieJson.Description,
                            Image_url = movieJson.Image_url,
                            Watch_url = movieJson.Watch_url,
                            GenreJson = JsonSerializer.Serialize(movieJson.Genre ?? new List<string>()),
                            CastJson = JsonSerializer.Serialize(movieJson.Cast ?? new List<string>())
                        };

                        context.Movies.Add(movie);
                        Console.WriteLine($"Added Movie: {movieJson.Name}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error adding movie {movieJson.Name}: {ex.Message}");
                    }
                }

                await context.SaveChangesAsync();
                Console.WriteLine("✅ Movie import completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during movie import: {ex.Message}");
            }
        }
    }

    public class MovieJsonDto
    {
        public string Name { get; set; } = string.Empty;
        public string Release_Year { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public List<string> Genre { get; set; } = new();
        public string Rating { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Cast { get; set; } = new();
        public string Id { get; set; } = string.Empty;
        public string Image_url { get; set; } = string.Empty;
        public string Watch_url { get; set; } = string.Empty;
    }
}