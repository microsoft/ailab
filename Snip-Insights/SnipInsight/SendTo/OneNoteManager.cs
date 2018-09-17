// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using OneNote = Microsoft.Office.Interop.OneNote;
using SnipInsight.Util;
using System.Windows.Interop;
using System.Diagnostics;
using System.Threading;
using Microsoft.Win32;

namespace SnipInsight.SendTo
{
    internal class OneNoteEntity
    {
        internal string Id;
        internal string Name;

        internal OneNoteEntity(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    internal class OneNoteSection : OneNoteEntity
    {
        internal OneNoteSection(string id, string name)
            : base(id, name)
        {
        }
    }

    internal class OneNotePage : OneNoteEntity
    {
        internal OneNotePage(string id, string name)
            : base(id, name)
        {
        }
    }

    internal class OneNoteManager : OneNote.IQuickFilingDialogCallback, IDisposable
    {
        // Application interface (OneNote 2013) --> https://msdn.microsoft.com/en-us/library/office/jj680120.aspx
        // Note
        // We recommend specifying a version of OneNote (such as xs2013) instead of using xsCurrent or leaving it blank, because this will allow your add-in to work with future versions of OneNote.

        OneNote.Application _oneNoteApp;
        const OneNote.XMLSchema _oneNoteXmlSchema = OneNote.XMLSchema.xs2013;
        OneNoteXmlDoc _oneNoteXml;
        string _imageFilePath;
        string _publishUrl;
        ManualResetEvent _insertCompleteEvent;
        Exception _insertException;
        bool _cancelled;

        ~OneNoteManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _oneNoteApp = null; // there is no Dispose or Close

                if (_insertCompleteEvent != null)
                {
                    _insertCompleteEvent.Dispose();
                    _insertCompleteEvent = null;
                }
            }
        }

        private bool EnsureSnipOneNoteElevationParity()
        {
            Process oneNoteProcess = GetRunningOneNoteProcess();
            if (oneNoteProcess != null)
            {
                bool snipElevated = Utils.IsProcessRunningElevated(Process.GetCurrentProcess());
                bool oneNoteElevated = Utils.IsProcessRunningElevated(oneNoteProcess);

                if (snipElevated != oneNoteElevated)
                {
                    string elevatedApp = snipElevated ? "Snip" : "OneNote";
                    string unelevatedApp = snipElevated ? "OneNote" : "Snip";

                    string message = string.Format(Properties.Resources.SendToOneNoteProcessElevationMismatchDiaglogMessage, elevatedApp, unelevatedApp);

                    MessageBox.Show(message, Properties.Resources.SendToOneNoteProcessElevationMismatchDiaglogTitle, MessageBoxButton.OK);

                    return false;
                }
            }

            return true;
        }

        private bool EnsureOneNoteFirstRunComplete()
        {
            bool firstRunComplete = false;
            try
            {
                string[] officeMajorVersions = { "16", "15" }; // search order preference is newer Office versions first
                const string firstBootRegkeyTemplate = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Office\{0}.0\OneNote";

                foreach (string officeMajorVersion in officeMajorVersions)
                {
                    object firstBootStatus = Registry.GetValue(string.Format(firstBootRegkeyTemplate, officeMajorVersion), "FirstBootStatus", null);

                    if (firstBootStatus != null && (int)firstBootStatus >= 0x01000101) // regkey will not exist if OneNote has never been used. Regvalue is set to 0x01000101 (Office 15) or higher (i.e. 0x02000202 (Office 16)) once OneNote has been configured
                    {
                        firstRunComplete = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Diagnostics.LogLowPriException(new ApplicationException("Error querying registry for OneNote FirstRun", ex));
            }

            if (!firstRunComplete)
            {
                MessageBox.Show(Properties.Resources.SendToOneNoteFirstRunNotCompleteDialogMessage, Properties.Resources.SendToOneNoteFirstRunNotCompleteDialogTitle, MessageBoxButton.OK);
            }

            return firstRunComplete;
        }

        private void InitializeOneNoteApplication()
        {
            // create the OneNote Application with retry since starting the Application\OneNote.exe process can fail if a running OneNote process is in the midst of shutting down just as we try to initialize
            uint attemptCount = 0;
            uint attemptCountMax = 3;

            do
            {
                try
                {
                    attemptCount++;

                    _oneNoteApp = new OneNote.Application();

                    break;
                }
                catch (Exception)
                {
                    if (attemptCount == attemptCountMax)
                    {
                        throw;
                    }

                    System.Threading.Thread.Sleep(1000); // give a second to let OneNote exit completely
                }
            } while (attemptCount < attemptCountMax);
        }

        // Initialize outside the constructor so that we can return bool 'success' to the caller of the class
        private bool Initialize(string imageFilePath, string publishUrl)
        {
            if (EnsureSnipOneNoteElevationParity() && EnsureOneNoteFirstRunComplete())
            {
                InitializeOneNoteApplication();
                _oneNoteXml = new OneNoteXmlDoc(_oneNoteXmlSchema);

                _imageFilePath = imageFilePath;
                _publishUrl = publishUrl;
                _insertCompleteEvent = new ManualResetEvent(false);
                _insertException = null;
                _cancelled = false;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Inserts the snip into OneNote
        /// </summary>
        /// <param name="imageFilePath"></param>
        /// <param name="publishUrl"></param>
        /// <returns>bool on success or failure. Null if the user cancelled</returns>
        internal bool? InsertSnip(string imageFilePath, string publishUrl = null)
        {
            // Quick Filing dialog box interfaces (OneNote 2013) --> https://msdn.microsoft.com/EN-US/library/office/jj680122.aspx

            try
            {
                if (!Initialize(imageFilePath, publishUrl))
                {
                    return false;
                }

                OneNote.IQuickFilingDialog qfDialog = _oneNoteApp.QuickFiling();

                qfDialog.Title = Properties.Resources.SendToOneNoteDialogTitle;
                qfDialog.Description = Properties.Resources.SendToOneNoteDialogDescription;
                qfDialog.TreeDepth = OneNote.HierarchyElement.hePages;
                qfDialog.ParentWindowHandle = (ulong)new WindowInteropHelper(AppManager.TheBoss.MainWindow).Handle.ToInt64(); // sets the dialog modal to the editor window

                qfDialog.SetRecentResults(OneNote.RecentResultType.rrtFiling, true, true, false);

                // add a 'Send To' button, it will have index 0 since its the first button we added
                qfDialog.AddButton(Properties.Resources.SendToOneNoteDialogSendToButton, OneNote.HierarchyElement.heSections | OneNote.HierarchyElement.hePages, OneNote.HierarchyElement.heNone, true);

                // watch for the OneNote process to exit since the QuickFilingDialog is modal to the MainWindow, which will cause Snip to hang if OneNote is killed while the QuickFilingDialog is being displayed
                WatchForOneNoteProcessExit();

                qfDialog.Run(this); // OnDialogClosed() callback is called just after the user makes a selction and finishes running the dialog

                // wait for the OnDialogClosed() callback thread to complete the insert
                _insertCompleteEvent.WaitOne();

                if (_cancelled)
                {
                    return null;
                }

                // check for an exception raised from the Insert code ran in the OnDialogClosed() callback thread
                if (_insertException != null)
                {
                    Diagnostics.LogException(_insertException);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Diagnostics.LogException(ex);
                return false;
            }
        }

        private void WatchForOneNoteProcessExit()
        {
            new Thread(WatchForOneNoteProcessExitThreadProc)
            {
                IsBackground = true // allow the process to terminate if this thread is still running
            }
            .Start();
        }

        /// <summary>
        /// Watches for the OneNote process to exit and prompts the user to restart Snip Insights if the OneNote process is killed while the QuickFilingDialog is being displayed.
        /// This fcn is hang recovery code
        /// </summary>
        void WatchForOneNoteProcessExitThreadProc()
        {
            try
            {
                Process oneNoteProcess = GetRunningOneNoteProcess();

                if (oneNoteProcess != null)
                {
                    while (_insertCompleteEvent != null && !_insertCompleteEvent.WaitOne(0) && !oneNoteProcess.HasExited) // wait until the OnDialogClosed() callback finishes the insert and signals or the OneNote process exits
                    {
                        Thread.Sleep(500);
                    }

                    if (oneNoteProcess.HasExited && _insertCompleteEvent != null && !_insertCompleteEvent.WaitOne(0))
                    {
                        // OneNote QuickFilingDialog is modal to the MainWindow and if OneNote gets killed the Snip Insights MainWindow will be hung
                        // prompt the user to restart Snip Insights 
                        MessageBox.Show(Properties.Resources.SendToOneNoteRestartSnipDialogMessage, Properties.Resources.SendToOneNoteRestartSnipDialogTitle, MessageBoxButton.OK);

                        AppManager.TheBoss.RestartApp(true); // 'true' to kill the app since it will not gracefully close if the MainWindow is hung
                    }
                }
            }
            catch
            {}
        }

        private Process GetRunningOneNoteProcess()
        {
            // get the OneNote process that is running in the same Windows session as the Snip Insights. OneNote is a single instance app per session.
            return Process.GetProcessesByName("OneNote").Where(p => p.SessionId == Process.GetCurrentProcess().SessionId).FirstOrDefault();
        }

        public void OnDialogClosed(OneNote.IQuickFilingDialog qfDialog)
        {
            try
            {
                if (qfDialog.PressedButton == 0) // 0 is the index for the 'Send To' button we added to the dialog
                {
                    InsertSnipIntoSelectedObject(qfDialog.SelectedItem);
                }
                else
                {
                    _cancelled = true;
                }
            }
            finally
            {
                _insertCompleteEvent.Set();
            }
        }

        private void InsertSnipIntoSelectedObject(string oneNoteObjectId)
        {
            try
            {
                string hierarchyXml;
                _oneNoteApp.GetHierarchy(oneNoteObjectId, OneNote.HierarchyScope.hsSelf, out hierarchyXml, _oneNoteXmlSchema);

                OneNoteEntity entity = _oneNoteXml.GetEntity(hierarchyXml);

                if (entity is OneNoteSection)
                {
                    InsertImageIntoSection((OneNoteSection)entity, _imageFilePath, _publishUrl);
                }
                else if (entity is OneNotePage)
                {
                    InsertImageIntoPage((OneNotePage)entity, _imageFilePath, _publishUrl);
                }
            }
            catch (Exception ex)
            {
                _insertException = ex; // will be reported in InsertSnip() after this fcn returns
            }
        }

        internal void InsertImageIntoSection(OneNoteSection section, string imageFilePath, string publishUrl)
        {
            string newPageId;
            _oneNoteApp.CreateNewPage(section.Id, out newPageId, OneNote.NewPageStyle.npsDefault);

            OneNotePage newPage = GetPageById(newPageId);

            InsertImageIntoPage(newPage, imageFilePath, publishUrl, true);
        }

        internal void InsertImageIntoPage(OneNotePage page, string imageFilePath, string publishUrl, bool addTitle = false)
        {
            string xmlToInsert = _oneNoteXml.GetInsertSnipXml(page, GetBase64ImageString(imageFilePath), publishUrl, addTitle);

            _oneNoteApp.UpdatePageContent(xmlToInsert, System.DateTime.MinValue, _oneNoteXmlSchema);

            // navigate to inserted Snip
            string pageContentXmlAfterInsert;
            _oneNoteApp.GetPageContent(page.Id, out pageContentXmlAfterInsert, OneNote.PageInfo.piBasic, _oneNoteXmlSchema);

            string insertedSnipObjectId = _oneNoteXml.GetInsertedSnipObjectId(pageContentXmlAfterInsert);

            _oneNoteApp.NavigateTo(page.Id, insertedSnipObjectId);
        }

        private string GetBase64ImageString(string imageFilePath)
        {
            using (Bitmap image = new Bitmap(imageFilePath))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    image.Save(ms, ImageFormat.Png);
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        internal OneNotePage GetPageById(string pageId)
        {
            string hierarchyXml;
            _oneNoteApp.GetHierarchy(pageId, OneNote.HierarchyScope.hsSelf, out hierarchyXml, _oneNoteXmlSchema);

            return (OneNotePage)_oneNoteXml.GetEntity(hierarchyXml);
        }
    }

    internal class OneNoteXmlDoc
    {
        const OneNote.XMLSchema _oneNoteXmlSchema = OneNote.XMLSchema.xs2013;
        XmlDocument _xmlDoc;
        XmlNamespaceManager _namespaceMgr;
        static readonly string NamespaceUri = "http://schemas.microsoft.com/office/onenote/2013/onenote";
        static readonly string NamespacePrefix = "one";
        static readonly string SectionNodeName = string.Format("{0}:Section", NamespacePrefix);
        static readonly string PageNodeName = string.Format("{0}:Page", NamespacePrefix);
        static readonly string OutlineNodeName = string.Format("{0}:Outline", NamespacePrefix);
        static readonly string SectionNodeXPath = string.Format("//{0}", SectionNodeName);
        static readonly string PageNodeXPath = string.Format("//{0}", PageNodeName);
        static readonly string OutlineNodeXPath = string.Format("//{0}/{1}", PageNodeName, OutlineNodeName);

        internal OneNoteXmlDoc(OneNote.XMLSchema xmlSchema)
        {
            if (xmlSchema != _oneNoteXmlSchema)
            {
                throw new NotSupportedException(string.Format("The only supported xmlSchema is {0}", _oneNoteXmlSchema));
            }

            _xmlDoc = new XmlDocument();

            _namespaceMgr = new XmlNamespaceManager(_xmlDoc.NameTable);
            _namespaceMgr.AddNamespace(NamespacePrefix, NamespaceUri);
        }

         internal OneNoteEntity GetEntity(string hierarchyXml)
        {
            _xmlDoc.LoadXml(hierarchyXml);

            XmlElement rootNode = _xmlDoc.DocumentElement;

            OneNoteEntity entity = GetEntity(rootNode);

            if (string.Compare(rootNode.Name, SectionNodeName, true) == 0)
            {
                return new OneNoteSection(entity.Id, entity.Name);
            }
            else if (string.Compare(rootNode.Name, PageNodeName, true) == 0)
            {
                return new OneNotePage(entity.Id, entity.Name);
            }
            else
            {
                throw new ApplicationException(string.Format("Invalid OneNote xml. Unable to create OneNoteEntity. XML: {0}", hierarchyXml));
            }
        }

        private OneNoteEntity GetEntity(XmlNode node)
        {
            string id = node.Attributes["ID"].Value;
            string name = node.Attributes["name"].Value;

            return new OneNoteEntity(id, name);
        }

        internal string GetInsertSnipXml(OneNotePage page, string base64ImageString, string publishUrl, bool addTitle)
        {
            const string xmlFormat =
                "<?xml version=\"1.0\"?>" +
                "<one:Page xmlns:one=\"http://schemas.microsoft.com/office/onenote/2013/onenote\" ID=\"{0}\">" +  // format {0} - page.ID
                    "{1}" +                                                                                       // format {1} - titleXml
                    "<one:Outline>" +
                        "<one:OEChildren>" +
                            "<one:OE>" +
                                "<one:Image format=\"png\" {2}>" +                                                // format {2} - imageHyperlinkAttribure
                                    "<one:Data>{3}</one:Data>" +                                                  // format {3} - base64ImageString
                                "</one:Image>" +
                            "</one:OE>" +
                            "{4}" +                                                                               // format {4} - hyperlinkOutlineXml
                            "<one:OE>" +
                                "<one:T/>" +
                            "</one:OE>" +
                            "<one:OE>" +
                                "<one:T/>" +
                            "</one:OE>" +
                            "<one:OE>" +
                                "<one:T>" +
                                    "<![CDATA[{5}]]>" +                                                           // format {5} - caption
                                "</one:T>" +
                            "</one:OE>" +
                            "<one:OE>" +
                                "<one:T/>" +
                            "</one:OE>" +
                        "</one:OEChildren>" +
                    "</one:Outline>" +
                "</one:Page>";

            string caption = string.Format("{0} {1} - Snip", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
            string titleXmlFormat =
                    "<one:Title>" +
                        "<one:OE>" +
                            "<one:T>" +
                                "<![CDATA[{0}]]>" +                                                               // format {0} - caption
                            "</one:T>" +
                        "</one:OE>" +
                    "</one:Title>";
            string titleXml = addTitle ? string.Format(titleXmlFormat, caption) : "";
            string imageHyperlinkAttribute = !string.IsNullOrWhiteSpace(publishUrl) ? string.Format("hyperlink=\"{0}\"", publishUrl) : "";
            const string hyperlinkOutlineXmlFormat =
                            "<one:OE>" +
                                "<one:T/>" +
                            "</one:OE>" +
                            "<one:OE>" +
                                "<one:T>" +
                                    "<![CDATA[{0}]]>" +                                                           // format {0} - publishUrl
                                "</one:T>" +
                            "</one:OE>";
            string hyperlinkOutlineXml = !string.IsNullOrWhiteSpace(publishUrl) ? string.Format(hyperlinkOutlineXmlFormat, publishUrl) : "";

            return string.Format(xmlFormat, page.Id, titleXml, imageHyperlinkAttribute, base64ImageString, hyperlinkOutlineXml, caption);
        }

        internal string GetInsertedSnipObjectId(string pageContentXmlAfterInsert)
        {
            _xmlDoc.LoadXml(pageContentXmlAfterInsert);

            // return the objectID of the last Outline element

            XmlNodeList outlineElements = _xmlDoc.SelectNodes(OutlineNodeXPath, _namespaceMgr);

            return outlineElements[outlineElements.Count - 1].Attributes["objectID"].Value;
        }
    }
}
