using Lennox.LibYuvSharp;
using Microsoft.Azure.Kinect.Sensor;
using SharpDX;
using SharpDX.WIC;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Speaker.Recorder.Kinect
{
    public class DecodedKinectCaptureFrame : IDisposable
    {
        private readonly ImagingFactory imagingFactory;
        private readonly Image image;
        private BitmapDecoder bitmapDecoder;
        private WICStream stream;
        private BitmapSource lastGeneratedBitmap;
        private bool disposed;
        private BitmapFrameDecode frame;
        private FormatConverter formatConverter;

        public BitmapSource Bitmap
        {
            get
            {
                if (this.lastGeneratedBitmap == null && !this.disposed)
                {
                    lock (this.imagingFactory)
                    {
                        if (this.lastGeneratedBitmap == null && !this.disposed)
                        {
                            this.lastGeneratedBitmap = this.GetBitmapFromImage();
                            this.lastGeneratedBitmap.Disposed += this.LastGeneratedBitmap_Disposed;
                        }
                    }
                }

                return this.lastGeneratedBitmap;
            }
        }

        public KinectImageProvider Provider { get; }

        private void LastGeneratedBitmap_Disposed(object sender, EventArgs e)
        {
            this.lastGeneratedBitmap.Disposed -= this.LastGeneratedBitmap_Disposed;
            Interlocked.CompareExchange(ref this.lastGeneratedBitmap, null, (BitmapSource)sender);
        }

        [DllImport("k4a", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr k4a_image_get_buffer(IntPtr image_handle);

        public DecodedKinectCaptureFrame(Image image, KinectImageProvider provider)
        {
            this.imagingFactory = new ImagingFactory();
            this.image = image;
            Provider = provider;
        }

        private unsafe BitmapSource GetBitmapFromImage()
        {
            var captureData = k4a_image_get_buffer(this.image.Handle);
            var captureSize = (int)this.image.Size;

            switch (image.Format)
            {
                case ImageFormat.ColorMJPG:
                    {
                        this.bitmapDecoder = new BitmapDecoder(this.imagingFactory, ContainerFormatGuids.Jpeg);
                        this.stream = new WICStream(this.imagingFactory, new DataPointer(captureData, captureSize));
                        this.bitmapDecoder.Initialize(this.stream, DecodeOptions.CacheOnLoad);

                        this.frame = this.bitmapDecoder.GetFrame(0);
                        this.formatConverter = new FormatConverter(this.imagingFactory);
                        formatConverter.Initialize(frame, PixelFormat.Format32bppPBGRA, BitmapDitherType.None, null, 0.0, BitmapPaletteType.Custom);
                        return formatConverter;
                    }
                case ImageFormat.ColorBGRA32:
                    return new Bitmap(this.imagingFactory,
                        image.WidthPixels,
                        image.HeightPixels,
                        PixelFormat.Format32bppPBGRA,
                        new DataRectangle(captureData, image.StrideBytes),
                        captureSize);
                case ImageFormat.IR16:
                case ImageFormat.Depth16:
                    return new Bitmap(this.imagingFactory,
                        image.WidthPixels,
                        image.HeightPixels,
                        PixelFormat.Format16bppGrayFixedPoint,
                        new DataRectangle(captureData, image.StrideBytes),
                        captureSize);
                case ImageFormat.ColorNV12:
                    {
                        int textureWidth = this.image.WidthPixels;
                        int textureHeight = this.image.HeightPixels;
                        var imagePixels = textureWidth * textureHeight;
                        var nv12Span = new Span<byte>(captureData.ToPointer(), captureSize);
                        var nv12ySpan = nv12Span.Slice(0, imagePixels);
                        var nv12uvSpan = nv12Span.Slice(nv12ySpan.Length, (int)(imagePixels * 0.5));

                        var bitmap = new Bitmap(this.imagingFactory,
                            image.WidthPixels,
                            image.HeightPixels,
                            PixelFormat.Format32bppPBGRA,
                            BitmapCreateCacheOption.CacheOnLoad);
                        using (var bitmapLock = bitmap.Lock(BitmapLockFlags.Write))
                        {
                            var ptr = (byte*)bitmapLock.Data.DataPointer.ToPointer();
                            fixed (byte* yptr = nv12ySpan)
                            fixed (byte* uvptr = nv12uvSpan)
                            {
                                _ = LibYuv.NV12ToARGB(yptr, textureWidth, uvptr, textureWidth, ptr, textureWidth * 4, textureWidth, textureHeight);
                            }
                        }

                        return bitmap;
                    }
                case ImageFormat.ColorYUY2:
                    {
                        int textureWidth = this.image.WidthPixels;
                        int textureHeight = this.image.HeightPixels;
                        var imagePixels = textureWidth * textureHeight;
                        var yuv2Span = new Span<byte>(captureData.ToPointer(), captureSize);

                        var bitmap = new Bitmap(this.imagingFactory,
                            image.WidthPixels,
                            image.HeightPixels,
                            PixelFormat.Format32bppPBGRA,
                            BitmapCreateCacheOption.CacheOnLoad);
                        using (var bitmapLock = bitmap.Lock(BitmapLockFlags.Write))
                        {
                            var ptr = (byte*)bitmapLock.Data.DataPointer.ToPointer();
                            fixed (byte* yuv2ptr = yuv2Span)
                            {
                                _ = LibYuv.YUY2ToARGB(yuv2ptr, textureWidth * 2, ptr, textureWidth * 4, textureWidth, textureHeight);
                            }
                        }

                        return bitmap;
                    }
                case ImageFormat.Custom8:
                case ImageFormat.Custom16:
                case ImageFormat.Custom:
                default:
                    throw new NotImplementedException($"Format not implemented: {image.Format}.");
            }
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                this.frame?.Dispose();
                this.formatConverter?.Dispose();
                this.image?.Dispose();
                this.stream?.Dispose();
                this.bitmapDecoder?.Dispose();
                this.lastGeneratedBitmap?.Dispose();
                this.imagingFactory?.Dispose();
            }
        }
    }
}
