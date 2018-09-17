// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using SnipInsight.Conversion;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace SnipInsight.Util
{
    public static class ImageUtils
    {
        private const int OverlayHeight = 128;
        private const int OverlayWidth = 128;

        /// <summary>
        /// Overlay the captured image with play button.
        /// </summary>
        public static string OverlayImageWithPlayButton(BitmapSource source, out int outputWidth, out int outputHeight)
        {
            try
            {
                string result = Path.Combine(Path.GetTempPath(), string.Format("cNImage{0}.png", DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss")));
                string overlayUrl = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Play_360x200.png");

                using (MemoryStream ms = new MemoryStream())
                using (Image image = new Bitmap(PictureConverter.SaveToPng(source, ms)))
                {
                    // Use aspect ratio to determine size of output image. If source image is wider than it is tall,
                    // then we set max width to 320. If it is taller than it is wide, we set max height to 240.
                    double aspectRatio = (double)image.Width / image.Height;
                    if (aspectRatio > 1)
                    {
                        outputWidth = 320;
                        outputHeight = (int)(320 / aspectRatio);
                    }
                    else
                    {
                        outputHeight = 240;
                        outputWidth = (int)(240 * aspectRatio);
                    }

                    int sourceHeight = outputHeight;
                    int sourceWidth = outputWidth;

                    // If source image is too narrow, ensure the output image is large enough to fit the play button
                    // (which is 128x128) and some buffer space.
                    if (outputWidth < 150)
                    {
                        outputWidth = 150;
                    }
                    if (outputHeight < 150)
                    {
                        outputHeight = 150;
                    }

                    using (var bitmap = new Bitmap(outputWidth, outputHeight))
                    using (var canvas = Graphics.FromImage(bitmap))
                    {
                        canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;

                        canvas.DrawImage(image, new Rectangle((outputWidth - sourceWidth) / 2, (outputHeight - sourceHeight) / 2, sourceWidth, sourceHeight), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);

                        // Try and combine both the images together.
                        if (File.Exists(overlayUrl))
                        {
                            int overlayX = (int)((double)(outputWidth - OverlayWidth) / 2);
                            int overlayY = (int)((double)(outputHeight - OverlayHeight) / 2);

                            using (Image overlay = Image.FromFile(overlayUrl))
                            {
                                canvas.DrawImage(overlay, new Rectangle(overlayX, overlayY, OverlayWidth, OverlayHeight));
                            }
                        }

                        canvas.Save();
                        bitmap.Save(result, ImageFormat.Png);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                outputWidth = 340;
                outputHeight = 240;
                Diagnostics.LogException(ex);
                return null;
            }
        }
    }
}
