using Sketch2Code.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sketch2Code.Core.Entities
{
    [DataContract(IsReference = true)]
    public class PredictedObject
    {
        string _className;
        public PredictedObject()
        {
            this.BoundingBox = new BoundingBox();
        }
        [DataMember]
        public string ClassName
        {
            get
            {
                return _className;
            }
            set
            {
                _className = value;
                if (ImageHelper.Colors.ContainsKey(value))
                    this.BoundingBox.BoxColor = ImageHelper.Colors[value];
                else
                    this.BoundingBox.BoxColor = Brushes.Green;
            }
        }
        [DataMember]
        public double Probability { get; set; }
        [DataMember]
        public BoundingBox BoundingBox { get; set; }
        [DataMember]
        public IList<string> Text { get; set; }
        public byte[] SlicedImage { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string FileName { get; set; }
        public override string ToString()
        {
            return $"Top: {this.BoundingBox.Top} - Left: {this.BoundingBox.Left} - Width: {this.BoundingBox.Width} - Height: {this.BoundingBox.Height} -> Probability: {this.Probability}";
        }
    }

    [DataContract(IsReference = true)]
    public class BoundingBox
    {
        public Brush BoxColor { get; set; }
        [DataMember]
        public double Top { get; set; }
        [DataMember]
        public double Left { get; set; }
        [DataMember]
        public double Height { get; set; }
        [DataMember]
        public double Width { get; set; }

        [DataMember]
        public double HeightPercentage
        {
            get
            {
                if (MaxHeight > 0)
                {
                    return Height /MaxHeight;
                }
                else
                {
                    return 0;
                }
                
            }
        }

        [DataMember]
        public double WidthPercentage
        {
            get
            {
                if (MaxWidth > 0)
                {
                    return Width / MaxWidth;
                }
                else
                {
                    return 0;
                }
            }
        }

        public Tuple<double, double> TopLeft
        {
            get
            {
                return new Tuple<double, double>(this.Top, this.Left);
            }
        }
        public Tuple<double, double> BottomLeft
        {
            get
            {
                return new Tuple<double, double>(this.Top - Height, this.Left);
            }
        }
        public Tuple<double, double> TopRight
        {
            get
            {
                return new Tuple<double, double>(this.Top, this.Left + this.Width);
            }
        }
        public Tuple<double, double> BottomRight
        {
            get
            {
                return new Tuple<double, double>(this.Top - Height, this.Left + this.Width);
            }
        }
        [DataMember]
        public double TopNorm { get; set; }
        [DataMember]
        public double LeftNorm { get; set; }
        [DataMember]
        public double MaxWidth { get; set; }
        [DataMember]
        public double MaxHeight { get; set; }
        [DataMember]
        public double MiddleHeight
        {
            get
            {
                return Math.Abs(Top - Height) / 2;
            }
        }
        [DataMember]
        public double MiddleWidth { get { return Left + Width / 2; } }
        public Rectangle Rectangle
        {
            get
            {
                int x = (int)Math.Floor(this.Left);
                int y = (int)Math.Floor(this.Top);
                int width = (int)Math.Floor(this.Width);
                int height = (int)Math.Floor(this.Height);


                return new Rectangle(x, y, width, height);
            }
        }
        [DataMember]
        public PredictedObject PredictedObject { get; set; }
    }
}
