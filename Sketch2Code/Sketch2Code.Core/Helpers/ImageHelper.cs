using Sketch2Code.Core.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sketch2Code.Core.Helpers
{
    public static class ImageHelper
    {
        public static byte[] SliceImage(this byte[] image, double x, double y, double width, double height)
        {
            using (var ms = new MemoryStream(image))
            {
                return SliceImage(ms, x, y, width, height);
            }
        }
        public static byte[] SliceImage(this Stream image, double x, double y, double width, double height)
        {
            var _width = (int)Math.Floor(width);
            var _height = (int)Math.Floor(height);
            var _x = (int)Math.Floor(x);
            var _y = (int)Math.Floor(y);

            var rectangle = new Rectangle(_x, _y, _width, _height);

            using (Bitmap img = Image.FromStream(image) as Bitmap)
            {
                var slice = img.Clone(rectangle, img.PixelFormat);
                using (var ms = new MemoryStream())
                {
                    slice.Save(ms, ImageFormat.Png);
                    return ms.ToArray();
                }
            }
        }
        public static byte[] DrawRectangle(this Stream image, IList<PredictedObject> objects)
        {
            byte[] buffer = null;

            int tabHeight = 60;
            int margin = 15;
            int fontSize = 25;

            using (var img = Bitmap.FromStream(image))
            {
                using (var graphics = Graphics.FromImage(img))
                {
                    foreach (var p in objects)
                    {
                        var _width = (int)Math.Floor(p.BoundingBox.Width);
                        var _height = (int)Math.Floor(p.BoundingBox.Height);
                        var _x = (int)Math.Floor(p.BoundingBox.Left);
                        var _y = (int)Math.Floor(p.BoundingBox.Top);

                        var rectangle = new Rectangle(_x, _y, _width, _height);
                        var tab = new Rectangle(_x, _y - tabHeight, _width, tabHeight);
                       
                        graphics.DrawRectangle(Pens.Red, tab);
                        graphics.FillRectangle(p.BoundingBox.BoxColor, tab);

                        using (var font = new Font("Arial", fontSize, FontStyle.Bold, GraphicsUnit.Pixel))
                        {
                            var textBox = new Rectangle(tab.Left + margin, tab.Top + margin, tab.Width, tab.Height);
                            graphics.DrawString($"{p.ClassName}: {(p.Probability * 100).ToString("F")}%", font, Brushes.White, textBox);
                        }

                        graphics.DrawRectangle(Pens.Red, rectangle);
                    }
                }
                using (var ms = new MemoryStream())
                {
                    img.Save(ms, img.RawFormat);
                    buffer = ms.ToArray();
                }
            }

            return buffer;
        }
        public static byte[] DrawRectangle(this byte[] image, IList<PredictedObject> objects)
        {
            using(var ms = new MemoryStream(image))
            {
                return DrawRectangle(ms, objects);
            }
        }

        public static byte[] DrawRectangle(this byte[] image, double x, double y, double width, double height)
        {
            using (var ms = new MemoryStream(image))
            {
                using (var img = Bitmap.FromStream(ms))
                {
                    using (var graphics = Graphics.FromImage(img))
                    {
                        var _width = (int)Math.Floor(width);
                        var _height = (int)Math.Floor(height);
                        var _x = (int)Math.Floor(x);
                        var _y = (int)Math.Floor(y);

                        var rectangle = new Rectangle(_x, _y, _width, _height);
                        graphics.DrawRectangle(Pens.Chocolate, rectangle);
                    }

                    img.Save(ms, img.RawFormat);
                }
                return ms.ToArray();
            }
        }


        public static Dictionary<string, Brush> Colors
        {
            get
            {
                var dict = new Dictionary<string, Brush>();
                dict.Add("Button", Brushes.Firebrick);
                dict.Add("CheckBox", Brushes.Goldenrod);
                dict.Add("ComboBox", Brushes.MediumVioletRed);
                dict.Add("Heading", Brushes.SteelBlue);
                dict.Add("Image", Brushes.Purple);
                dict.Add("Label", Brushes.LightSeaGreen);
                dict.Add("Link", Brushes.DeepPink);
                dict.Add("Paragraph", Brushes.DarkOrange);
                dict.Add("RadioButton", Brushes.Chocolate);
                dict.Add("Table", Brushes.LightSlateGray);
                return dict;
            }
        }

        private static Image scaleByPercent(Image imgPhoto, int Percent)
        {
            float nPercent = ((float)Percent / 100);

            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;


            int destX = 0;
            int destY = 0;
            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(destWidth, destHeight,
                                     PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                                    imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.InterpolationMode = InterpolationMode.High;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }

        public static byte[] OptimizeImage(byte[] original, int scale, int quality)
        {
            MemoryStream outStream = new MemoryStream();

            Image img = Image.FromStream(new MemoryStream(original));

            scale = calculateScale(img.Width, img.Height);

            img = scaleByPercent(img, scale);

            EncoderParameters eps = new EncoderParameters(1);
            eps.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);
            ImageCodecInfo ici = GetEncoderInfo("image/png");

            ExifRotate(img);

            img.Save(outStream, ici, eps);

            return outStream.GetBuffer();
        }



        private static int calculateScale(double width, double height)
        {
            double dimension;

            if (width > height)
            {
                dimension = width;
            }
            else
            {
                dimension = height;
            }

            if (dimension > 1024)
            {
                return Convert.ToInt32(1 / (dimension / 1024) * 100);
            }

            return 100;
        }

        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        

        public static void ExifRotate(Image img)
        {
            
            const int exifOrientationID = 0x0112; //274
            if (!img.PropertyIdList.Contains(exifOrientationID))
                return;

            var prop = img.GetPropertyItem(exifOrientationID);
            int val = BitConverter.ToUInt16(prop.Value, 0);
            var rot = RotateFlipType.RotateNoneFlipNone;

            if (val == 3 || val == 4)
                rot = RotateFlipType.Rotate180FlipNone;
            else if (val == 5 || val == 6)
                rot = RotateFlipType.Rotate90FlipNone;
            else if (val == 7 || val == 8)
                rot = RotateFlipType.Rotate270FlipNone;

            if (val == 2 || val == 4 || val == 5 || val == 7)
                rot |= RotateFlipType.RotateNoneFlipX;

            if (rot != RotateFlipType.RotateNoneFlipNone)
                img.RotateFlip(rot);
        }
    }
}

