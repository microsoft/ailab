// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Drawing;
using System.Windows.Media.Imaging;
using SnipInsight.Util;
using Rectangle = System.Drawing.Rectangle;

namespace SnipInsight.ImageCapture
{
    /// <summary>
    /// This class is to manage the screen snapshot of the time that user clicks image capture button
    /// </summary>
    internal class ScreenshotImage : IDisposable
    {
        private Rectangle _rectangleCaptureImage;
        private Bitmap _screenSnapshot;
        private double _screenScalor;

        public Bitmap ScreenSnapshotImage
        {
            get { return _screenSnapshot; }
        }

        public ScreenshotImage()
        {
        }

        public void Dispose()
        {
            if (_screenSnapshot != null)
            {
                _screenSnapshot.Dispose();
            }
        }

        ~ScreenshotImage()
        {
            if (_screenSnapshot != null)
            {
                _screenSnapshot.Dispose();
            }
        }

        public void SnapShot(Rectangle rectangle, double screenScalor)
        {
            //var screenProps = new ScreenProperties();
            //screenProps.GetMonitorsInformation();
            //var maxRectangle = screenProps.GetMaxRectangleFromMonitors();
            //_rectangleCaptureImage = maxRectangle;

            _screenScalor = screenScalor;
            _rectangleCaptureImage = new Rectangle(
                (int)(rectangle.Left * _screenScalor),
                (int)(rectangle.Top * _screenScalor),
                (int)(rectangle.Width * _screenScalor),
                (int)(rectangle.Height * _screenScalor)
                );

            var bmDesktop = ScreenCapture.CaptureWindowRegion(IntPtr.Zero, _rectangleCaptureImage.Left, _rectangleCaptureImage.Top,
                _rectangleCaptureImage.Width, _rectangleCaptureImage.Height);
            _screenSnapshot = bmDesktop;
        }

        public BitmapSource GetCaptureImage(NativeMethods.RECT rect, DpiScale dpiScaleOfSourceWindow)
        {
            if (_screenScalor != 1)
            {
                DpiScale adjustedScale = new DpiScale(dpiScaleOfSourceWindow.X * _screenScalor, dpiScaleOfSourceWindow.Y * _screenScalor);
                dpiScaleOfSourceWindow = adjustedScale;
            }

            // crop the invisible area
            var left = rect.left * _screenScalor < _rectangleCaptureImage.Left ? _rectangleCaptureImage.Left : rect.left * _screenScalor;
            var top = rect.top * _screenScalor < _rectangleCaptureImage.Top ? _rectangleCaptureImage.Top : rect.top * _screenScalor;
            var right = rect.right * _screenScalor > _rectangleCaptureImage.Right ? _rectangleCaptureImage.Right : rect.right * _screenScalor;
            var bottom = rect.bottom * _screenScalor > _rectangleCaptureImage.Bottom ? _rectangleCaptureImage.Bottom : rect.bottom * _screenScalor;

            var height = bottom - top;
            var width = right - left;

            left = left  - _rectangleCaptureImage.Left;
            top = top - _rectangleCaptureImage.Top;


            if (left < 0)
            {
                width = width + left;
                left = 0;
            }

            if (top < 0)
            {
                height = height + top;
                top = 0;
            }

            Rectangle rectangle = new Rectangle(
                (int)left,
                (int)top,
                (int)width,
                (int)height
                );

            return ScreenCapture.CaptureBmpFromImage(_screenSnapshot, rectangle, dpiScaleOfSourceWindow);
        }
    }
}
