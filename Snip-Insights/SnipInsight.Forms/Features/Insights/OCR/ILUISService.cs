using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Refit;
using SnipInsight.Forms.Features.Insights.OCR.Models;

namespace SnipInsight.Forms.Features.Insights.OCR
{
    public interface ILUISService
    {
        [Post("/apps/{LUISAppID}")]
        Task<LUISModel> Proccess(
                                 [AliasAs("LUISAppID")] string luisappid,
                                 string q,
                                 string timezoneOffset,
                                 string verbose,
                                 string spellCheck,
                                 string staging,
                                 [Header("Ocp-Apim-Subscription-Key")] string apiKey);
    }
}
