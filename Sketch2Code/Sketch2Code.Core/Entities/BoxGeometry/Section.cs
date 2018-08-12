using Sketch2Code.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sketch2Code.Core.BoxGeometry
{
    public class Section
    {
        public double Start
        {
            get
            {
                return Slices[0].Start;
            }
        }

        public double End
        {
            get
            {
                return Slices[Slices.Count - 1].End;
            }
        }

        public bool IsEmpty { get { return Slices[0].IsEmpty; } }

        public List<SliceSection> Slices = new List<SliceSection>();

        public List<BoundingBox> Boxes
        {
            get
            {
                List<BoundingBox> boxes = new List<BoundingBox>();

                foreach (SliceSection slice in Slices)
                {
                    boxes.AddRange(slice.Boxes);
                }

                boxes = boxes.Distinct().ToList();

                return boxes;
            }
        }

    }
}
