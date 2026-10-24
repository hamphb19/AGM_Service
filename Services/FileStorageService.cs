using System.Security.Cryptography;
using System.Text;

namespace AGM_API.Services
{
    /// <summary>
    /// Speichert hochgeladene Dateien außerhalb von wwwroot (konfigurierbar über Uploads:Path,
    /// auf Railway ein Volume) und liefert sie nur über signierte, ablaufende URLs aus
    /// (siehe FilesController). In der Datenbank werden nur relative Pfade gespeichert,
    /// z. B. "farms/12/logo-....png".
    /// </summary>
    public class FileStorageService
    {
        public const long MaxImageBytes = 10 * 1024 * 1024; // 10 MB
        private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
        private static readonly TimeSpan UrlLifetime = TimeSpan.FromHours(24);

        private readonly byte[] _signingKey;
        private readonly string? _publicBaseUrl;
        private readonly IHttpContextAccessor _http;

        public string Root { get; }

        public FileStorageService(IConfiguration config, IWebHostEnvironment env, IHttpContextAccessor http, ILogger<FileStorageService> logger)
        {
            _http = http;

            var configured = config["Uploads:Path"];
            Root = Path.GetFullPath(string.IsNullOrWhiteSpace(configured)
                ? Path.Combine(env.ContentRootPath, "App_Data", "uploads")
                : configured);
            Directory.CreateDirectory(Root);

            _publicBaseUrl = config["Uploads:PublicBaseUrl"]?.TrimEnd('/');

            var key = config["Uploads:SigningKey"];
            if (string.IsNullOrWhiteSpace(key))
            {
                logger.LogWarning("Uploads:SigningKey is not set — using a random key. Image URLs become invalid after each restart. Set Uploads__SigningKey in production.");
                _signingKey = RandomNumberGenerator.GetBytes(32);
            }
            else
            {
                _signingKey = Encoding.UTF8.GetBytes(key);
            }

            MigrateLegacyUploads(Path.Combine(env.ContentRootPath, "wwwroot", "uploads"), logger);
        }

        /// <summary>Prüft Typ, Größe und Datei-Signatur eines Bildes. Gibt eine Fehlermeldung oder null zurück.</summary>
        public static async Task<string?> ValidateImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return "No file provided.";
            if (file.Length > MaxImageBytes) return "File too large (max 10 MB).";

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(ext)) return "Unsupported file type.";

            var header = new byte[12];
            await using var s = file.OpenReadStream();
            var read = await s.ReadAsync(header);
            if (read < 12 || !LooksLikeImage(header)) return "File is not a valid image.";
            return null;
        }

        private static bool LooksLikeImage(byte[] h) =>
            (h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF) ||                                   // JPEG
            (h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47) ||                   // PNG
            (h[0] == 0x52 && h[1] == 0x49 && h[2] == 0x46 && h[3] == 0x46 &&
             h[8] == 0x57 && h[9] == 0x45 && h[10] == 0x42 && h[11] == 0x50);                   // WEBP

        /// <summary>Speichert die Datei unter folder/fileName und gibt den relativen Pfad zurück.</summary>
        public async Task<string> SaveAsync(IFormFile file, string folder, string fileName)
        {
            var relative = Combine(folder, fileName);
            var fullPath = ToFullPath(relative) ?? throw new InvalidOperationException("Invalid path.");
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            await using var stream = File.Create(fullPath);
            await file.CopyToAsync(stream);
            return relative;
        }

        public void Delete(string? relativeOrLegacy)
        {
            var full = ToFullPath(Normalize(relativeOrLegacy));
            if (full != null && File.Exists(full)) File.Delete(full);
        }

        public void DeleteFolder(string folder)
        {
            var full = ToFullPath(folder);
            if (full != null && Directory.Exists(full)) Directory.Delete(full, recursive: true);
        }

        /// <summary>Signierte URL für einen gespeicherten Pfad (auch alte absolute /uploads/-URLs werden umgewandelt).</summary>
        public string? GetUrl(string? relativeOrLegacy)
        {
            var relative = Normalize(relativeOrLegacy);
            if (string.IsNullOrEmpty(relative)) return relativeOrLegacy;

            // Ablauf auf volle Stunden runden, damit URLs eine Zeit lang stabil bleiben (Bild-Cache).
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var exp = (now / 3600 + 1) * 3600 + (long)UrlLifetime.TotalSeconds;
            var sig = Sign(relative, exp);
            var path = string.Join('/', relative.Split('/').Select(Uri.EscapeDataString));
            return $"{BaseUrl()}/api/files/{path}?exp={exp}&sig={sig}";
        }

        /// <summary>Prüft Signatur und Ablauf und liefert den Dateipfad oder null.</summary>
        public string? Resolve(string relative, long exp, string? sig)
        {
            if (string.IsNullOrEmpty(sig) || exp < DateTimeOffset.UtcNow.ToUnixTimeSeconds()) return null;
            var expected = Encoding.ASCII.GetBytes(Sign(relative, exp));
            var given = Encoding.ASCII.GetBytes(sig);
            if (!CryptographicOperations.FixedTimeEquals(expected, given)) return null;
            var full = ToFullPath(relative);
            return full != null && File.Exists(full) ? full : null;
        }

        private string Sign(string relative, long exp)
        {
            using var hmac = new HMACSHA256(_signingKey);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{relative}|{exp}"));
            return Convert.ToBase64String(hash).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private string BaseUrl()
        {
            if (!string.IsNullOrEmpty(_publicBaseUrl)) return _publicBaseUrl;
            var req = _http.HttpContext?.Request;
            return req == null ? "" : $"{req.Scheme}://{req.Host}";
        }

        private static string Combine(string folder, string fileName) =>
            $"{folder.Trim('/')}/{fileName}".Replace('\\', '/');

        /// <summary>Alte Werte wie "https://host/uploads/farms/1/logo.png" → "farms/1/logo.png".</summary>
        private static string? Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var idx = value.IndexOf("/uploads/", StringComparison.OrdinalIgnoreCase);
            if (value.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return idx >= 0 ? value[(idx + "/uploads/".Length)..].Split('?')[0] : null;
            return value.TrimStart('/');
        }

        /// <summary>Absoluter Pfad innerhalb von Root, oder null bei Pfad-Ausbruch ("..").</summary>
        private string? ToFullPath(string? relative)
        {
            if (string.IsNullOrEmpty(relative)) return null;
            var full = Path.GetFullPath(Path.Combine(Root, relative));
            var rootWithSep = Root.EndsWith(Path.DirectorySeparatorChar) ? Root : Root + Path.DirectorySeparatorChar;
            return full.StartsWith(rootWithSep, StringComparison.Ordinal) ? full : null;
        }

        private void MigrateLegacyUploads(string legacyRoot, ILogger logger)
        {
            try
            {
                if (!Directory.Exists(legacyRoot) || Path.GetFullPath(legacyRoot) == Root) return;
                foreach (var src in Directory.EnumerateFiles(legacyRoot, "*", SearchOption.AllDirectories))
                {
                    var dest = Path.Combine(Root, Path.GetRelativePath(legacyRoot, src));
                    if (File.Exists(dest)) continue;
                    Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                    File.Copy(src, dest);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not migrate legacy uploads from {LegacyRoot}", legacyRoot);
            }
        }
    }
}
