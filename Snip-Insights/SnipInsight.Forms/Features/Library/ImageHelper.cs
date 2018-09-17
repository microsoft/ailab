using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;

namespace SnipInsight.Forms.Features.Library
{
    public static class ImageHelper
    {
        public static void Resize(string pathSource, int newWidth, int newHeight, string pathDestiny)
        {
            if (File.Exists(pathDestiny))
            {
                return;
            }

            var resizeOptions = new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(newWidth, newHeight) };

            using (var image = Image.Load(pathSource))
            using (var thumbnail = image.Clone(operation => operation.Resize(resizeOptions)))
            {
                var directoryDestiny = Path.GetDirectoryName(pathDestiny);

                if (!Directory.Exists(directoryDestiny))
                {
                    Directory.CreateDirectory(directoryDestiny);
                }

                thumbnail.Save(pathDestiny);
            }
        }

        internal static MemoryStream GetStream(string imagePath)
        {
            MemoryStream stream = new MemoryStream();

            using (FileStream source = File.Open(imagePath, FileMode.Open))
            {
                source.CopyTo(stream);
            }

            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }
    }
}
