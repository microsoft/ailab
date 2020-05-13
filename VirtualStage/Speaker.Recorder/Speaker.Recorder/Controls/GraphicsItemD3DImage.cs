using SharpDX.Direct3D11;
using SharpDX.Direct3D9;
using SharpDX.DXGI;
using Speaker.Recorder.Capture;
using System;
using System.Windows;
using System.Windows.Interop;
using Windows.Graphics;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;

namespace Speaker.Recorder.Controls
{
    public class GraphicsItemD3DImage : D3DImage
    {
        private GraphicsCaptureItem item;
        private Texture2DDescription renderTargetDescription;
        private Direct3D11CaptureFramePool framePool;
        private GraphicsCaptureSession session;
        private SizeInt32 lastSize;
        private IDirect3DDevice device;
        private Direct3DEx direct3d;
        private DeviceEx d9device;
        private SharpDX.Direct3D11.Device d3dDevice;
        private SharpDX.Direct3D11.Texture2D renderTarget;
        private bool backBufferSetted;
        private long fpsCount;

        public int FpsDivider { get; set; } = 1;

        public double RenderWidth { get; set; } = -1;

        public double RenderHeight { get; set; } = -1;

        public GraphicsItemD3DImage()
        {
            if (App.Current.MainWindow != null)
            {
                IntPtr window = new WindowInteropHelper(App.Current.MainWindow).Handle;
                var presentparams = new SharpDX.Direct3D9.PresentParameters
                {
                    Windowed = true,
                    SwapEffect = SharpDX.Direct3D9.SwapEffect.Discard,
                    DeviceWindowHandle = window,
                    PresentationInterval = PresentInterval.Immediate,
                };

                const CreateFlags deviceFlags = CreateFlags.HardwareVertexProcessing | CreateFlags.Multithreaded | CreateFlags.FpuPreserve;

                this.direct3d = new Direct3DEx();
                this.device = Direct3D11Helper.CreateDevice();
                this.d3dDevice = Direct3D11Helper.CreateSharpDXDevice(this.device);
                this.d9device = new DeviceEx(this.direct3d, 0, DeviceType.Hardware, IntPtr.Zero, deviceFlags, presentparams);
                this.IsFrontBufferAvailableChanged += this.GraphicItemD3DImageIsFrontBufferAvailableChanged;
            }
        }

        ~GraphicsItemD3DImage()
        {
            this.ResetState(false);
            this.renderTarget?.Dispose();
            this.d3dDevice?.Dispose();
            this.device?.Dispose();
        }

        public void SetGraphicItem(GraphicsCaptureItem item)
        {
            this.ResetState();
            this.item = item;

            if (this.item != null)
            {
                this.renderTargetDescription = new Texture2DDescription
                {
                    CpuAccessFlags = CpuAccessFlags.None,
                    Width = item.Size.Width,
                    Height = item.Size.Height,
                    Usage = ResourceUsage.Default,
                    Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                    ArraySize = 1,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    OptionFlags = ResourceOptionFlags.Shared,
                    MipLevels = 1,
                    SampleDescription = new SampleDescription(1, 0),
                };
                this.renderTarget = new Texture2D(d3dDevice, this.renderTargetDescription);

                framePool = Direct3D11CaptureFramePool.Create(device, DirectXPixelFormat.B8G8R8A8UIntNormalized, 2, this.item.Size);
                session = framePool.CreateCaptureSession(this.item);
                lastSize = this.item.Size;

                framePool.FrameArrived += this.OnFrameArrived;
                session.StartCapture();
            }
        }

        private void ResetState(bool resetBackBuffer = true)
        {
            this.backBufferSetted = false;
            this.session?.Dispose();
            if (this.framePool != null)
            {
                this.framePool.FrameArrived -= this.OnFrameArrived;
                this.framePool.Dispose();
            }

            if (resetBackBuffer)
            {
                this.Lock();
                this.SetBackBuffer(D3DResourceType.IDirect3DSurface9, IntPtr.Zero, true);
                this.Unlock();
            }
        }

        private void OnFrameArrived(Direct3D11CaptureFramePool sender, object args)
        {
            this.RequestRender();
        }

        private void GraphicItemD3DImageIsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.RequestRender();
        }

        public void RequestRender()
        {
            this.fpsCount++;
            using var frame = this.framePool.TryGetNextFrame();
            if (frame != null && this.IsFrontBufferAvailable && this.fpsCount % this.FpsDivider == 0)
            {
                if (this.TryLock(new Duration(TimeSpan.Zero)))
                {
                    var newSize = false;
                    if (frame.ContentSize.Width != lastSize.Width ||
                        frame.ContentSize.Height != lastSize.Height)
                    {
                        // The thing we have been capturing has changed size.
                        // We need to resize the swap chain first, then blit the pixels.
                        // After we do that, retire the frame and then recreate the frame pool.
                        newSize = true;
                        lastSize = frame.ContentSize;
                        this.renderTargetDescription.Width = lastSize.Width;
                        this.renderTargetDescription.Height = lastSize.Height;
                        this.renderTarget?.Dispose();
                        this.renderTarget = new Texture2D(d3dDevice, this.renderTargetDescription);
                        backBufferSetted = false;
                    }

                    using (var bitmap = Direct3D11Helper.CreateSharpDXTexture2D(frame.Surface))
                    {
                        d3dDevice.ImmediateContext.CopyResource(bitmap, this.renderTarget);
                    }

                    if (newSize || !backBufferSetted)
                    {
                        using var resource = this.renderTarget.QueryInterface<SharpDX.DXGI.Resource>();
                        var handle = resource.SharedHandle;

                        using var texture = new Texture(
                            this.d9device,
                            this.renderTarget.Description.Width,
                            this.renderTarget.Description.Height,
                            1,
                            SharpDX.Direct3D9.Usage.RenderTarget,
                            SharpDX.Direct3D9.Format.A8R8G8B8,
                            Pool.Default,
                            ref handle);
                        using var surface = texture.GetSurfaceLevel(0);
                        this.SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativePointer, true);
                        backBufferSetted = true;
                    }

                    this.AddDirtyRect(new Int32Rect(0, 0, lastSize.Width, lastSize.Height));

                    if (newSize)
                    {
                        framePool.Recreate(device, DirectXPixelFormat.B8G8R8A8UIntNormalized, 2, lastSize);
                    }

                }

                this.Unlock();
            }
        }
    }
}
