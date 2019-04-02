using System.Collections.Generic;

namespace Microsoft.CognitiveSearch.Skills.Hocr
{
    public class OcrLayoutText
    {
        public string Language { get; set; }

        public string Text { get; set; }

        public List<NormalizedLine> Lines { get; set; }

        public List<NormalizedWord> Words { get; set; }
    }

    public class NormalizedLine
    {
        public List<Point> BoundingBox { get; set; }

        public string Text { get; set; }
    }

    public class NormalizedWord
    {
        public List<Point> BoundingBox { get; set; }

        public string Text { get; set; }
    }

    public class Point
    {
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }

        public int Y { get; }

    }
}
