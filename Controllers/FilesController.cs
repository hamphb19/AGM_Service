using AGM_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace AGM_API.Controllers
{
    /// <summary>
    /// Liefert hochgeladene Dateien aus. Kein Login nötig (Bilder werden direkt von
    /// &lt;Image&gt; geladen), aber nur mit gültiger, ablaufender Signatur, die die API
    /// ausschließlich an berechtigte Farm-Mitglieder herausgibt.
    /// </summary>
    [Route("api/files")]
    [ApiController]
    [AllowAnonymous]
    public class FilesController(FileStorageService storage) : ControllerBase
    {
        private static readonly FileExtensionContentTypeProvider ContentTypes = new();

        [HttpGet("{**path}")]
        public IActionResult Get(string path, [FromQuery] long exp, [FromQuery] string? sig)
        {
            var full = storage.Resolve(path, exp, sig);
            if (full == null) return NotFound();

            if (!ContentTypes.TryGetContentType(full, out var contentType))
                contentType = "application/octet-stream";

            Response.Headers.CacheControl = "private, max-age=3600";
            return PhysicalFile(full, contentType);
        }
    }
}
