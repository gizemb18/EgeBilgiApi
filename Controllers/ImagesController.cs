using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using ImageApi.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ImageApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly string[] _allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        private readonly string _imageRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

        [HttpGet("{category}/{fileName}")]
        public IActionResult GetImage(string category, string fileName)
        {
            var filePath = Path.Combine(_imageRootPath, category, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Image not found.");
            }

            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                return BadRequest("Unsupported image format.");
            }

            var image = System.IO.File.OpenRead(filePath);
            var contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };

            return File(image, contentType);
        }

        [HttpGet("list/{category}")]
        public IActionResult GetImageList(string category)
        {
            var directoryPath = Path.Combine(_imageRootPath, category);

            if (!Directory.Exists(directoryPath))
            {
                return NotFound("Category not found.");
            }

            var images = Directory.GetFiles(directoryPath)
                .Where(file => _allowedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                .Select(file => new ImageModel
                {
                    FileName = Path.GetFileName(file),
                    FilePath = Url.Action(nameof(GetImage), new { category, fileName = Path.GetFileName(file) }),
                    ContentType = Path.GetExtension(file).ToLowerInvariant() switch
                    {
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        _ => "application/octet-stream"
                    },
                    Size = new FileInfo(file).Length
                })
                .ToList();

            return Ok(images);
        }
    }
}
