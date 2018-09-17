// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.IO;

namespace SnipInsight.Package
{
	public class PackageData : IDisposable
    {
        public PackageData()
        {

        }

        public PackageData(string url, MemoryStream thumbnail, ulong duration, bool hasMedia, bool isPackage)
        {
            Url = url;
            Thumbnail = thumbnail;
            Duration = duration;
            HasMedia = hasMedia;
            IsPackage = isPackage;
        }

        /// <summary>
        /// Url to the image or package.
        /// </summary>
        public string Url { get; set; }

        public MemoryStream Thumbnail { get; set; }

        /// <summary>
        /// Indicates if there is media (audio/video) in the package.
        /// </summary>
        public bool HasMedia { get; set; }

        /// <summary>
        /// Indicates if the URL is for mixp package or just png file.
        /// </summary>
        public bool IsPackage { get; set; }  // Can be removed in future if everything is a mixp package.

        public ulong Duration { get; set; }

        public DateTime LastWriteTime { get; set; }

        public string MixId { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Thumbnail != null)
                {
                    Thumbnail.Dispose();
                    Thumbnail = null;
                }
            }
        }
    }
}
