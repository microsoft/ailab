using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoxGeometry
{
    public class BoundingBox
    {
        public string Code;
        public double X;
        public double Y;
        public double Height;
        public double Width;

        public double MiddleHeight
        {
            get
            {
                return Y + Height / 2;
            }
        }

        public double MiddleWidth { get { return X + Width / 2; } }
    }
}
