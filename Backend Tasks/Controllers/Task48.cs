using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Task48.Controllers
{
    public class Upload
    {
        public IFormFile? File { get; set; }
        public string Owner { get; set; }
    }

    public class FileMetadata
    {
        public string Owner { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastModificationTime { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class BaseController : ControllerBase
    {
        protected readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        public BaseController()
        {
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }

        protected string SanitizeFileName(string fileName)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }
    }

    public class CreateController : BaseController
    {
        [HttpPost("CreateTask48")]
        public async Task<IActionResult> Post([FromForm] Upload uploadFile)
        {
            IFormFile? file = uploadFile.File;
            string owner = uploadFile.Owner;

            if (file == null || file.Length == 0)
                return BadRequest("No file selected");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || extension != ".jpg")
                return BadRequest("Invalid file type");

            var sanitizedOwnerName = SanitizeFileName(owner);
            var newFileName = $"{sanitizedOwnerName}{extension}";
            var filePath = Path.Combine(_storagePath, newFileName);

            if (System.IO.File.Exists(filePath))
                return BadRequest("A file with the same owner name already exists.");

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var metadata = new FileMetadata
                {
                    Owner = owner,
                    CreationTime = DateTime.Now,
                    LastModificationTime = DateTime.Now
                };

                var metadataPath = Path.Combine(_storagePath, $"{sanitizedOwnerName}.json");
                await System.IO.File.WriteAllTextAsync(metadataPath, JsonSerializer.Serialize(metadata));

                var fileUrl = Url.Content($"~/uploads/{newFileName}");
                return Created(fileUrl, new { url = fileUrl });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error while uploading the file.");
            }
        }
    }

    public class DeleteController : BaseController
    {
        [HttpGet("DeleteTask48")]
        public IActionResult Delete([FromQuery] string fileName, [FromQuery] string fileOwner)
        {
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(fileOwner))
            {
                return BadRequest("Invalid input data.");
            }

            var sanitizedFileName = SanitizeFileName(Path.GetFileNameWithoutExtension(fileName));
            var filePath = Path.Combine(_storagePath, $"{sanitizedFileName}.jpg");
            var metadataPath = Path.Combine(_storagePath, $"{sanitizedFileName}.json");

            if (!System.IO.File.Exists(filePath) || !System.IO.File.Exists(metadataPath))
            {
                return BadRequest("File or metadata not found.");
            }

            var metadata = JsonSerializer.Deserialize<FileMetadata>(System.IO.File.ReadAllText(metadataPath));

            if (metadata.Owner != fileOwner)
            {
                return Forbid("You do not have permission to delete this file.");
            }

            try
            {
                System.IO.File.Delete(filePath);
                System.IO.File.Delete(metadataPath);
                return Ok("File and its metadata deleted successfully.");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error while deleting the file.");
            }
        }
    }
    public class UpdateController : BaseController
    {
        [HttpPost("UpdateTask48")]
        public async Task<IActionResult> Post([FromForm] Upload uploadFile)
        {
            IFormFile? file = uploadFile.File;
            string owner = uploadFile.Owner;

            if (file == null || file.Length == 0)
                return BadRequest("No file selected");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || extension != ".jpg")
                return BadRequest("Invalid file type");

            var sanitizedOwnerName = SanitizeFileName(owner);
            var fileName = $"{sanitizedOwnerName}{extension}";
            var filePath = Path.Combine(_storagePath, fileName);
            var metadataPath = Path.Combine(_storagePath, $"{sanitizedOwnerName}.json");

            if (!System.IO.File.Exists(filePath) || !System.IO.File.Exists(metadataPath))
                return BadRequest("File or metadata not found.");

            var metadata = JsonSerializer.Deserialize<FileMetadata>(System.IO.File.ReadAllText(metadataPath));

            if (metadata.Owner != owner)
                return Forbid("You do not have permission to update this file.");

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                metadata.LastModificationTime = DateTime.Now;
                await System.IO.File.WriteAllTextAsync(metadataPath, JsonSerializer.Serialize(metadata));

                return Ok("File and its metadata updated successfully.");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error while updating the file.");
            }
        }
    }
    public class RetrieveController : BaseController
    {
        [HttpGet("RetrieveTask48")]
        public IActionResult Retrieve([FromQuery] string fileName, [FromQuery] string fileOwner)
        {
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(fileOwner))
            {
                return BadRequest("Invalid input data.");
            }

            var sanitizedFileName = SanitizeFileName(Path.GetFileNameWithoutExtension(fileName));
            var filePath = Path.Combine(_storagePath, $"{sanitizedFileName}.jpg");
            var metadataPath = Path.Combine(_storagePath, $"{sanitizedFileName}.json");

            if (!System.IO.File.Exists(filePath) || !System.IO.File.Exists(metadataPath))
            {
                return NotFound("File or metadata not found.");
            }

            var metadata = JsonSerializer.Deserialize<FileMetadata>(System.IO.File.ReadAllText(metadataPath));

            if (metadata.Owner != fileOwner)
            {
                return Forbid("You do not have permission to retrieve this file.");
            }

            try
            {
                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                return File(fileStream, "application/octet-stream", $"{sanitizedFileName}.jpg");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error while retrieving the file.");
            }
        }
    }
}
