using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnipInsight.Forms.GTK.Features.Drawing
{
    public class DrawedPath
    {
        public DrawedPath()
        {
            this.Points = new List<Cairo.PointD>();
        }

        public Cairo.Color Color { get; set; }

        public double LineWeight { get; set; }

        public List<Cairo.PointD> Points { get; set; }
    }
}
