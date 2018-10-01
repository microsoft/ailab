using System;
using System.Collections.Generic;
using Cairo;
using XF = Xamarin.Forms;

namespace SnipInsight.Forms.GTK.Common
{
    public static class CairoHelpers
    {
        public static readonly int LeftMouseButton = 1;

        public static void DrawInverseSelectionRectangles(
            Context context,
            XF.Color color,
            double x0,
            double y0,
            double x1,
            double y1,
            double windowWidth,
            double windowHeight)
        {
            context.SetSourceRGBA(color.R, color.G, color.B, color.A);

            double actualX0, actualY0, actualX1, actualY1;

            if (x1 < x0)
            {
                actualX0 = x1;
                actualX1 = x0;
            }
            else
            {
                actualX0 = x0;
                actualX1 = x1;
            }

            if (y1 < y0)
            {
                actualY0 = y1;
                actualY1 = y0;
            }
            else
            {
                actualY0 = y0;
                actualY1 = y1;
            }

            context.Rectangle(0, 0, windowWidth, actualY0);
            var sideRectanglesHeight = actualY1 - actualY0;
            context.Rectangle(0, actualY0, actualX0, sideRectanglesHeight);
            context.Rectangle(
                actualX1,
                actualY0,
                windowWidth - actualX1,
                sideRectanglesHeight);
            context.Rectangle(0, actualY1, windowWidth, windowHeight - actualY1);

            context.Fill();
        }

        public static void DrawSelectionRectangle(
            Context context,
            double[] dashes,
            XF.Color color,
            double x0,
            double y0,
            double x1,
            double y1)
        {
            // FIXME macOS: line keeps being of width 2, but with some weird transparency
            // context.LineWidth = 1;
            context.SetDash(dashes, 0);
            context.SetSourceRGB(color.R, color.G, color.B);

            context.Rectangle(x0, y0, x1 - x0, y1 - y0);
            context.Stroke();
        }

        public static void DrawSizeTooltip(
            Context context,
            XF.Color backgroundColor,
            XF.Color textColor,
            double x0,
            double y0,
            double x1,
            double y1,
            int paddingInPixels,
            int marginInPixels)
        {
            context.SetFontSize(12);

            var width = Math.Abs(x1 - x0);
            var height = Math.Abs(y1 - y0);
            var text = $"{width} x {height}";
            var extents = context.TextExtents(text);
            var tooltipWidth = extents.Width + (paddingInPixels * 2);
            var tooltipHeight = extents.Height + (paddingInPixels * 2);

            var x = x1 - marginInPixels - tooltipWidth + paddingInPixels;
            if (x < 0)
            {
                x = 0;
            }

            var y = y1 + marginInPixels + tooltipHeight - paddingInPixels;
            if (y < 0)
            {
                y = 0;
            }

            context.SetSourceRGB(
                backgroundColor.R,
                backgroundColor.G,
                backgroundColor.B);

            RoundedRectangle(
                context,
                x - paddingInPixels,
                y - tooltipHeight + paddingInPixels,
                tooltipWidth,
                tooltipHeight);

            context.Fill();

            context.SetSourceRGB(textColor.R, textColor.G, textColor.B);
            context.MoveTo(x, y);
            context.ShowText(text);

            context.Stroke();
        }

        public static Gdk.Size GetAspectFitSize(int imageWidth, int imageHeight, int destinyWidth, int destinyHeight)
        {
            var ratioWidth = destinyWidth / (double)imageWidth;
            var ratioHeight = destinyHeight / (double)imageHeight;

            var finalSize = new Gdk.Size(destinyWidth, destinyHeight);

            if (ratioHeight < ratioWidth)
            {
                finalSize.Width = (int)((destinyHeight / (double)imageHeight) * imageWidth);
            }
            else if (ratioWidth < ratioHeight)
            {
                finalSize.Height = (int)((destinyWidth / (double)imageWidth) * imageHeight);
            }

            return finalSize;
        }

        public static bool IsInside(double x, double y, Rectangle rectangle) =>
            x >= rectangle.X &&
            x <= (rectangle.X + rectangle.Width) &&
            y >= rectangle.Y &&
            y <= (rectangle.Y + rectangle.Height);

        public static Rectangle NormalizeRectangle(Rectangle rectangle)
        {
            var x = rectangle.Width < 0 ? rectangle.X + rectangle.Width : rectangle.X;
            var y = rectangle.Height < 0 ? rectangle.Y + rectangle.Height : rectangle.Y;
            var normalizedRectangle = new Rectangle(x, y, Math.Abs(rectangle.Width), Math.Abs(rectangle.Height));

            return normalizedRectangle;
        }

        // Taken from https://cairographics.org/cookbook/roundedrectangles/ (Method C)
        public static void RoundedRectangle(
            Context context, double x, double y, double width, double height, int radius = 10)
        {
            context.MoveTo(x + radius, y);
            context.LineTo(x + width - radius, y);
            context.CurveTo(x + width, y, x + width, y, x + width, y + radius);
            context.LineTo(x + width, y + height - radius);
            context.CurveTo(x + width, y + height, x + width, y + height, x + width - radius, y + height);
            context.LineTo(x + radius, y + height);
            context.CurveTo(x, y + height, x, y + height, x, y + height - radius);
            context.LineTo(x, y + radius);
            context.CurveTo(x, y, x, y, x + radius, y);
        }

        public static List<PointD> GetInterpolatedPoints(PointD p0, PointD p1, double lineWeight)
        {
            var interPolatedPoints = new List<PointD>();
            var distance = GetDistance(p0, p1);

            var interpolatedPoint = p0;
            while (distance > lineWeight)
            {
                interpolatedPoint.X = interpolatedPoint.X + ((p1.X - interpolatedPoint.X) * lineWeight);

                interpolatedPoint.Y = interpolatedPoint.Y + ((p1.Y - interpolatedPoint.Y) * lineWeight);

                interPolatedPoints.Add(interpolatedPoint);

                distance = GetDistance(p1, interpolatedPoint);
            }

            return interPolatedPoints;
        }

        public static double GetDistance(PointD p0, PointD p1)
        {
            return Math.Abs(Math.Sqrt(Math.Pow(p1.X - p0.X, 2) + Math.Pow(p1.Y - p0.Y, 2)));
        }
    }
}
