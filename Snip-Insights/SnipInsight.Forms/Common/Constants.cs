using System.IO;

namespace SnipInsight.Forms.Common
{
    public static class Constants
    {
        public const string ScreenshotExtension = "jpeg";

        public const string Title = "Snip Insights";

        public static readonly string IconPath = Path.Combine("Resources", "Icons", "SnipInsights.ico");

        public static readonly string LogoPath = Path.Combine("Resources", "Images", "Logo.png");

        public static readonly string[] ValidExtensions = { "*.png", "*.jpg", "*.jpeg", "*.bmp" };

        public static readonly string UnknownLanguage = "unk";

        public static readonly string TemporalImageFilename = "temp.dat";
    }
}