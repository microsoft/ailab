#if MACOS
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AppKit;
using CoreGraphics;
using Foundation;
using ImageIO;
using MobileCoreServices;

namespace SnipInsight.Forms.GTK.Features.Snipping
{
    public static class MacOSScreenshotHelpers
    {
        private const string AppServicesPath =
            "/System/Library/Frameworks/ApplicationServices.framework/Versions/Current/ApplicationServices";

        private static readonly string TemporalPath = "/tmp";

        private static string imageFormat;
        private static bool isXamarinMacEnvironmentInitialized;

        public static void InitializeXamarinMacEnvironmentUnderMacOS()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return;
            }

            if (isXamarinMacEnvironmentInitialized)
            {
                return;
            }

            NSApplication.Init();
            imageFormat = UTType.PNG;

            isXamarinMacEnvironmentInitialized = true;
        }

        // Based on: https://stackoverflow.com/a/40864231
        public static List<(string path, CGRect bounds)> Take()
        {
            if (!isXamarinMacEnvironmentInitialized)
            {
                return null;
            }

            var result = CGGetActiveDisplayList(0, null, out int displayCount);

            // https://developer.apple.com/documentation/coregraphics/cgerror/success
            if (result != 0)
            {
                Console.WriteLine("Error retrieving the displays count.");
                return null;
            }

            // CGDirectDisplayID is an Int32: https://developer.apple.com/documentation/coregraphics/cgdirectdisplayid
            var activeDisplays = new int[displayCount];
            result = CGGetActiveDisplayList(displayCount, activeDisplays, out displayCount);

            if (result != 0)
            {
                Console.WriteLine("Error retrieving the displays list.");
                return null;
            }

            var screenshots = new List<(string path, CGRect bounds)>();

            for (int i = 0; i < displayCount; i++)
            {
                var displayId = activeDisplays[i];

                var handle = CGDisplayCreateImage(displayId);
                var screenShot = new CGImage(handle);

                var path = $"{TemporalPath}/screenshot-{i}.{imageFormat.ToLowerInvariant()}";
                Save(screenShot, path);

                var bounds = CGDisplayBounds(displayId);
                screenshots.Add((path, bounds));
            }

            return screenshots;
        }

        // https://developer.apple.com/documentation/coregraphics/1456395-cgdisplaybounds
        [DllImport(AppServicesPath, EntryPoint = "CGDisplayBounds")]
        private static extern CGRect CGDisplayBounds(int display);

        // https://developer.apple.com/documentation/coregraphics/1455691-cgdisplaycreateimage
        [DllImport(AppServicesPath, EntryPoint = "CGDisplayCreateImage")]
        private static extern /* CGImageRef */ IntPtr CGDisplayCreateImage(int displayId);

        // https://developer.apple.com/documentation/coregraphics/1454603-cggetactivedisplaylist
        [DllImport(AppServicesPath, EntryPoint = "CGGetActiveDisplayList")]
        private static extern int CGGetActiveDisplayList(int maxDisplays, int[] activeDisplays, out int displayCount);

        private static void Save(CGImage screenshot, string path)
        {
            var fileURL = new NSUrl(path, false);

            using (var dataConsumer = new CGDataConsumer(fileURL))
            {
                var imageDestination = CGImageDestination.Create(dataConsumer, imageFormat, 1);
                imageDestination.AddImage(screenshot);
                imageDestination.Close();
            }
        }
    }
}
#endif
