// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using SnipInsight.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SnipInsight.Package
{
	/// <summary>
	/// Manager to maintain and handle snipInsights.
	/// </summary>
	internal class SnipInsightsManager
    {
        public event EventHandler<PackageArgs> ImageSaved;
        public event EventHandler<PackageArgs> ImageDeleted;

        private const string SnipInsightsFolder = "My Snips";

        private readonly string _snipInsightsDirectory;

        public SnipInsightsManager()
        {
            _snipInsightsDirectory = GetSnipInsightsDirectory();
        }

        public List<FileInfo> GetAllSnipInsightFileInfos()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(_snipInsightsDirectory);
            return directoryInfo.GetFiles().OrderByDescending(p => p.LastWriteTimeUtc).ToList();
        }

        public async Task<PackageData> GetPackageDataAsync(FileInfo file)
        {
            PackageData data = null;
            switch (file.Extension)
            {
                case ".png":
                    MemoryStream thumbnail = new MemoryStream();
                    using (var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                    {
                        await fileStream.CopyToAsync(thumbnail);
                    }
                    thumbnail.Position = 0;
                    data = new PackageData
					{
							Duration = 0,
							HasMedia = false,
							IsPackage = false,
							Url = file.FullName,
							Thumbnail = thumbnail
					};
                    break;
            }
            if (data != null)
            {
                data.LastWriteTime = file.LastAccessTime;
            }
            return data;
        }

        /// <summary>
        /// Deletes the given image file.
        /// </summary>
        public void DeleteImage(string imageFile)
        {
            if (File.Exists(imageFile))
            {
                File.Delete(imageFile);
                if (ImageDeleted != null)
                {
                    ImageDeleted(this, new PackageArgs { PackageUrl = imageFile });
                }
            }
        }

        /// <summary>
        /// Saves an image to the snipInsights.
        /// </summary>
        public string SaveImage(MemoryStream image)
        {
            // Create the mix file path.
            string file;
            do
            {
                file = Path.Combine(_snipInsightsDirectory, String.Format("capture{0}.{1}", DateTime.Now.ToString("yyyyMMddHHmmssfff"), "png"));
            } while (File.Exists(file));

            using (FileStream fs = new FileStream(file, FileMode.CreateNew))
            {
                image.CopyTo(fs);
            }

            SaveInCustomFolder(image);

            image.Position = 0;
            MemoryStream cloned = new MemoryStream();
            image.CopyTo(cloned);
            image.Position = 0;
            cloned.Position = 0;
            if (ImageSaved != null)
            {
                ImageSaved(this, new PackageArgs { PackageUrl = file, Thumbnail = cloned, Duration = 0, HasMedia = false });
            }
            return file;
        }

        /// <summary>
        /// Save the screenshot in the user's location of choice
        /// </summary>
        /// <param name="image"> The stream containing the screenshot</param>
        public void SaveInCustomFolder(MemoryStream image)
        {
            EnsureCustomFolderExists();

            if (UserSettings.CustomDirectory != null && UserSettings.CustomDirectory != _snipInsightsDirectory)
            {
                string file;
                do
                {
                    file = Path.Combine(UserSettings.CustomDirectory,
                        String.Format("capture{0}.{1}",
                        DateTime.Now.ToString("yyyyMMddHHmmssfff"), "png"));
                } while (File.Exists(file));

                using (FileStream fs = new FileStream(file, FileMode.CreateNew))
                {
                    if (image != null)
                    {
                        // Reset the pointer position to the start of stream
                        image.Position = 0;
                        // Write the memory in a new file
                        image.CopyTo(fs);
                    }
                }
            }
        }

        /// <summary>
        /// Ensure the user's custom directory still exists when saving
        /// In case of deletion pre-screenshot
        /// </summary>
        public void EnsureCustomFolderExists()
        {
            if (!Directory.Exists(UserSettings.CustomDirectory))
            {
                try
                {
                    Directory.CreateDirectory(UserSettings.CustomDirectory);
                }
                catch (Exception ex)
                {
                    // If it couldn't be created, we redirect to the default value
                    UserSettings.CustomDirectory = _snipInsightsDirectory;
                    Diagnostics.LogException(ex);
                }
            }
        }

        #region Helpers
        /// <summary>
        /// Get the snipInsights directory.
        /// </summary>
        /// <returns></returns>
        private string GetSnipInsightsDirectory()
        {
            string snipInsightsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), SnipInsightsFolder);
            if (!Directory.Exists(snipInsightsDirectory))
            {
                Directory.CreateDirectory(snipInsightsDirectory);
            }
            return snipInsightsDirectory;
        }
        #endregion
    }
    public class PackageArgs : EventArgs
    {
        public string PackageUrl { get; set; }
        public MemoryStream Thumbnail { get; set; }

        public ulong Duration { get; set; }

        public bool HasMedia { get; set; }
    }
}
