using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SnipInsight.Forms.Features.Insights
{
    public interface ILoadable
    {
        Task LoadAsync(Stream image, CancellationToken cancelToken);
    }
}