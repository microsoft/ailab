using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Speaker.Recorder.Kinect
{
    public class KinectCaptureProvidersFactory
    {
        private readonly Dictionary<object, (int count, KinectCaptureProvider provider)> currentCaptureProviders = new Dictionary<object, (int, KinectCaptureProvider)>();
        private readonly ILogger<KinectCaptureProvidersFactory> logger;
        private readonly IConfiguration configuration;

        public KinectCaptureProvidersFactory(ILogger<KinectCaptureProvidersFactory> logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        public void Remove(object key)
        {
            if (key != null && currentCaptureProviders.TryGetValue(key, out var current))
            {
                current.count--;
                this.logger.LogInformation($"KinectProvider for {key} count decremented {current.count}");
                if (current.count <= 0)
                {
                    this.logger.LogInformation($"Removing KinectProvider for {key} and killing process");
                    currentCaptureProviders.Remove(key);
                    current.provider.Dispose();
                }
                else
                {
                    currentCaptureProviders[key] = current;
                }
            }
        }

        public KinectCaptureProvider GetOrCreateProvider(object key)
        {
            if (key != null)
            {
                if (!currentCaptureProviders.TryGetValue(key, out var current))
                {
                    this.logger.LogInformation($"Creating KinectProvider for {key}");
                    KinectCaptureProvider provider = key switch
                    {
                        int deviceIndex when deviceIndex >= 0 => new KinectCaptureProvider(deviceIndex, this.configuration),
                        string playbackPath when playbackPath != null => new KinectCaptureProvider(playbackPath),
                        _ => throw new InvalidOperationException("The Azure Kinect provider only can be opened from a deviceId or a plaback file path."),
                    };

                    current = (0, provider);
                }

                this.logger.LogInformation($"KinectProvider for {key} count incremented {current.count}");
                currentCaptureProviders[key] = (++current.count, current.provider);
                return current.provider;
            }

            return null;
        }
    }
}
