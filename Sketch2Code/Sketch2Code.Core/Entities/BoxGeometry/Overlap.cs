using Sketch2Code.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sketch2Code.Core.BoxGeometry
{
    public class Overlap
    {
        const double MARGIN = 0;

        public double OverlapArea(BoundingBox b1, BoundingBox b2)
        {
            double x_overlap = CalculateOverlapX(b1.Left, b1.Width, b2.Left, b2.Width);
            double y_overlap = CalculateOverlapY(b1.Top, b1.Height, b2.Top, b2.Height);

            double b1_area = b1.Height * b1.Width;
            double b2_area = b2.Height * b2.Width;

            //Calculate percentage over smaller box
            if (b1_area < b2_area)
            {
                return (x_overlap * y_overlap / b1_area);
            }
            else
            {
                return (x_overlap * y_overlap / b2_area);
            }
        }

        public double OverlapAreaX(BoundingBox b1, BoundingBox b2)
        {
            double x_overlap = CalculateOverlapX(b1.Left, b1.Width, b2.Left, b2.Width);
            double b1_area = b1.Width;
            double b2_area = b2.Width;

            if (b1_area < b2_area)
            {
                return (x_overlap / b1_area);
            }
            else
            {
                return (x_overlap / b2_area);
            }
        }

        public double OverlapAreaY(BoundingBox b1, BoundingBox b2)
        {
            double y_overlap = CalculateOverlapY(b1.Top, b1.Height, b2.Top, b2.Height);
            double b1_area = b1.Height;
            double b2_area = b2.Height;

            if (b1_area < b2_area)
            {
                return (y_overlap / b1_area);
            }
            else
            {
                return (y_overlap / b2_area);
            }
        }

        private static double CalculateOverlapX(double b1_left, double b1_width, double b2_left, double b2_width)
        {
            return Math.Max(0, Math.Min(b1_left + b1_width + MARGIN, b2_left + b2_width) - Math.Max(b1_left, b2_left));
        }

        private static double CalculateOverlapY(double b1_top, double b1_height, double b2_top, double b2_height)
        {
            return Math.Max(0, Math.Min(b1_top + b1_height + MARGIN, b2_top + b2_height) - Math.Max(b1_top, b2_top));
        }


    }
}
