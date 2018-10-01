using IO = System.IO;

namespace SnipInsight.Forms.Features.Library
{
    public class ImageModel
    {
        private const string ThumbnailsFolder = "Thumbnails";

        private readonly string filename;
        private readonly string folder;
        private readonly string path;
        private string pathThumbnail;

        public ImageModel(string path)
        {
            this.path = path;

            this.folder = IO.Path.GetDirectoryName(path);
            this.filename = IO.Path.GetFileName(path);
            this.pathThumbnail = IO.Path.Combine(this.folder, ThumbnailsFolder, this.filename);
        }

        public string Path => this.path;

        public string PathThumbnail => this.pathThumbnail;
    }
}
