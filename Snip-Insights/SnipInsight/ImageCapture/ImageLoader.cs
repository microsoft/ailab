// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SnipInsight.ImageCapture
{
    /// <summary>
    /// Loading a bitmap image.
    /// </summary>
    class ImageLoader
    {
        /// <summary>
        /// Loading a bitmap image from URL handles async and sync loading
        /// </summary>
        /// <param name="uri">url as a string</param>
        /// <returns>task object with result of BitmapImage if successful, else exception</returns>
        public static Task<BitmapImage> LoadFromUrl(Uri uri)
        {
            var tcs = new TaskCompletionSource<BitmapImage>();

            var bitmap = new BitmapImage();

            bitmap.DownloadCompleted += (object sender, EventArgs e) =>
            {
                tcs.TrySetResult(bitmap);
            };

            bitmap.DownloadFailed += (object sender, System.Windows.Media.ExceptionEventArgs e) =>
            {
                tcs.TrySetException(e.ErrorException);
            };

            bitmap.BeginInit();
            bitmap.UriSource=uri;
            bitmap.EndInit();

            if (!bitmap.IsDownloading)
            {
                tcs.TrySetResult(bitmap);
            }

            return tcs.Task;
        }
    }
}