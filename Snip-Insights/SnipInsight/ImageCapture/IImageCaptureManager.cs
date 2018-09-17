// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Windows.Media.Imaging;

namespace SnipInsight.ImageCapture
{
    internal delegate void CapturingDoneNotify();

    internal delegate void DrawSelectedAreaNotify(System.Drawing.Point ptCursor);

    public class ImageCaptureEventArgs : EventArgs
    {
        public ImageCaptureEventArgs(BitmapSource image)
        {
            Image = image;
        }

        public BitmapSource Image { get; private set; }
    }

    public interface IImageCaptureManager
    {
        void StartCapture();
        void CapturingDone();
        void CapturingCancel();
        event EventHandler<ImageCaptureEventArgs> CaptureCompleted;
    }
}
