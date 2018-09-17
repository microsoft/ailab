// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SnipInsight.Conversion
{
	internal static class PictureConverter
    {
        internal const int ThumbnailWidth = 320;
        internal const int ThumbnailHeight = 240;

        /// <summary>
        /// Generate snapshot of WPF UI elements
        /// which may be used to get image+ink as currently shown on the screen
        /// This function can be used independenly of any recording
        /// </summary>
        /// <param name="baseElement"></param>
        /// <param name="overlayElement"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static BitmapSource GenerateSnapshot(UIElement baseElement, UIElement overlayElement, double scale=1)
        {
            Size requestedSize = new Size(baseElement.RenderSize.Width * scale, baseElement.RenderSize.Height * scale);

            // Render the content
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)requestedSize.Width, (int)requestedSize.Height, 96d, 96d, PixelFormats.Pbgra32);
            VisualBrush baseElementBrush = new VisualBrush(baseElement);
            VisualBrush overlayElementBrush = new VisualBrush(overlayElement);

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.PushTransform(new ScaleTransform(scale, scale));
                drawingContext.DrawRectangle(baseElementBrush, null, new Rect(new Point(0, 0), new Point(baseElement.RenderSize.Width, baseElement.RenderSize.Height)));
                drawingContext.DrawRectangle(overlayElementBrush, null, new Rect(new Point(0, 0), new Point(baseElement.RenderSize.Width, baseElement.RenderSize.Height)));
            }

            renderTargetBitmap.Render(drawingVisual);
            return renderTargetBitmap;
        }


        public static T SaveToPng<T>(BitmapSource bitmap, T outputStream) where T:Stream
        {
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            encoder.Save(outputStream);
            return outputStream;
        }

        public static T SaveToJpg<T>(BitmapSource bitmap, T outputStream, int quality = 95) where T:Stream
        {
            JpegBitmapEncoder jpgEncoder = new JpegBitmapEncoder();
            jpgEncoder.QualityLevel = quality;
            jpgEncoder.Frames.Add(BitmapFrame.Create(bitmap));
            jpgEncoder.Save(outputStream);
            return outputStream;
        }

        public static T SaveToBmp<T>(BitmapSource bitmap, T outputStream) where T : Stream
        {
            BmpBitmapEncoder bmpEncoder = new BmpBitmapEncoder();
            bmpEncoder.Frames.Add(BitmapFrame.Create(bitmap));
            bmpEncoder.Save(outputStream);
            return outputStream;
        }
    }
}
