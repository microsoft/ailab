using System.IO;
using System.Reflection;

namespace SnipInsight.Forms.Tests.Common
{
    public static class ImageHelper
    {
        public static Stream GetHarryPotterImage() => GetImage("Harry_Potter.jpg");

        public static Stream GetToyStoryImage() => GetImage("toy-story.jpg");

        public static Stream GetCatImage() => GetImage("cat.png");

        public static Stream GetPlaymobilImage() => GetImage("playmobil.png");

        public static Stream GetHandWritenImage() => GetImage("texthandwritted.png");

        public static Stream GetHandWritenImage2() => GetImage("texthandWritted2.png");

        private static Stream GetImage(string filename)
        {
            var assembly = typeof(ImageHelper).GetTypeInfo().Assembly;
            var image = assembly.GetManifestResourceStream($"SnipInsight.Forms.Tests.Resources.{filename}");
            return image;
        }
    }
}
