using System.IO;
using System.Threading.Tasks;
using Refit;

namespace SnipInsight.Forms.Common
{
    public static class IOHelper
    {
        public static async Task<string> SaveImageAndReturnPathAsync(string url)
        {
            var downloadImageService = RestService.For<IDownloadImageService>(url);
            var stream = await RetryHelper.WrapAsync(downloadImageService.GetImage());

            if (stream == null)
            {
                return null;
            }

            var path = Constants.TemporalImageFilename;

            using (var fileStream = File.Create(path))
            {
                await stream.CopyToAsync(fileStream);
            }

            return path;
        }
    }
}
