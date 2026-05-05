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
            var fileNameWithPrefix = Path.GetFileName(subpath) ?? string.Empty;
            if (string.IsNullOrEmpty(fileNameWithPrefix))
            {
                return new NotFoundFileInfo(subpath);
            }

            if (fileNameWithPrefix.Contains('_'))
            {
                var parts = fileNameWithPrefix.Split('_');
                if (int.TryParse(parts[0], out int place))
                {
                    var folderPath = UploadPaths.GetPath(place);
                    var actualFileName = string.Join("_", parts.Skip(1));

                    if (!string.IsNullOrEmpty(folderPath))
                    {
                        var fileLocation = Path.Combine(_wwwRootPath, folderPath, actualFileName);
                        if (File.Exists(fileLocation))
                            return new PhysicalFileInfo(new FileInfo(fileLocation));
                    }
                }
            }

            foreach (var path in UploadPaths.GetAllPaths())
            {
                if (string.IsNullOrWhiteSpace(path)) continue;

                var fileLocation = Path.Combine(_wwwRootPath, path, fileNameWithPrefix);
                if (File.Exists(fileLocation))
                    return new PhysicalFileInfo(new FileInfo(fileLocation));
            }

            var rootFileLocation = Path.Combine(_wwwRootPath, fileNameWithPrefix);
            if (File.Exists(rootFileLocation))
                return new PhysicalFileInfo(new FileInfo(rootFileLocation));

            return new NotFoundFileInfo(fileNameWithPrefix);
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
