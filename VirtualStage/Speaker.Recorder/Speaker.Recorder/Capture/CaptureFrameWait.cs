using System;
using System.Threading;
using Windows.Graphics;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;

namespace Speaker.Recorder.Capture
{
    public sealed class CaptureFrameWait : IDisposable
    {
        private readonly ManualResetEvent[] events;
        private readonly ManualResetEvent frameEvent;
        private readonly ManualResetEvent closedEvent;
        private readonly Func<GraphicsCaptureItem> itemFactory;
        private readonly SharpDX.Direct3D11.Multithread multithread;
        private IDirect3DDevice device;
        private SharpDX.Direct3D11.Device d3dDevice;
        private SharpDX.Direct3D11.Texture2D blankTexture;
        private Direct3D11CaptureFrame currentFrame;
        private GraphicsCaptureSession session;
        private Direct3D11CaptureFramePool framePool;
        private int initializationThread;

        public GraphicsCaptureItem CurrentItem { get; private set; }

        public CaptureFrameWait(IDirect3DDevice device, Func<GraphicsCaptureItem> itemFactory)
        {
            this.device = device;
            this.d3dDevice = Direct3D11Helper.CreateSharpDXDevice(device);
            this.multithread = d3dDevice.QueryInterface<SharpDX.Direct3D11.Multithread>();
            this.multithread.SetMultithreadProtected(true);
            this.frameEvent = new ManualResetEvent(false);
            this.closedEvent = new ManualResetEvent(false);
            this.events = new[] { closedEvent, frameEvent };
            this.itemFactory = itemFactory;
            this.CurrentItem = this.itemFactory();

            InitializeBlankTexture(this.CurrentItem.Size);
            InitializeCapture();
        }

        private void InitializeCapture()
        {
            this.initializationThread = Thread.CurrentThread.ManagedThreadId;
            this.CurrentItem.Closed += this.OnClosed;
            this.framePool = Direct3D11CaptureFramePool.CreateFreeThreaded(
                this.device,
                DirectXPixelFormat.B8G8R8A8UIntNormalized,
                1,
                this.CurrentItem.Size);
            this.framePool.FrameArrived += this.OnFrameArrived;
            this.session = this.framePool.CreateCaptureSession(this.CurrentItem);
            this.session.StartCapture();
        }

        private void InitializeBlankTexture(SizeInt32 size)
        {
            var description = new SharpDX.Direct3D11.Texture2DDescription
            {
                Width = size.Width,
                Height = size.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                SampleDescription = new SharpDX.DXGI.SampleDescription()
                {
                    Count = 1,
                    Quality = 0
                },
                Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource | SharpDX.Direct3D11.BindFlags.RenderTarget,
                CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None
            };
            this.blankTexture = new SharpDX.Direct3D11.Texture2D(this.d3dDevice, description);

            using var renderTargetView = new SharpDX.Direct3D11.RenderTargetView(this.d3dDevice, this.blankTexture);
            this.d3dDevice.ImmediateContext.ClearRenderTargetView(renderTargetView, new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 1));
        }

        private void SetResult(Direct3D11CaptureFrame frame)
        {
            this.currentFrame = frame;
            this.frameEvent.Set();
        }

        private void Stop()
        {
            this.closedEvent.Set();
        }

        private void OnFrameArrived(Direct3D11CaptureFramePool sender, object args)
        {
            this.SetResult(sender.TryGetNextFrame());
        }

        private void OnClosed(GraphicsCaptureItem sender, object args)
        {
            var newItem = this.itemFactory();
            if (newItem == null)
            {
                this.Stop();
            }
            else
            {
                this.CleanupItem();
                this.CurrentItem = this.itemFactory();
                this.InitializeCapture();
            }
        }

        private void CleanupItem()
        {
            this.framePool?.Dispose();
            if (this.initializationThread == Thread.CurrentThread.ManagedThreadId)
            {
                this.session?.Dispose();
            }
            if (this.CurrentItem != null)
            {
                this.CurrentItem.Closed -= OnClosed;
            }
            this.CurrentItem = null;
            this.currentFrame?.Dispose();
        }

        public SurfaceWithInfo WaitForNewFrame()
        {
            // Let's get a fresh one.
            this.currentFrame?.Dispose();
            this.frameEvent.Reset();

            var signaledEvent = this.events[WaitHandle.WaitAny(this.events)];
            if (signaledEvent == this.closedEvent)
            {
                this.CleanupItem();
                return null;
            }

            using var multithreadLock = new MultithreadLock(this.multithread);
            using var sourceTexture = Direct3D11Helper.CreateSharpDXTexture2D(this.currentFrame.Surface);
            var description = sourceTexture.Description;
            description.Usage = SharpDX.Direct3D11.ResourceUsage.Default;
            description.BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource | SharpDX.Direct3D11.BindFlags.RenderTarget;
            description.CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None;
            description.OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None;

            using var copyTexture = new SharpDX.Direct3D11.Texture2D(this.d3dDevice, description);
            var width = Math.Clamp(this.currentFrame.ContentSize.Width, 0, this.currentFrame.Surface.Description.Width);
            var height = Math.Clamp(this.currentFrame.ContentSize.Height, 0, this.currentFrame.Surface.Description.Height);

            var region = new SharpDX.Direct3D11.ResourceRegion(0, 0, 0, width, height, 1);

            this.d3dDevice.ImmediateContext.CopyResource(blankTexture, copyTexture);
            this.d3dDevice.ImmediateContext.CopySubresourceRegion(sourceTexture, 0, region, copyTexture, 0);
            var result = new SurfaceWithInfo
            {
                SystemRelativeTime = this.currentFrame.SystemRelativeTime,
                Surface = Direct3D11Helper.CreateDirect3DSurfaceFromSharpDXTexture(copyTexture)
            };
            return result;
        }

        public void Dispose()
        {
            this.Stop();
            this.CleanupItem();
            this.device = null;
            this.d3dDevice = null;
            this.blankTexture?.Dispose();
            this.blankTexture = null;
        }
    }
}
