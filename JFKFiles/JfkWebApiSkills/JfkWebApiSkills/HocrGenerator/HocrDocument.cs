using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.CognitiveSearch.Skills.Hocr
{
    // Uses HOCR format for representing the document metadata.
    // See https://en.wikipedia.org/wiki/HOCR
    public class HocrDocument
    {
        private readonly string header = @"
            <?xml version='1.0' encoding='UTF-8'?>
            <!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
            <html xmlns='http://www.w3.org/1999/xhtml' xml:lang='en' lang='en'>
            <head>
                <title></title>
                <meta http-equiv='Content-Type' content='text/html;charset=utf-8' />
                <meta name='ocr-system' content='Microsoft Cognitive Services' />
                <meta name='ocr-capabilities' content='ocr_page ocr_carea ocr_par ocr_line ocrx_word'/>
            </head>
            <body>";
        private readonly string footer = "</body></html>";

        public HocrDocument(IEnumerable<HocrPage> pages)
        {
            Metadata = header + Environment.NewLine + string.Join(Environment.NewLine, pages.Select(p => p.Metadata)) + Environment.NewLine + footer;
            Text = string.Join(Environment.NewLine, pages.Select(p => p.Text));
        }

        public string Metadata { get; set; }

        public string Text { get; set; }
    }

    public class HocrPage
    {
        StringWriter metadata = new StringWriter();
        StringWriter text = new StringWriter() { NewLine = " " };

        public HocrPage(OcrImageMetadata imageMetadata, int pageNumber, Dictionary<string,string> wordAnnotations = null)
        {
            // page
            metadata.WriteLine($"<div class='ocr_page' id='page_{pageNumber}' title='image \"{imageMetadata.ImageStoreUri}\"; bbox 0 0 {imageMetadata.Width} {imageMetadata.Height}; ppageno {pageNumber}'>");
            metadata.WriteLine($"<div class='ocr_carea' id='block_{pageNumber}_1'>");

            IEnumerable<IEnumerable<NormalizedWord>> wordGroups;
            if (imageMetadata.HandwrittenLayoutText != null && imageMetadata.LayoutText != null)
            {
                if (imageMetadata.HandwrittenLayoutText.Text.Length > imageMetadata.LayoutText.Text.Length)
                {
                    wordGroups = BuildOrderedWordGroupsFromBoundingBoxes(imageMetadata.HandwrittenLayoutText.Lines, imageMetadata.HandwrittenLayoutText.Words);
                } else
                {
                    wordGroups = BuildOrderedWordGroupsFromBoundingBoxes(imageMetadata.LayoutText.Lines, imageMetadata.LayoutText.Words);
                }
            } else if (imageMetadata.HandwrittenLayoutText != null)
            {
                wordGroups = BuildOrderedWordGroupsFromBoundingBoxes(imageMetadata.HandwrittenLayoutText.Lines, imageMetadata.HandwrittenLayoutText.Words);
            } else
            {
                wordGroups = BuildOrderedWordGroupsFromBoundingBoxes(imageMetadata.LayoutText.Lines, imageMetadata.LayoutText.Words);
            }

            int li = 0;
            int wi = 0;
            foreach (IEnumerable<NormalizedWord> words in wordGroups)
            {
                metadata.WriteLine($"<span class='ocr_line' id='line_{pageNumber}_{li}' title='baseline -0.002 -5; x_size 30; x_descenders 6; x_ascenders 6'>");
                
                foreach (NormalizedWord word in words)
                {
                    string annotation = "";
                    if (wordAnnotations != null && wordAnnotations.TryGetValue(word.Text, out string wordAnnotation))
                    {
                        annotation = $"data-annotation='{wordAnnotation}'";
                    }
                    string bbox = word.BoundingBox != null && word.BoundingBox.Count == 4 ? $"bbox {word.BoundingBox[0].X} {word.BoundingBox[0].Y} {word.BoundingBox[2].X} {word.BoundingBox[2].Y}" : "";
                    metadata.WriteLine($"<span class='ocrx_word' id='word_{pageNumber}_{li}_{wi}' title='{bbox}' {annotation}>{word.Text}</span>");
                    text.WriteLine(word.Text);
                    wi++;
                }
                li++;
                metadata.WriteLine("</span>"); // Line
            }
            metadata.WriteLine("</div>"); // Reading area
            metadata.WriteLine("</div>"); // Page
        }

        public string Metadata
        {
            get { return metadata.ToString(); }
        }
        
        public string Text
        {
            get { return text.ToString(); }
        }

        private IEnumerable<IEnumerable<NormalizedWord>> BuildOrderedWordGroupsFromBoundingBoxes(List<NormalizedLine> lines, List<NormalizedWord> words)
        {
            List<LineWordGroup> lineGroups = new List<LineWordGroup>();
            foreach (NormalizedLine line in lines)
            {
                LineWordGroup currGroup = new LineWordGroup(line);
                foreach (NormalizedWord word in words)
                {
                    if (CheckIntersection(line.BoundingBox, word.BoundingBox) && line.Text.Contains(word.Text))
                    {
                        currGroup.Words.Add(word);
                    }
                }
                lineGroups.Add(currGroup);
            }
            return lineGroups.OrderBy(grp => grp.Line.BoundingBox.Select(p => p.Y).Max()).Select(grp => grp.Words.FirstOrDefault()?.BoundingBox == null ? grp.Words.ToArray() : grp.Words.OrderBy(l => l.BoundingBox[0].X).ToArray());      
        }

        private bool CheckIntersection(List<Point> line, List<Point> word)
        {
            int lineLeft = line.Select(pt => pt.X).Min();
            int lineTop = line.Select(pt => pt.Y).Min();
            int lineRight = line.Select(pt => pt.X).Max();
            int lineBottom = line.Select(pt => pt.Y).Max();

            int wordLeft = word.Select(pt => pt.X).Min();
            int wordTop = word.Select(pt => pt.Y).Min();
            int wordRight = word.Select(pt => pt.X).Max();
            int wordBottom = word.Select(pt => pt.Y).Max();

            return !(wordLeft > lineRight
                || wordRight < lineLeft
                || wordTop > lineBottom
                || wordBottom < lineTop);
        }

        private class LineWordGroup
        {
            public NormalizedLine Line;
            public List<NormalizedWord> Words;

            public LineWordGroup(NormalizedLine line)
            {
                Line = line;
                Words = new List<NormalizedWord>();
            }
        }
    }
}
