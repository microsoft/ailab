// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SnipInsight.Util;

namespace SnipInsight.ImageCapture
{
    internal static class ScreenCapture
    {
        /// <summary>
        /// Creates an Image object containing a screen shot of a specific window
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Bitmap CaptureWindowRegion(IntPtr hWnd, int x, int y, int width, int height)
        {
            Bitmap bmCapture = null;
            var dpiScale = DpiUtilities.GetSystemScale();
            if (width > 0 && height > 0)
            {
                IntPtr ptrWindowDc = NativeMethods.GetWindowDC(hWnd);
                bmCapture = new Bitmap(width, height,
                    System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                bmCapture.MakeTransparent();
                Graphics g = Graphics.FromImage(bmCapture);
                IntPtr hdcPtr = g.GetHdc();

                NativeMethods.BitBlt(hdcPtr, 0, 0, width, height, ptrWindowDc, x,
                    y, NativeMethods.TernaryRasterOperations.SRCCOPY | NativeMethods.TernaryRasterOperations.CAPTUREBLT);
                bmCapture.SetResolution((float)(96.0 * dpiScale.X), (float)(96.0 * dpiScale.Y));

                g.ReleaseHdc();
                NativeMethods.ReleaseDC(IntPtr.Zero, ptrWindowDc);
                g.Dispose();
            }
            return bmCapture;
        }

        public static BitmapSource GetBitmapSource(System.Drawing.Bitmap source)
        {
            if (source == null)
            {
                return null;
            }

            //
            //  The following conversion preserves the DPI of the original image.
            //

            BitmapSource bs = null;

            BitmapData data = source.LockBits(new System.Drawing.Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, source.PixelFormat);

            try
            {
                bs = BitmapSource.Create(source.Width, source.Height, source.HorizontalResolution, source.VerticalResolution, PixelFormats.Bgra32, null,
                                         data.Scan0, data.Stride * source.Height, data.Stride);
            }
            finally
            {
                source.UnlockBits(data);
            }

            return bs;
        }

        public static BitmapSource CaptureBmpFromImage(Bitmap bmpImage, Rectangle r, DpiScale dpiScale = null)
        {
            Bitmap srcImage = CropImage(bmpImage, r);
            if (dpiScale != null && dpiScale.X > 0.0 && dpiScale.Y > 0.0)
            {
                // This preserves the DPI from the original screen where the image
                // was captured.
                srcImage.SetResolution((float)(96.0 * dpiScale.X), (float)(96.0 * dpiScale.Y));
            }
            return GetBitmapSource(srcImage);
        }

        public static Bitmap CropImage(Bitmap bmpImage, Rectangle r)
        {
            Bitmap nb = new Bitmap(r.Width, r.Height);
            using (Graphics g = Graphics.FromImage(nb))
            {
                g.DrawImage(bmpImage, -r.X, -r.Y);
            }
            return nb;
        }

        public static Bitmap ResizeImage(Bitmap srcImage, int newWidth, int newHeight)
        {
            Bitmap newImage = new Bitmap(newWidth, newHeight);
            using (Graphics gr = Graphics.FromImage(newImage))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.DrawImage(srcImage, new Rectangle(0, 0, newWidth, newHeight));
            }
            return newImage;
        }
    }
}
