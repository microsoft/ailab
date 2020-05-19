using Microsoft.Extensions.Logging;
using Speaker.Recorder.Capture;
using Speaker.Recorder.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Windows.Graphics.Capture;

namespace Speaker.Recorder.PowerPoint
{
    public class PowerPointGraphicsCaptureItemsFactory
    {
        private readonly Dictionary<string, (int count, GraphicsCaptureItem graphicsCaptureItem, Process process)> currentCaptureItems = new Dictionary<string, (int, GraphicsCaptureItem, Process)>();
        private readonly ILogger<PowerPointGraphicsCaptureItemsFactory> logger;

        public PowerPointGraphicsCaptureItemsFactory(ILogger<PowerPointGraphicsCaptureItemsFactory> logger)
        {
            this.logger = logger;
        }

        public void Remove(string path)
        {
            if (path != null && currentCaptureItems.TryGetValue(path, out var current))
            {
                current.count--;
                this.logger.LogInformation($"PowerPointProvider for {path} count decremented {current.count}");
                if (current.count <= 0)
                {
                    this.logger.LogInformation($"Removing PowerPointProvider for {path} and killing process");
                    currentCaptureItems.Remove(path);
                    current.process.Kill();
                }
                else
                {
                    currentCaptureItems[path] = current;
                }
            }
        }

        public void SendKey(Key key)
        {
            foreach (var item in currentCaptureItems.Values)
            {
                WindowHelper.SendKey(item.process.MainWindowHandle, new IntPtr(KeyInterop.VirtualKeyFromKey(key)));
            }
        }

        public void CloseAll()
        {
            foreach (var item in currentCaptureItems.Values)
            {
                try
                {
                    item.process.Kill();
                }
                catch { }
            }
        }

        public GraphicsCaptureItem GetOrCreateGraphicsCaptureItem(string path)
        {
            if (path != null)
            {
                try
                {
                    if (!currentCaptureItems.TryGetValue(path, out var current))
                    {
                        this.logger.LogInformation($"Creating PowerPointProvider for {path}");
                        var process = PowerPointHelper.StartPowerPoint(path);
                        var size = WindowHelper.GetWindowSize(process.MainWindowHandle);
                        WindowHelper.SetWindowPosition(process.MainWindowHandle, new Rect(new Point(size.Width - 1, size.Height - 1), new Size(1920, 1080)));
                        WindowHelper.SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
                        GraphicsCaptureItem item = CaptureHelper.CreateItemForWindow(process.MainWindowHandle);
                        item.Closed += Item_Closed;
                        current = (0, item, process);

                        void Item_Closed(GraphicsCaptureItem sender, object args)
                        {
                            sender.Closed -= Item_Closed;
                            currentCaptureItems.Remove(path);
                        }
                    }

                    current.count++;
                    this.logger.LogInformation($"PowerPointProvider for {path} count incremented {current.count}");
                    currentCaptureItems[path] = current;
                    return current.graphicsCaptureItem;

                }
                catch (Exception e)
                {
                    this.logger.LogInformation(e, $"Error creating PowerPointProvider for {path}");
                }
            }

            return null;
        }
    }
}
