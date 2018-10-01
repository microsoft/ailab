using System.IO;
using System.Threading.Tasks;
using Refit;

namespace SnipInsight.Forms.Common
{
    public interface IDownloadImageService
    {
        [Headers("Content-Type: application/octet-stream")]
        [Get("")]
        Task<Stream> GetImage();
    }
}
