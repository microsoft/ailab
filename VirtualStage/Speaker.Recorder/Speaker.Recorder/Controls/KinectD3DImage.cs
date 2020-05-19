using System;
using System.Windows;
using System.Windows.Interop;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Direct3D9;
using SharpDX.DXGI;
using SharpDX.WIC;
using Windows.Graphics;
using Windows.Graphics.DirectX.Direct3D11;
using Speaker.Recorder.Kinect;
using System.Threading;
using System.Threading.Tasks;

namespace Speaker.Recorder.Controls
{
    public class KinectD3DImage : D3DImage, IDisposable
    {
        private Texture2DDescription renderTargetDescription;
        private SizeInt32 lastSize;
        private IDirect3DDevice device;
        private Direct3DEx direct3d;
        private ImagingFactory imagingFactory;
        private DeviceEx d9device;
        private SharpDX.Direct3D11.Device d3dDevice;
        private SharpDX.Direct3D11.Texture2D cpuRenderTarget;
        private SharpDX.Direct3D11.Texture2D renderTarget;
        private bool backBufferSetted;

        private KinectImageProvider provider;
        private ManualResetEventSlim renderEvent;
        private DecodedKinectCaptureFrame lastFrame;
        private bool disposed;
        private long fpsCount;

        public int FpsDivider { get; set; } = 1;

        public int RenderWidth { get; set; } = -1;

        public int RenderHeight { get; set; } = -1;

        public KinectD3DImage()
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
                this.imagingFactory = new ImagingFactory();
                this.device = Capture.Direct3D11Helper.CreateDevice();
                this.d3dDevice = Capture.Direct3D11Helper.CreateSharpDXDevice(this.device);
                this.d9device = new DeviceEx(this.direct3d, 0, DeviceType.Hardware, IntPtr.Zero, deviceFlags, presentparams);
                this.IsFrontBufferAvailableChanged += this.GraphicItemD3DImageIsFrontBufferAvailableChanged;
                this.renderEvent = new ManualResetEventSlim(false);
                var renderThread = new Thread(this.RenderThread);
                renderThread.Start();
            }
        }

        public void SetProvider(KinectImageProvider provider)
        {
            this.ResetState();

            this.provider = provider;
            if (this.provider != null)
            {
                this.provider.FrameArrived += this.OnFrameArrived;
            }
        }

        private void RenderThread(object obj)
        {
            var providerParam = new KinectImageProvider[1];
            try
            {
                while (!this.Dispatcher.HasShutdownStarted && !this.disposed)
                {
                    try
                    {
                        if (this.renderEvent.Wait(100))
                        {
                            this.renderEvent.Reset();

                            if (this.disposed)
                            {
                                return;
                            }

                            using var frame = Interlocked.Exchange(ref this.lastFrame, null);
                            if (frame != null && frame.Bitmap != null)
                            {
                                var frameSize = frame.Bitmap.Size;
                                var ratio = 1d;
                                if (this.RenderWidth > 0 && frameSize.Width > frameSize.Height)
                                {
                                    ratio = (double)this.RenderWidth / frameSize.Width;
                                }

                                if (this.RenderHeight > 0 && frameSize.Height > frameSize.Width)
                                {
                                    ratio = (double)this.RenderHeight / frameSize.Height;
                                }

                                ratio = ratio > 1 ? 1 : ratio;

                                var desiredSize = new Size2((int)(frameSize.Width * ratio), (int)(frameSize.Height * ratio));
                                if (this.renderTarget == null || this.renderTargetDescription.Width != desiredSize.Width || this.renderTargetDescription.Height != desiredSize.Height)
                                {
                                    this.renderTarget?.Dispose();
                                    this.cpuRenderTarget?.Dispose();
                                    this.CreateRenderTargets(desiredSize);
                                }

                                using var scale = new BitmapScaler(this.imagingFactory);
                                scale.Initialize(frame.Bitmap, desiredSize.Width, desiredSize.Height, BitmapInterpolationMode.Linear);
                                using var bitmap = new Bitmap(this.imagingFactory, scale, BitmapCreateCacheOption.NoCache);
                                using var bitmapLock = bitmap.Lock(BitmapLockFlags.Read);
                                DataBox imageDataBox = new DataBox(bitmapLock.Data.DataPointer, bitmapLock.Data.Pitch, bitmapLock.Data.Pitch * bitmapLock.Size.Height);
                                lock (d3dDevice)
                                {
                                    if (!this.d3dDevice.IsDisposed)
                                    {
                                        d3dDevice.ImmediateContext.UpdateSubresource(imageDataBox, this.cpuRenderTarget);
                                        d3dDevice.ImmediateContext.CopyResource(this.cpuRenderTarget, this.renderTarget);
                                        d3dDevice.ImmediateContext.Flush();
                                    }
                                }

                                if (!this.Dispatcher.HasShutdownStarted && !this.disposed)
                                {
                                    providerParam[0] = frame.Provider;
                                    this.Dispatcher.Invoke((InvokeRenderWithProvider)this.RequestRenderForProvider, providerParam);
                                }
                            }
                        }
                    }
                    catch (Exception e) when (!(e is OperationCanceledException))
                    {
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private delegate void InvokeRenderWithProvider(KinectImageProvider provider);

        private void RequestRenderForProvider(KinectImageProvider provider)
        {
            if (this.provider == provider)
            {
                this.RequestRender();
            }
        }

        private void OnFrameArrived(object sender, EventArgs e)
        {
            this.fpsCount++;
            if (this.fpsCount % this.FpsDivider != 0)
            {
                return;
            }

            var provider = this.provider;
            if (provider != null)
            {
                var old = Interlocked.Exchange(ref this.lastFrame, provider.TryGetLastFrame());
                old?.Dispose();
                this.renderEvent.Set();
            }
        }

        private void CreateRenderTargets(Size2 size)
        {
            this.lastSize = new SizeInt32 { Width = size.Width, Height = size.Height };
            this.renderTargetDescription = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.None,
                Width = this.lastSize.Width,
                Height = this.lastSize.Height,
                Usage = ResourceUsage.Default,
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                ArraySize = 1,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                OptionFlags = ResourceOptionFlags.Shared,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
            };
            this.renderTarget = new Texture2D(d3dDevice, this.renderTargetDescription);
            var cpuRenderTargetDescription = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Write,
                Width = this.lastSize.Width,
                Height = this.lastSize.Height,
                Usage = ResourceUsage.Default,
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                ArraySize = 1,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
            };
            this.cpuRenderTarget = new Texture2D(d3dDevice, cpuRenderTargetDescription);
            this.backBufferSetted = false;
        }

        private void ResetState(bool resetBackBuffer = true)
        {
            this.backBufferSetted = false;
            if (this.provider != null)
            {
                this.provider.FrameArrived -= this.OnFrameArrived;
            }

            if (resetBackBuffer)
            {
                this.Lock();
                this.SetBackBuffer(D3DResourceType.IDirect3DSurface9, IntPtr.Zero, true);
                this.Unlock();
            }
        }

        private void GraphicItemD3DImageIsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.RequestRender();
        }

        public void RequestRender()
        {
            if (this.IsFrontBufferAvailable && !this.disposed)
            {
                if (this.TryLock(new Duration(TimeSpan.Zero)))
                {
                    if (!this.backBufferSetted)
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
                        this.backBufferSetted = true;
                    }

                    this.AddDirtyRect(new Int32Rect(0, 0, lastSize.Width, lastSize.Height));

                }

                this.Unlock();
            }
        }

        public void Dispose()
        {
            this.disposed = true;
            this.renderEvent?.Set();
            if (this.d3dDevice != null)
            {
                lock (this.d3dDevice)
                {
                    this.ResetState(false);
                    this.renderTarget?.Dispose();
                    this.d3dDevice?.Dispose();
                    this.device?.Dispose();
                }
            }
        }
    }
}