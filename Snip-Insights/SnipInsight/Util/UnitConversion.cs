// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace SnipInsight.Util
{
    using System.Windows;

    public static class UnitConversion
    {
        /// <summary>
        /// Convert to EMUs from pixels.
        /// </summary>
        static public Size ConvertToEMU(Size input)
        {
            double dx = 96;
            PresentationSource source = PresentationSource.FromVisual(AppManager.TheBoss.MainWindow);
            if (source != null)
            {
                dx = 96.0 * source.CompositionTarget.TransformToDevice.M11;
            }
            double width = (input.Width) * 12700;
            double height = (input.Height) * 12700;
            return new Size(width, height);
        }

        /// <summary>
        /// Convert to EMUs from pixels.
        /// </summary>
        static public Rect ConvertToEMU(Rect input)
        {
            double dx = 96;
            PresentationSource source = PresentationSource.FromVisual(AppManager.TheBoss.MainWindow);
            if (source != null)
            {
                dx = 96.0 * source.CompositionTarget.TransformToDevice.M11;
            }
            double x = (input.X) * 12700;
            double y = (input.Y) * 12700;
            double width = (input.Width) * 12700;
            double height = (input.Height) * 12700;
            return new Rect(x, y, width, height);
        }

        /// <summary>
        /// Convert to pixels from EMU's.
        /// </summary>
        static public Size ConvertToPixels(Size input)
        {
            double dx = 96;
            PresentationSource source = PresentationSource.FromVisual(AppManager.TheBoss.MainWindow);
            if (source != null)
            {
                dx = 96.0 * source.CompositionTarget.TransformToDevice.M11;
            }
            double width = (input.Width / 12700);
            double height = (input.Height / 12700);
            return new Size(width, height);
        }

        /// <summary>
        /// Convert to pixels from EMU's.
        /// </summary>
        static public Rect ConvertToPixels(Rect input)
        {
            double dx = 96;
            PresentationSource source = PresentationSource.FromVisual(AppManager.TheBoss.MainWindow);
            if (source != null)
            {
                dx = 96.0 * source.CompositionTarget.TransformToDevice.M11;
            }
            double x = (input.X / 12700);
            double y = (input.Y / 12700);
            double width = (input.Width / 12700);
            double height = (input.Height / 12700);
            return new Rect(x, y, width, height);
        }
    }
}
