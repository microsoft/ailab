using Sketch2Code.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sketch2Code.Core.BoxGeometry
{
    public class Geometry
    {
        public void AlignTop(List<BoundingBox> testBoxes)
        {
            double average = testBoxes.Average(p => p.Top);

            foreach (BoundingBox b in testBoxes)
            {
                b.Top = average;
            }
        }

        public void AlignLeft(List<BoundingBox> testBoxes)
        {
            double average = testBoxes.Average(p => p.Left);

            foreach (BoundingBox b in testBoxes)
            {
                b.Left = average;
            }
        }

        public void MakeSameSize(List<BoundingBox> testBoxes)
        {
            double wAvg = testBoxes.Average(p => p.Width);
            double hAvg = testBoxes.Average(p => p.Height);


            foreach (BoundingBox b in testBoxes)
            {
                b.Height = hAvg; 
                b.Width = wAvg;
            }
        }

        public bool IsBoxInVerticalRange(BoundingBox master, BoundingBox child)
        {
            return child.MiddleHeight > master.Top && child.MiddleHeight < (master.Top + master.Height);
        }

        public bool IsBoxInHorizontalRange(BoundingBox master, BoundingBox child)
        {
            return child.MiddleWidth > master.Top && child.MiddleWidth < (master.Left + master.Width);
        }

        public ProjectionRuler BuildProjectionRuler(List<BoundingBox> boxes, ProjectionAxisEnum axis)
        {
            ProjectionRuler ruler = new ProjectionRuler();

            List<SliceSection> slices = buildSlices(boxes, axis);
            fillSlices(boxes, slices,axis);

            Section s = new Section();
            s.Slices.Add(slices[0]);
            ruler.Sections.Add(s);

            bool isEmpty = false;

            for (int i = 1; i < slices.Count; i++)
            {
                if (!isEmpty)
                {
                    if (slices[i].IsEmpty)
                    {
                        //close previous section
                        s = new Section();
                        s.Slices.Add(slices[i]);
                        isEmpty = true;
                        ruler.Sections.Add(s);
                    }
                    else
                    {
                        //modify previous section
                        s.Slices.Add(slices[i]);
                    }
                }
                else
                {
                    if (!slices[i].IsEmpty)
                    {
                        //close previous section
                        s = new Section();
                        s.Slices.Add(slices[i]);
                        isEmpty = false;
                        ruler.Sections.Add(s);
                    }
                    else
                    {
                        //modify previous section
                        s.Slices.Add(slices[i]);
                    }
                }
            }

            return ruler;
        }

        private static void fillSlices(List<BoundingBox> boxes, List<SliceSection> slices, ProjectionAxisEnum axis)
        {
            int first;
            int last;

            //fill departments
            foreach (BoundingBox b in boxes)
            {
                if (axis == ProjectionAxisEnum.Y)
                {
                    first = slices.FindIndex(p => (p.Start <= b.Top) && (p.End >= b.Top));
                    last = slices.FindIndex(p => (p.End >= b.Top + b.Height));
                }
                else
                {
                    first = slices.FindIndex(p => (p.Start <= b.Left) && (p.End >= b.Left));
                    last = slices.FindIndex(p => (p.End >= b.Left + b.Width));
                }

                for (int i = first; i <= last; i++)
                {
                    slices[i].IsEmpty = false;
                    slices[i].Boxes.Add(b);
                }
            }
        }

        private static List<SliceSection> buildSlices(List<BoundingBox> boxes, ProjectionAxisEnum axis)
        {
            double precision = 1000;
            double min;
            double max;

            if (axis == ProjectionAxisEnum.Y)
            {
                //calculate minimum
                min = boxes.Min(p => p.Top);

                //calculate maximum
                max = boxes.Max(p => p.Top + p.Height);

            }
            else
            {
                //calculate minimum
                min = boxes.Min(p => p.Left);

                //calculate maximum
                max = boxes.Max(p => p.Left + p.Width);
            }

            //make departments
            double sliceSize = (max - min) / precision;

            List<SliceSection> slices = new List<SliceSection>();

            for (double i = min; i < max; i = i + sliceSize)
            {
                SliceSection slice = new SliceSection();
                slice.Start = i;
                slice.End = i + sliceSize;
                slice.IsEmpty = true;
                slices.Add(slice);
            }

            return slices;
        }

        public GroupBox BuildGroups(List<BoundingBox> boxes)
        {
            GroupBox root = new GroupBox();
            root.Direction = GroupBox.GroupDirectionEnum.Vertical;
            BuildChildGroups(boxes, ProjectionAxisEnum.Y, root);
            return root.Groups[0];
        }
        public void BuildChildGroups(List<BoundingBox> boxes, ProjectionAxisEnum axis, GroupBox parent)
        {
            GroupBox g = new GroupBox();
            g.IsEmpty = false;
            

            if (axis == ProjectionAxisEnum.X)
            {
                g.Direction = GroupBox.GroupDirectionEnum.Horizontal;

            }
            else
            {
                g.Direction = GroupBox.GroupDirectionEnum.Vertical;
            }

            parent.Groups.Add(g);

            if (boxes.Count > 1)
            {
                g.X = boxes.Min(p => p.Left);
                g.Y = boxes.Min(p => p.Top);
                g.Width = boxes.Max(p => p.Width);
                g.Height = boxes.Max(p => p.Height);

                ProjectionRuler ruler = BuildProjectionRuler(boxes, axis);
                                
                foreach (Section sec in ruler.Sections)
                {
                    if (axis == ProjectionAxisEnum.X)
                    {
                        BuildChildGroups(sec.Boxes, ProjectionAxisEnum.Y, g);
                    }
                    else
                    {
                        BuildChildGroups(sec.Boxes, ProjectionAxisEnum.X, g);
                    }
                }
            }
            else
            {
                if (boxes.Count == 0)
                {
                    g.IsEmpty = true;
                }
                if (boxes.Count == 1)
                {
                    g.X = boxes.Min(p => p.Left);
                    g.Y = boxes.Min(p => p.Top);
                    g.Width = boxes.Max(p => p.Width);
                    g.Height = boxes.Max(p => p.Height);

                    //Add the box to the structure
                    g.Boxes.Add(boxes[0]);

                    CalculateAlignments(g);
                }
            }
        }

        public void DeleteOverlapping(List<BoundingBox> boxes)
        {
            List<BoundingBox> toDelete = new List<BoundingBox>();

            Overlap ovl = new Overlap();

            foreach (BoundingBox outer in boxes)
            {
                foreach (BoundingBox inner in boxes)
                {
                    if (outer != inner)
                    {
                        if (ovl.OverlapArea(outer, inner) > .75)
                        {
                            //delete the one with lower probability
                            if (outer.PredictedObject.Probability > inner.PredictedObject.Probability)
                            {
                                toDelete.Add(inner);
                            }
                            else
                            {
                                toDelete.Add(outer);
                            }
                        }
                    }
                }
            }

            foreach (BoundingBox b in toDelete)
            {
                boxes.Remove(b);
            }
        }

        public void RemoveOverlappingOld(List<BoundingBox> boxes)
        {
            const int TO_MOVE = 50;

            Overlap ovl = new Overlap();

            DeleteOverlapping(boxes);

            boxes = boxes.OrderBy(p => p.Top).ToList();

            foreach (BoundingBox outer in boxes)
            {
                foreach (BoundingBox inner in boxes)
                {
                    if (outer != inner)
                    {
                        if (ovl.OverlapArea(outer, inner) > 0)
                        {
                            double ovl_x = ovl.OverlapAreaX(outer, inner);
                            double ovl_y = ovl.OverlapAreaY(outer, inner);

                            if (ovl_y > ovl_x)
                            {
                                //Move inner to the right
                                if (inner.Left > outer.Left)
                                {
                                    inner.Left = outer.Left + outer.Width + TO_MOVE;
                            }
                            else
                            {
                                outer.Left = inner.Left + inner.Width + TO_MOVE;
                            }
                        }
                            else
                            {
                                if (inner.Top > outer.Top)
                                {
                                    //Move inner to the bottom
                                    inner.Top = outer.Top + outer.Height + TO_MOVE;
                                }
                                else
                                {
                                    //Move outer to the bottom
                                    outer.Top = inner.Top + inner.Height + TO_MOVE;
                                }

                                //Move everything down 100
                                List<BoundingBox> toMove =  boxes.Where(p => p.Top > inner.Top + inner.Height).ToList();

                                foreach (BoundingBox b in toMove)
                                {
                                    b.Top = b.Top + TO_MOVE;
                                }

                            }
                        }
                    }
                }
            }
        }

        public void RemoveOverlapping(List<BoundingBox> boxes)
        {
            RemoveOverlappingY(boxes);
            RemoveOverlappingX(boxes);
            DeleteOverlapping(boxes);
        }

        public void RemoveOverlappingY(List<BoundingBox> boxes)
        {
            const int TO_MOVE = 50;

            Overlap ovl = new Overlap();

            boxes = boxes.OrderBy(p => p.Top).ToList();

            for (int i = 0; i < boxes.Count; i++)
            {
                BoundingBox outer = boxes[i];

                for (int j = i+1; j < boxes.Count; j++)
                {
                    BoundingBox inner = boxes[j];

                    if (ovl.OverlapAreaY(outer, inner) < .75)
                    {
                        if (ovl.OverlapAreaY(outer, inner) < .25)
                        {
                            //move down
                            inner.Top = inner.Top + TO_MOVE;
                        }

                        if (ovl.OverlapArea(outer, inner) > 0)
                        {
                            //move down
                            inner.Top = outer.Top + outer.Height + TO_MOVE;
                        }
                    }
                }
            }
        }

        public void RemoveOverlappingX(List<BoundingBox> boxes)
        {
            const int TO_MOVE = 50;

            Overlap ovl = new Overlap();

            boxes = boxes.OrderBy(p => p.Left).ToList();

            for (int i = 0; i < boxes.Count; i++)
            {
                BoundingBox outer = boxes[i];

                for (int j = i + 1; j < boxes.Count; j++)
                {
                    BoundingBox inner = boxes[j];

                    if (ovl.OverlapAreaX(outer, inner) < .75)
                    {
                        if (ovl.OverlapAreaX(outer, inner) < .25)
                        {
                            //move down
                            inner.Left = inner.Left + TO_MOVE;
                        }

                        if (ovl.OverlapArea(outer, inner) > 0)
                        {
                            //move down
                            inner.Left = outer.Left + outer.Width + TO_MOVE;

                        }
                    }
                }
            }
        }

        public void CalculateAlignments(GroupBox g)
        {
            if (g.Direction == GroupBox.GroupDirectionEnum.Horizontal && g.Boxes.Count == 1)
            {
                //calculate alignment
                BoundingBox b = g.Boxes[0];

                double center_b = (b.Width / 2) + b.Left;
                double center_g_steps = b.MaxWidth / 3;

                if (center_b < center_g_steps)
                {
                    g.Alignment = GroupBox.GroupAlignmentEnum.Left;
                }
                else
                {
                    if (center_b > center_g_steps & center_b < center_g_steps * 2)
                    {
                        g.Alignment = GroupBox.GroupAlignmentEnum.Center;
                    }
                    else
                    {
                        g.Alignment = GroupBox.GroupAlignmentEnum.Right;
                    }
                }
            }
        }
    }
}
