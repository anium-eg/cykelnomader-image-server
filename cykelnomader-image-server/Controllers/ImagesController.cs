using Microsoft.AspNetCore.Mvc;

[Route("api/images")]
[ApiController]
public class ImagesController : ControllerBase
{
    private readonly string _imageStoragePath;

    public ImagesController(IConfiguration configuration, IWebHostEnvironment env)
    {
        _imageStoragePath = Path.Combine(env.ContentRootPath, configuration["ImageStoragePath"]);

        if (!Directory.Exists(_imageStoragePath))
        {
            Directory.CreateDirectory(_imageStoragePath);
        }
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage(IFormFile file, [FromForm] string location)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        if (string.IsNullOrWhiteSpace(location))
        {
            return BadRequest("Location is required.");
        }



        // Sanitize location and create directory
        var sanitizedLocation = location.Trim().Replace("\\", "/");
        var folderPath = Path.Combine(_imageStoragePath, sanitizedLocation);

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(folderPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var fileUrl = $"{Request.Scheme}://{Request.Host}/api/images/{sanitizedLocation}/{fileName}";

        return Ok(new { Url = fileUrl });
    }

    [HttpGet("{*filePath}")]
    public IActionResult GetImage(string filePath)
    {
        var fullFilePath = Path.Combine(_imageStoragePath, filePath);

        if (!System.IO.File.Exists(fullFilePath))
            return NotFound("Image not found.");

        var fileStream = System.IO.File.OpenRead(fullFilePath);
        var contentType = "image/jpeg"; // Adjust based on file type
        return File(fileStream, contentType);
    }

}
