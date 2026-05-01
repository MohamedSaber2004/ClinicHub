using ClinicHub.Application.Common.Services;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;

namespace ClinicHub.API.Services
{
    public class CustomFileProvider : IFileProvider
    {
        private readonly string _wwwRootPath;

        public CustomFileProvider(string? wwwRootPath = null)
        {
            _wwwRootPath = !string.IsNullOrWhiteSpace(wwwRootPath)
                ? wwwRootPath
                : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            var filePath = Path.GetFileName(subpath) ?? string.Empty;
            if (string.IsNullOrEmpty(filePath))
            {
                return new NotFoundFileInfo(subpath);
            }

            foreach (var path in UploadPaths.GetAllPaths())
            {
                if (string.IsNullOrWhiteSpace(path)) continue;

                var fileLocation = Path.Combine(_wwwRootPath, path, filePath);
                if (File.Exists(fileLocation))
                    return new PhysicalFileInfo(new FileInfo(fileLocation));
            }

            var rootFileLocation = Path.Combine(_wwwRootPath, filePath);
            if (File.Exists(rootFileLocation))
                return new PhysicalFileInfo(new FileInfo(rootFileLocation));

            return new NotFoundFileInfo(filePath);
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return NotFoundDirectoryContents.Singleton;
        }

        public IChangeToken Watch(string filter)
        {
            return NullChangeToken.Singleton;
        }
    }
}
