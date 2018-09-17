// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SnipInsight.ClipboardUtils
{
	/// <summary>
	/// Manager for all clipboard functionality.
	/// </summary>
	public static class ClipboardManager
    {
        #region Clipboard Html Formats Constants
        /// <summary>
        /// Header for clipboard Html format.
        /// </summary>
        private const string Header = @"Version:0.9
StartHTML:<<<<<<<<1
EndHTML:<<<<<<<<2
StartFragment:<<<<<<<<3
EndFragment:<<<<<<<<4
StartSelection:<<<<<<<<3
EndSelection:<<<<<<<<4
SourceURL: {0}
";

        /// <summary>
        /// Html open tag.
        /// </summary>
        private const string HtmlOpen = "<html>\r\n<body>";

        /// <summary>
        /// Html close tag.
        /// </summary>
        private const string HtmlClose = "</body>\r\n</html>";

        /// <summary>
        /// Start fragment for the inner html.
        /// </summary>
        private const string StartFragment = "<!--StartFragment-->";

        /// <summary>
        /// End fragment for the inner html.
        /// </summary>
        private const string EndFragment = "<!--EndFragment-->";

        /// <summary>
        /// Link only html.
        /// </summary>
        private const string LinkHtml = "<a href='{0}'>{1}</a>";

        /// <summary>
        /// Image link html.
        /// </summary>
        private const string ImageLinkHtml = "<a href='{0}'><img src='{1}' alt-text='{0}' style='border:none;outline:0;width:{2}px;height:{3}px;' border='0' width='{2}px' height='{3}px'/></a><br /><br /><a href='{0}'>{0}</a>";
        #endregion

        /// <summary>
        /// Copy to clipboard.
        /// </summary>
        /// <param name="image"></param>
        public static bool Copy(BitmapSource image)
        {
            try
            {
                Clipboard.Clear();

                IDataObject obj = new DataObject();

                // Set the image onto clipboard.
                if (image != null)
                {
                    obj.SetData(DataFormats.Bitmap, image, false);
                }
                Clipboard.SetDataObject(obj, true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Copy text to clipboard.
        /// </summary>
        public static bool Copy(string text)
        {
            try
            {
                Clipboard.Clear();
                Clipboard.SetText(text);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #region Helpers
        /// <summary>
        /// Generate the clipboard html
        /// </summary>
        private static string GetClipboardHtml(string header, string html)
        {
            var sb = new StringBuilder();
            sb.Append(header);
            sb.AppendLine(@"<!DOCTYPE HTML  PUBLIC ""-//W3C//DTD HTML 4.0  Transitional//EN"">");
            sb.Append(HtmlOpen);
            sb.Append(StartFragment);
            // Get the fragmentStart
            int fragmentStart = GetByteCount(sb);
            sb.Append(html);
            int fragmentEnd = GetByteCount(sb);
            sb.Append(EndFragment);
            sb.Append(HtmlClose);

            // Back-patch offsets (scan only the  header part for performance)
            sb.Replace("<<<<<<<<1", header.Length.ToString("D9"), 0, header.Length);
            sb.Replace("<<<<<<<<2", GetByteCount(sb).ToString("D9"), 0, header.Length);
            sb.Replace("<<<<<<<<3", fragmentStart.ToString("D9"), 0, header.Length);
            sb.Replace("<<<<<<<<4", fragmentEnd.ToString("D9"), 0, header.Length);
            return sb.ToString();
        }
        /// <summary>
        /// Calculates the number of bytes produced  by encoding the string in the string builder in UTF-8 and not .NET default  string encoding.
        /// </summary>
        /// <param name="sb">the  string builder to count its string</param>
        /// <param  name="start">optional: the start index to calculate from (default  - start of string)</param>
        /// <param  name="end">optional: the end index to calculate to (default - end  of string)</param>
        /// <returns>the number of bytes  required to encode the string in UTF-8</returns>
        private static int GetByteCount(StringBuilder sb, int start = 0, int end = -1)
        {
            char[] byteArray = new char[1];
            int count = 0;
            end = end > -1 ? end : sb.Length;
            for (int i = start; i < end; i++)
            {
                byteArray[0] = sb[i];
                count += Encoding.UTF8.GetByteCount(byteArray);
            }
            return count;
        }
        #endregion
    }
}
