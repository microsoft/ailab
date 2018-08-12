using Sketch2Code.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sketch2Code.Core.BoxGeometry
{
    public class SliceSection
    {
        public double Start;
        public double End;
        public bool IsEmpty = true;
        public List<BoundingBox> Boxes = new List<BoundingBox>();
    }
}
