// EMRSystem.API/Controllers/PackagingController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Security.Claims;

namespace EMRSystem.API.Controllers
{
    [ApiController]
    [Route("api/packaging")]
    [Authorize(Roles = "Admin")] // Cực kỳ quan trọng: Chỉ Admin mới có quyền này
    public class PackagingController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<PackagingController> _logger;

        public PackagingController(IWebHostEnvironment env, ILogger<PackagingController> logger)
        {
            _env = env;
            _logger = logger;
        }

        /// <summary>
        /// Đóng gói và tải về toàn bộ mã nguồn của dự án (loại trừ các thư mục nhạy cảm).
        /// </summary>
        [HttpGet("source-code")]
        public IActionResult DownloadSourceCode([FromQuery] string? version = null)
        {
            _logger.LogWarning("Source code download initiated by user {User}", User.FindFirstValue(ClaimTypes.Name));

            try
            {
                // Giả định cấu trúc: project_root/backend/EMRSystem.API
                var solutionRoot = Directory.GetParent(Directory.GetParent(_env.ContentRootPath)!.FullName)!.FullName;
                
                var versionSuffix = string.IsNullOrWhiteSpace(version) ? DateTime.UtcNow.ToString("yyyyMMdd") : version;
                var zipFileName = $"emr-system-source-code-{versionSuffix}.zip";
                
                // Sử dụng MemoryStream để tránh tạo file tạm
                var memoryStream = new MemoryStream();
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    AddDirectoryToArchive(archive, solutionRoot, "");
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                return File(memoryStream, "application/zip", zipFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to package source code.");
                return Problem("An error occurred while packaging the source code.");
            }
        }

        /// <summary>
        /// Tải về gói build artifact đã được tạo sẵn bởi quy trình CI/CD.
        /// </summary>
        [HttpGet("artifact")]
        public IActionResult DownloadArtifact([FromQuery] string version = "latest")
        {
            _logger.LogInformation("Artifact download initiated for version {Version} by user {User}", version, User.FindFirstValue(ClaimTypes.Name));

            // Đường dẫn tới thư mục chứa các gói build
            var artifactsPath = Path.Combine(Directory.GetParent(_env.ContentRootPath)!.FullName, "out");
            if (!Directory.Exists(artifactsPath))
            {
                return NotFound(new { message = "Artifacts directory not found." });
            }

            var fileName = $"emr-system-{version}.zip";
            var filePath = Path.Combine(artifactsPath, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { message = $"Artifact for version '{version}' not found." });
            }

            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(stream, "application/zip", fileName);
        }


        private void AddDirectoryToArchive(ZipArchive archive, string sourceDir, string entryPrefix)
        {
            var excludeDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "bin", "obj", "node_modules", "dist", ".git", ".vs", ".vscode", ".idea", "out"
            };

            var excludeExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".user", ".suo", ".log"
            };

            var files = Directory.GetFiles(sourceDir, "*.*", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                if (excludeExtensions.Contains(Path.GetExtension(file))) continue;

                var entryName = Path.Combine(entryPrefix, Path.GetFileName(file)).Replace('\\', '/');
                archive.CreateEntryFromFile(file, entryName, CompressionLevel.Optimal);
            }

            var subDirs = Directory.GetDirectories(sourceDir);
            foreach (var subDir in subDirs)
            {
                var dirName = new DirectoryInfo(subDir).Name;
                if (excludeDirs.Contains(dirName)) continue;

                AddDirectoryToArchive(archive, subDir, Path.Combine(entryPrefix, dirName).Replace('\\', '/'));
            }
        }
    }
}