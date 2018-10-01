// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Drawing;

namespace SnipInsight.ImageCapture
{
    /// <summary>
    /// capture the image
    /// </summary>
    public class ImageCaptureManager : IImageCaptureManager, IDisposable
    {
        private ImageCaptureWindow _wCapture;

        private ImageCaptureCursor _imageCaptureCursor;

        private readonly AreaSelection _areaSelection;

        public ImageCaptureManager()
        {
            //    SetDpiAwareness();
            _areaSelection = new AreaSelection();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_wCapture != null)
                {
                    _wCapture.Close();
                    _wCapture = null;
                }
                if (_imageCaptureCursor != null)
                {
                    _imageCaptureCursor.Dispose();
                    _imageCaptureCursor = null;
                }
            }
        }

        ~ImageCaptureManager()
        {
            Dispose(false);
        }

        /// <summary>
        /// start capturing
        /// </summary>
        public void StartCapture()
        {
            Rectangle rctDeskTop = SmartBoundaryDetection.GetDesktopBounds();
            _imageCaptureCursor = new ImageCaptureCursor();
            var cursor = _imageCaptureCursor.GetCursor();

            _wCapture = new ImageCaptureWindow(rctDeskTop, _areaSelection);

            _wCapture.Cursor = cursor;
            _wCapture.NotifyCapturingDone += OnScreenCapturingDoneNotify;
            _wCapture.NotifyCapturingCancel += OnScreenCapturingCancelNotify;
            _wCapture.Show();
        }

        public void CapturingDone()
        {
            _wCapture.CapturingDone(_wCapture, null);
        }

        public void CapturingCancel()
        {
            _wCapture.CapturingCancel(_wCapture, null);
        }

        public event EventHandler<ImageCaptureEventArgs> CaptureCompleted;

        private void OnScreenCapturingDoneNotify(object sender, EventArgs args)
        {
            if (_wCapture != null)
            {
                _wCapture.Close();
            }

            var captureCompleted = CaptureCompleted;
            if (captureCompleted != null)
            {
                captureCompleted(this, (ImageCaptureEventArgs)args);
            }
        }

        private void OnScreenCapturingCancelNotify(object sender, EventArgs args)
        {
            if (_wCapture != null)
            {
                _wCapture.Close();
            }

            var captureCompleted = CaptureCompleted;
            if (captureCompleted != null)
            {
                captureCompleted(this, new ImageCaptureEventArgs(null));
            }
        }
    }
}
