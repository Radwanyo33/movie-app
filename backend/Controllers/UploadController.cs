using Live_Movies.Services;
using Microsoft.AspNetCore.Mvc;

namespace Live_Movies.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly ILogger<UploadController> _logger;

        public UploadController(IImageService imageService, ILogger<UploadController> logger)
        {
            _imageService = imageService;
            _logger = logger;
        }

        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                if (file == null)
                    return BadRequest(new { success = false, message = "No file provided" });

                var imagePath = await _imageService.SaveImageAsync(file);
                return Ok(new { success = true, imagePath = imagePath });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        [HttpDelete("image")]
        public async Task<IActionResult> DeleteImage([FromQuery] string imagePath)
        {
            try
            {
                var result = await _imageService.DeleteImageAsync(imagePath);
                if (result)
                {
                    return Ok(new { success = true, message = "Image has been deleted successfully" });
                }
                else
                    return NotFound(new { success = false, message = "Image not found." });
            }catch(Exception ex)
            {
                _logger.LogError(ex, "Error deleting image");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
