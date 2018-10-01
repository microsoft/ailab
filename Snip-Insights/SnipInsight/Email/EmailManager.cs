// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace SnipInsight.EmailController
{
    using SnipInsight.Util;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Mail;

    public static class EmailManager
    {
        /// <summary>
        /// Open email client with attachment.
        /// </summary>
        public static bool OpenEmailClientWithAttachment(string file, string subject, string attachmentName, string attachmentMimeType, string toAddress = null, string body = null)
        {
            if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
            {
                return false;
            }

            try
            {
                using (MemoryStream ms = new MemoryStream())
                using (MailMessage message = new MailMessage())
                {
                    // copy the file to memory.
                    using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        fs.CopyTo(ms);
                        fs.Flush();
                        ms.Position = 0;
                    }

                    // create a new email message with attachment.
                    message.From = new MailAddress("youremail@domain.com");
                    message.To.Add(new MailAddress(toAddress ?? "toemail@domain.com"));
                    message.Subject = subject;
                    message.Attachments.Add(new Attachment(ms, attachmentName, attachmentMimeType));
                    if (!string.IsNullOrWhiteSpace(body))
                    {
                        message.Body = body;
                    }
                    string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".eml");
                    while (File.Exists(tempFile))
                    {
                        tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".eml");
                    }

                    // save the message to disk and open it.
                    if (SaveMessage(message, tempFile))
                    {
                        Process.Start(tempFile);
                        return true;
                    }
                    else
                    {
                        if (File.Exists(tempFile))
                        {
                            File.Delete(tempFile);
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Diagnostics.LogException(ex);
                return false;
            }
        }

        /// <summary>
        /// Open email client with image embedded in the email and also attached.
        /// </summary>
        public static bool OpenEmailClientWithEmbeddedImage(string file, string subject, string attachmentName, string imageContentType)
        {
            if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
            {
                return false;
            }

            try
            {
                using (MemoryStream ms = new MemoryStream())
                using (MailMessage message = new MailMessage())
                {
                    // copy the file to memory.
                    using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        fs.CopyTo(ms);
                        fs.Flush();
                        ms.Position = 0;
                    }

                    // create a new email message with embedded image and also attach the image as alternate.
                    message.From = new MailAddress("youremail@domain.com");
                    message.To.Add(new MailAddress("toemail@domain.com"));
                    message.Subject = subject;
                    message.Attachments.Add(new Attachment(ms, attachmentName, imageContentType));
                    message.IsBodyHtml = true;

                    // setup the linked image.
                    using (MemoryStream imageSource = new MemoryStream(ms.GetBuffer()))
                    {
                        LinkedResource resource = new LinkedResource(imageSource);
                        resource.ContentId = "snipimage";
                        resource.ContentType = new System.Net.Mime.ContentType(imageContentType);

                        // Create a plain view for clients not supporting html.
                        var plainView = AlternateView.CreateAlternateViewFromString("Please find my snip attached with this email.", null, "text/plain");

                        // Create a html view for clients supporting html
                        const string format = @"<body style='font-family:""Calibri""'><img src=cid:snipimage alt-text='Snip' style='border:none;outline:0;' border='0' /><br/></body>";
                        var htmlView = AlternateView.CreateAlternateViewFromString(format, null, "text/html");
                        htmlView.LinkedResources.Add(resource);

                        // Add both the views to the mail message.
                        message.AlternateViews.Add(htmlView);
                        message.AlternateViews.Add(plainView);

                        // Get the temp file.
                        string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".eml");
                        while (File.Exists(tempFile))
                        {
                            tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".eml");
                        }

                        // save the message to disk and open it.
                        if (SaveMessage(message, tempFile))
                        {
                            Process.Start(tempFile);
                            return true;
                        }
                        else
                        {
                            if (File.Exists(tempFile))
                            {
                                File.Delete(tempFile);
                            }
                            return false;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Diagnostics.LogException(ex);
                return false;
            }
        }


        /// <summary>
        /// Open email client with html body.
        /// </summary>
        public static bool OpenEmailClientWithHyperlinkedImage(string file, string url, int width, int height)
        {
            if (String.IsNullOrWhiteSpace(file) || !File.Exists(file) || String.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            // create a new email message with embedded image
            using (MailMessage message = new MailMessage())
            {
                message.From = new MailAddress("youremail@domain.com");
                message.To.Add(new MailAddress("toemail@domain.com"));
                message.Subject = "Sharing a snip with you";
                message.IsBodyHtml = true;

                // setup the linked image.
                LinkedResource resource = new LinkedResource(file);
                resource.ContentId = "playImage";
                resource.ContentType = new System.Net.Mime.ContentType("image/png");

                // Create a plain view for clients not supporting html.
                var plainView = AlternateView.CreateAlternateViewFromString("Here is the link: " + url, null,
                    "text/plain");

                // Create a html view for clients supporting html
                const string format =
                    @"<body style='font-family:""Calibri""'><a href={0}><img src=cid:playImage alt-text='{0}' style='border:none;outline:0;width:{1}px;height:{2}px;' border='0' width='{1}' height='{2}'/></a><br/><br/><p><a href={0}>{0}</a></p></body>";
                var htmlView = AlternateView.CreateAlternateViewFromString(String.Format(format, url, width, height),
                    null, "text/html");
                htmlView.LinkedResources.Add(resource);

                // Add both the views to the mail message.
                message.AlternateViews.Add(htmlView);
                message.AlternateViews.Add(plainView);

                // Get the temp file.
                string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".eml");
                while (File.Exists(tempFile))
                {
                    tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".eml");
                }

                // save the message to disk and open it.
                if (SaveMessage(message, tempFile))
                {
                    Process.Start(tempFile);
                    return true;
                }
                else
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                    return false;
                }
            }
        }

        #region Helpers
        /// <summary>
        /// Save message as an EML format.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool SaveMessage(MailMessage message, string fileName)
        {
            try
            {
                // get a temp folder for the email.
                string tempMailFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
                while (Directory.Exists(tempMailFolder))
                {
                    tempMailFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
                }
                Directory.CreateDirectory(tempMailFolder);

                // Write to the temporary folder.
                using (SmtpClient client = new SmtpClient())
                {
                    client.UseDefaultCredentials = true;
                    client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                    client.PickupDirectoryLocation = tempMailFolder;
                    client.Send(message);
                }

                // verify the folder.
                string[] files = Directory.GetFiles(tempMailFolder);
                if (files.Count() != 1 || !files[0].EndsWith(".eml"))
                {
                    return false;
                }

                // Setup the output email file.
                using (var binaryWriter = new BinaryWriter(new FileStream(fileName, FileMode.Create)))
                {
                    //Write the Unsent header to the file so the mail client knows this mail must be presented in "New message" mode
                    binaryWriter.Write(System.Text.Encoding.UTF8.GetBytes("X-Unsent: 1" + Environment.NewLine));
                    binaryWriter.Flush();
                }

                // Copy to the given output file.
                using (StreamReader sr = new StreamReader(new FileStream(files[0], FileMode.Open, FileAccess.Read)))
                using (StreamWriter sw = new StreamWriter(new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.None)))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.ToLower().Contains("youremail@domain.com") || line.ToLower().Contains("toemail@domain.com"))
                        {
                            continue;
                        }
                        else
                        {
                            sw.WriteLine(line);
                        }
                    }
                    sw.Flush();
                }

                // Clean up the directory.
                Directory.Delete(tempMailFolder, true);
                return true;
            }
            catch (Exception ex)
            {
                Diagnostics.LogException(ex);
                return false;
            }
        }

        #endregion
    }
}
