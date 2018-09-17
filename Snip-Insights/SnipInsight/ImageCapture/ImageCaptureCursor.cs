// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.Win32.SafeHandles;
using SnipInsight.Util;

namespace SnipInsight.ImageCapture
{
    /// <summary>
    /// See http://stackoverflow.com/questions/9218029/safefilehandle-close-throws-an-exception-but-the-handle-is-valid-and-works
    /// </summary>
    class SafeIconHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeIconHandle()
            : base(true)
        {
        }

        public SafeIconHandle(IntPtr hIcon)
            : this()
        {
            SetHandle(hIcon);
        }

        protected override bool ReleaseHandle()
        {
            return NativeMethods.DestroyIcon(this.handle);
        }
    }

    public class ImageCaptureCursor : IDisposable
    {
        private Cursor _cursor;
        private SafeIconHandle _safeHandle;

        public Cursor GetCursor()
        {
            if (_cursor == null)
            {
                // _cursor = CrossHairCursor(64, 64);
                _cursor = Cursors.None;
            }
            return _cursor;
        }

        private Cursor CrossHairCursor(int w, int h)
        {
            Pen pen = new Pen(Color.Red, 5);
            Pen thinPen = new Pen(Color.Red, 1);
            var pic = new Bitmap(w, h);
            var gr = Graphics.FromImage(pic);

            var pathXL = new GraphicsPath();
            var pathXR = new GraphicsPath();
            var pathX = new GraphicsPath();

            var pathYT = new GraphicsPath();
            var pathYB = new GraphicsPath();
            var pathY = new GraphicsPath();

            pathY.AddLine(new Point(w / 2, 0), new Point(w / 2, h));
            pathYT.AddLine(new Point(w / 2, h / 2 - 2 * 16), new Point(w / 2, h / 2 - 16));
            pathYB.AddLine(new Point(w / 2, h / 2 + 2 * 16), new Point(w / 2, h / 2 + 16));

            pathX.AddLine(new Point(0, h / 2), new Point(w, h / 2));
            pathXL.AddLine(new Point(w / 2 - 2 * 16, h / 2), new Point(w / 2 - 16, h / 2));
            pathXR.AddLine(new Point(w / 2 + 16, h / 2), new Point(w / 2 + 2 * 16, h / 2));

            gr.DrawPath(pen, pathXL);
            gr.DrawPath(pen, pathXR);
            gr.DrawPath(pen, pathYT);
            gr.DrawPath(pen, pathYB);

            gr.DrawPath(thinPen, pathX);
            gr.DrawPath(thinPen, pathY);
            var icon = Icon.FromHandle(pic.GetHicon());
            SafeIconHandle safeHandle = _safeHandle;
            if (safeHandle != null)
            {
                safeHandle.Dispose();
            }
            _safeHandle = new SafeIconHandle(icon.Handle);
            safeHandle = _safeHandle;
            return CursorInteropHelper.Create(safeHandle);
        }

        ~ImageCaptureCursor()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_cursor != null)
                {
                    _cursor.Dispose();
                    _cursor = null;
                }
                if (_safeHandle != null)
                {
                    _safeHandle.Dispose();
                    _safeHandle = null;
                }
            }
        }
    }
}
