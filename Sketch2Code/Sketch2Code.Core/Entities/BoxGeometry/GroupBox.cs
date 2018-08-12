using Sketch2Code.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Sketch2Code.Core.BoxGeometry
{
    [DataContract]
    public class GroupBox
    {
        private List<BoundingBox> boxes;
        public enum GroupDirectionEnum { Horizontal, Vertical }

        public enum GroupAlignmentEnum { Left, Center, Right}

        private GroupDirectionEnum direction;
        [DataMember]
        public bool IsEmpty;
        [DataMember]
        public double X;
        [DataMember]
        public double Y;
        [DataMember]
        public double Height;
        [DataMember]
        public double Width;

        [DataMember]
        public GroupAlignmentEnum Alignment;

        public int Count
        {
            get { return Boxes.Count; }

        }
        [DataMember]
        public List<BoundingBox> Boxes
        {
            get { return boxes; }
            set { boxes = value; }
        }
        [DataMember]
        public GroupDirectionEnum Direction
        {
            get { return direction; }
            set { direction = value; }
        }
        public GroupBox()
        {
            Boxes = new List<BoundingBox>();
            this.Groups = new List<GroupBox>();
        }
        [DataMember]
        public List<GroupBox> Groups { get; set; }
    }
}