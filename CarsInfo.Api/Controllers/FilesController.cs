using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace CarsInfo.Api.Controllers
{
    [Route("api/v{version:apiVersion}/files")]
    [Authorize]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly FileExtensionContentTypeProvider _fileExtensionContentTypeProvider;
        public FilesController(FileExtensionContentTypeProvider fileExtensionContentTypeProvider)
        {
            _fileExtensionContentTypeProvider = fileExtensionContentTypeProvider;
        }

        [HttpGet("{fileId}")]
        [ApiVersion(0.1,Deprecated =true)]
        public ActionResult GetFile(string fileId)
        {
            var pathToFile = "Boy Artmı.pptx";
            if (!System.IO.File.Exists(pathToFile))
            {
                return NotFound();
            }
            _fileExtensionContentTypeProvider.TryGetContentType(pathToFile, out var contentType);
            if (string.IsNullOrEmpty(contentType))
            {
                contentType = "application/octet-stream";
            }
            var bytes = System.IO.File.ReadAllBytes(pathToFile);
            return File(bytes, contentType, Path.GetFileName(pathToFile));
        }

        [HttpPost]
        public async Task<ActionResult> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length > 20000000
                || file.ContentType != "application/pdf")
            {
                return BadRequest("No file uploaded.");
            }
            var filepath = Path.Combine(Directory.GetCurrentDirectory(), $"Uploaded_File_{Guid.NewGuid()}.pdf");

            using (var stream = new FileStream(filepath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { filePath = filepath });
        }
    }
}
