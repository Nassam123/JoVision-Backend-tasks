﻿using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Task49.Controllers
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
    public enum FilterType
    {
        ByModificationDate,
        ByCreationDateDescending,
        ByCreationDateAscending,
        ByOwner
    }

    public class FilterRequest
    {
        public DateTime CreationDate { get; set; }
        public DateTime ModificationDate { get; set; }
        public string Owner { get; set; }
        public FilterType FilterType { get; set; }
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
        [HttpPost("Task49")]
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
        [HttpGet("Task49")]
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
        [HttpPost("Task49")]
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
        [HttpGet("Task49")]
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
    public class FilterController : BaseController
    {
        [HttpPost("Task49")]
        public IActionResult Filter([FromForm] FilterRequest filterRequest)
        {
            if (filterRequest == null || string.IsNullOrEmpty(filterRequest.Owner) && (filterRequest.FilterType == FilterType.ByOwner))
            {
                return BadRequest("Invalid input data.");
            }

            try
            {
                var files = Directory.GetFiles(_storagePath, "*.json")
                    .Select(f => JsonSerializer.Deserialize<FileMetadata>(System.IO.File.ReadAllText(f)))
                    .Where(fm => fm != null)
                    .Select(fm => new { fm.Owner, FileName = Path.GetFileNameWithoutExtension(fm.Owner) + ".jpg", fm.CreationTime, fm.LastModificationTime })
                    .ToList();

                IEnumerable<object> filteredFiles = null;

                switch (filterRequest.FilterType)
                {
                    case FilterType.ByModificationDate:
                        filteredFiles = files.Where(f => f.LastModificationTime < filterRequest.ModificationDate);
                        break;

                    case FilterType.ByCreationDateDescending:
                        filteredFiles = files.Where(f => f.CreationTime > filterRequest.CreationDate)
                                             .OrderByDescending(f => f.CreationTime);
                        break;

                    case FilterType.ByCreationDateAscending:
                        filteredFiles = files.Where(f => f.CreationTime > filterRequest.CreationDate)
                                             .OrderBy(f => f.CreationTime);
                        break;

                    case FilterType.ByOwner:
                        filteredFiles = files.Where(f => f.Owner == filterRequest.Owner);
                        break;

                    default:
                        return BadRequest("Invalid filter type.");
                }

                return Ok(filteredFiles);
            }
            catch (Exception)
            {
                Console.WriteLine(this.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error while filtering the files.");
            }
        }
    }
    public class TransferOwnershipController : BaseController
    {
        [HttpGet("Task49")]
        public IActionResult TransferOwnership([FromQuery] string oldOwner, [FromQuery] string newOwner)
        {
            if (string.IsNullOrEmpty(oldOwner) || string.IsNullOrEmpty(newOwner))
            {
                return BadRequest("Invalid input data.");
            }

            try
            {
                var files = Directory.GetFiles(_storagePath, "*.json")
                    .Select(f => new
                    {
                        MetadataPath = f,
                        Metadata = JsonSerializer.Deserialize<FileMetadata>(System.IO.File.ReadAllText(f))
                    })
                    .Where(f => f.Metadata != null && f.Metadata.Owner == oldOwner)
                    .ToList();

                foreach (var file in files)
                {
                    file.Metadata.Owner = newOwner;
                    System.IO.File.WriteAllText(file.MetadataPath, JsonSerializer.Serialize(file.Metadata));

                    var oldFileName = Path.GetFileNameWithoutExtension(file.MetadataPath) + ".jpg";
                    var newFileName = Path.Combine(_storagePath, SanitizeFileName(newOwner) + ".jpg");

                    if (System.IO.File.Exists(Path.Combine(_storagePath, oldFileName)))
                    {
                        System.IO.File.Move(Path.Combine(_storagePath, oldFileName), newFileName);
                    }
                }

                var newOwnerFiles = Directory.GetFiles(_storagePath, "*.json")
                    .Select(f => JsonSerializer.Deserialize<FileMetadata>(System.IO.File.ReadAllText(f)))
                    .Where(fm => fm != null && fm.Owner == newOwner)
                    .Select(fm => new { fm.Owner, FileName = Path.GetFileNameWithoutExtension(fm.Owner) + ".jpg", fm.CreationTime, fm.LastModificationTime })
                    .ToList();

                return Ok(newOwnerFiles);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error while transferring ownership.");
            }
        }
    }
}
