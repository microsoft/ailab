using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SnipInsight.Forms.Common;
using Xamarin.Forms;

[assembly: Dependency(typeof(SnipInsight.Forms.Features.Library.LibraryService))]

namespace SnipInsight.Forms.Features.Library
{
    public class LibraryService : ILibraryService
    {
        public LibraryService()
        {
            var snipsPath = Settings.Settings.SnipsPath;

            if (!Directory.Exists(snipsPath))
            {
                Directory.CreateDirectory(snipsPath);
            }
        }

        public void Delete(ImageModel image)
        {
            if (File.Exists(image.Path))
            {
                File.Delete(image.Path);
            }

            if (File.Exists(image.PathThumbnail))
            {
                File.Delete(image.PathThumbnail);
            }
        }

        public DateTime GetLastWriteTime(string path)
        {
            if (!File.Exists(path))
            {
                return DateTime.MinValue;
            }

            return File.GetLastWriteTime(path);
        }

        public async Task<IEnumerable<ImageModel>> LoadAllAsync()
        {
            var images = new List<ImageModel>();

            foreach (var extension in Constants.ValidExtensions)
            {
                var currentPaths = Directory.EnumerateFiles(Settings.Settings.SnipsPath, extension);
                images.AddRange(currentPaths.Select(path => new ImageModel(path)));
            }

            var tasksResizing = images.Select(
                image => Task.Run(() => ImageHelper.Resize(image.Path, 160, 100, image.PathThumbnail)))
                                      .AsParallel();
            await Task.WhenAll(tasksResizing);

            return images;
        }
    }
}
