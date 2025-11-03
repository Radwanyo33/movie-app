using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Live_Movies.Services
{
    public interface IImageService
    {
        Task<string> SaveImageAsync(IFormFile imageFile);
        Task<bool> DeleteImageAsync(string imagePath);
    }
    
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg" };
        private const long _maxFileSize = 5 * 1024 * 1024; //5 MB

        public ImageService(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
        }

        public async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                throw new ArgumentException("No image file provided.");

            if (imageFile.Length > _maxFileSize)
                throw new ArgumentException($"File size exceeds the limit of {_maxFileSize / 1024 / 1024} MB");

            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
                throw new ArgumentException("Invalid file type. Allowed types: " + string.Join(", ", _allowedExtensions));

            // Determine upload path based on environment
            string uploadsFolder;
            if (_environment.IsProduction())
            {
                // In production, use Render's disk
                uploadsFolder = Path.Combine("/opt/render/project/src/backend/uploads", "movies");
            }
            else
            {
                // In development, use wwwroot (your existing flow)
                uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "movies");
            }

            // Create uploads folder if it doesn't exist
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            //Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            // Return relative path for database storage (maintains your existing flow)
            return $"/uploads/movies/{fileName}";
        }

        public async Task<bool> DeleteImageAsync(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return false;

            try
            {
                string fullPath;
                if (_environment.IsProduction())
                {
                    // In production, use Render's disk path
                    fullPath = Path.Combine("/opt/render/project/src/backend", imagePath.TrimStart('/'));
                }
                else
                {
                    // In development, use your existing WebRootPath
                    fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));
                }

                if (File.Exists(fullPath))
                {
                    await Task.Run(() => File.Delete(fullPath));
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}