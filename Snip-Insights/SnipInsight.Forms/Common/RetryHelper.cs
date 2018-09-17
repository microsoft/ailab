using System;
using System.Net;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using Refit;
using SnipInsight.Forms.Features.Insights.OCR.Models;

namespace SnipInsight.Forms.Common
{
    public static class RetryHelper
    {
        private static RetryPolicy retryPolicy = GetDefaultPolicy();

        private static RetryPolicy<HandWrittenModel> longRetryPolicy = GetLongRetryPolicy();

        public static RetryPolicy DefaultPolicy { get; } = retryPolicy;

        public static RetryPolicy<HandWrittenModel> LongRetryPolicy { get; } = longRetryPolicy;

        public static Task WrapAsync(Task task)
        {
            return DefaultPolicy.ExecuteAsync(() => task);
        }

        public static Task<T> WrapAsync<T>(Task<T> task)
        {
            return DefaultPolicy.ExecuteAsync(() => task);
        }

        public static Task<HandWrittenModel> WrapLongRetryAsync<THandWrittenModel>(Task<HandWrittenModel> task)
        {
            return LongRetryPolicy.ExecuteAsync(() => task);
        }

        private static RetryPolicy GetDefaultPolicy()
        {
            return Policy
               .Handle<ApiException>(exception =>
               {
                   if (exception.StatusCode == HttpStatusCode.Unauthorized ||
                       (int)exception.StatusCode == 429)
                   {
                       return false;
                   }

                   return true;
               })
               .WaitAndRetryAsync(new[]
               {
                   TimeSpan.FromSeconds(0.5),
                   TimeSpan.FromSeconds(1),
                   TimeSpan.FromSeconds(2)
               });
        }

        private static RetryPolicy<HandWrittenModel> GetLongRetryPolicy()
        {
            return Policy
                 .Handle<ApiException>(exception =>
                 {
                     if (exception.StatusCode == HttpStatusCode.Unauthorized ||
                         (int)exception.StatusCode == 429)
                     {
                         return false;
                     }

                     return true;
                 })
                .OrResult<HandWrittenModel>(result =>
                {
                    if (result.Status == "Running"
                         && result.RecognitionResult == null)
                    {
                        return true;
                    }

                    return false;
                })
                .WaitAndRetryAsync<HandWrittenModel>(new[]
               {
                   TimeSpan.FromSeconds(0.5),
                   TimeSpan.FromSeconds(1),
                   TimeSpan.FromSeconds(2),
                   TimeSpan.FromSeconds(4),
                   TimeSpan.FromSeconds(8),
                   TimeSpan.FromSeconds(16)
               });
        }
    }
}
