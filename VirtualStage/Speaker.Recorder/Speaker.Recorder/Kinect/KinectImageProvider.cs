using Microsoft.Azure.Kinect.Sensor;
using SharpDX.WIC;
using System;
using System.Threading;

namespace Speaker.Recorder.Kinect
{
    public class KinectImageProvider : IDisposable
    {
        private event EventHandler frameArrived;

        public event EventHandler FrameArrived
        {
            add
            {
                this.frameArrived += value;
                if (this.frameArrived?.GetInvocationList().Length == 1)
                {
                    this.provider.CaptureArrived += this.OnCaptureArrived;
                }
            }
            remove
            {
                this.frameArrived -= value;
                if (this.frameArrived == null || this.frameArrived.GetInvocationList().Length == 0)
                {
                    this.provider.CaptureArrived -= this.OnCaptureArrived;
                }
            }
        }

        private ImagingFactory imagingFactory;
        private DecodedKinectCaptureFrame lastFrame;
        private KinectCaptureProvider provider;

        public KinectCaptureImage ImageType { get; }

        public KinectImageProvider(KinectCaptureProvider provider, KinectCaptureImage type)
        {
            this.imagingFactory = new ImagingFactory();
            this.provider = provider;
            this.ImageType = type;
        }

        private void OnCaptureArrived(object sender, Microsoft.Azure.Kinect.Sensor.Capture capture)
        {
            var handler = this.frameArrived;
            if (handler != null)
            {
                using var image = this.GetImage(capture)?.Reference();
                handler = this.frameArrived;
                if (image != null && handler != null)
                {
                    var frame = new DecodedKinectCaptureFrame(image.Reference(), this);
                    var oldFrame = Interlocked.Exchange(ref this.lastFrame, frame);
                    oldFrame?.Dispose();

                    handler = this.frameArrived;
                    handler?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public DecodedKinectCaptureFrame TryGetLastFrame()
        {
            return Interlocked.Exchange(ref this.lastFrame, null);
        }

        private Image GetImage(Microsoft.Azure.Kinect.Sensor.Capture capture)
        {
            return this.ImageType switch
            {
                KinectCaptureImage.Depth => capture.Depth,
                KinectCaptureImage.IR => capture.IR,
                _ => capture.Color,
            };
        }

        public void Dispose()
        {
            this.provider.CaptureArrived -= this.OnCaptureArrived;
            this.imagingFactory.Dispose();
            this.frameArrived = null;
            Interlocked.Exchange(ref this.lastFrame, null)?.Dispose();
        }
    }
}
