// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SnipInsight.Package
{
    using System;
    using System.IO;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Represents a link to a SnipInsight.
    /// </summary>
    public class SnipInsightLink : IDisposable, INotifyPropertyChanged
    {
        private MemoryStream _imageStream;

		~SnipInsightLink()
        {
            Dispose(true);
        }

		public BitmapSource ThumbnailImage { get; private set; }

		/// <summary>
		/// SnipInsight path.
		/// </summary>
		public string Url { get; set; }

        public MemoryStream ImageStream
        {
            get
            {
                return _imageStream;
            }
            set
            {
                if (_imageStream != null)
                {
                    _imageStream.Dispose();
                    _imageStream = null;
                }
                _imageStream = value;
                if (value != null)
                {
                    ThumbnailImage = CreateBitmapSource(value, 112);
                }
            }
        }

        /// <summary>
        /// Indicates if there is media (audio/video) in the package.
        /// </summary>
        public bool HasMedia { get; set; }

        /// <summary>
        /// Indicates if the URL is for mixp package or just png file.
        /// </summary>
        public bool HasPackage { get; set; }  // Can be removed in future if everything is a mixp package.

        public ulong Duration { get; set; }

        private string _mixId;

        public string MixId
        {
            get { return _mixId; }
            set
            {
                if (_mixId == value) return;
                _mixId = value;
                OnPropertyChanged("MixId");
            }
        }

        /// <summary>
        /// True if there is a pending deletion for this item. Helps to avoid multiple deletion without complexity of disabling and enabling close controls.
        /// </summary>
        public bool DeletionPending
        {
            get { return _deletionPending; }
            set
            {
                if (_deletionPending != value)
                {
                    _deletionPending = value;
                    OnPropertyChanged("DeletionPending");
                }
            }
        }
        private bool _deletionPending;

        public string LeftCaption
        {
            get
            {
                var dateString = LastWriteTime.ToString("MMMM dd, yyyy hh:mm"); //TODO: Need to add to resoure since date format can be different in other countries.
                return dateString;
            }
        }

        public string RightCaption
        {
            get
            {
                string durationString = string.Empty;
                if (Duration > 0)
                {
                    durationString = " " + TimeSpan.FromMilliseconds(Duration).ToString(@"mm\:ss", null); // TODO: Need to add to resource.
                }
                return durationString;
            }
        }

        public DateTime LastWriteTime { get; set; }

        #region Time Grouping

        /// <summary>
        /// Gets the text label used for grouping items by date.
        /// </summary>
        /// <value>
        /// A value that can be used for grouping items by date.
        /// </value>
        public string TimeGroupingLabel
        {
            get
            {
                DateTime now = DateTime.Now;

                DateTime time = LastWriteTime;

                if (time.Year == now.Year)
                {
                    if (time.Month == now.Month && time.Day == now.Day)
                    {
                        return "Today";
                    }
                    else if (time >= now.AddDays(-7).Date)
                    {
                        return "Last 7 days";
                    }
                    else
                    {
                        // If it's the current year, just return month
                        return LastWriteTime.ToString("MMMM");
                    }
                }
                else
                {
                    return LastWriteTime.ToString("MMMM yyyy");
                }
            }
        }

        #endregion

        public static BitmapSource CreateBitmapSource(Stream stream, int? maxHeight = null)
        {
            BitmapImage bitmapImage = null;
            if (stream != null)
            {
                stream.Position = 0;
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                if (maxHeight.HasValue)
                    bitmapImage.DecodePixelHeight = maxHeight.Value;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.StreamSource.Position = 0;
                bitmapImage.EndInit();
                if (bitmapImage.CanFreeze)
                {
                    bitmapImage.Freeze();
                }
            }
            return bitmapImage;
        }

        public override bool Equals(object obj)
        {
            var other = obj as SnipInsightLink;
            if (other != null && other.Url == Url)
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Url.GetHashCode();
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
                ThumbnailImage = null;
                if (_imageStream != null)
                {
                    _imageStream.Dispose();
                    _imageStream = null;
                }
            }
        }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
