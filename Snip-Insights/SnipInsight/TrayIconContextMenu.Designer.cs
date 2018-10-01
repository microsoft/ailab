using SnipInsight.ViewModels;

namespace SnipInsight
{
    partial class TrayIconContextMenu
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuItemNewCapture = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItemLibrary = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItemSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItemExit = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenu.SuspendLayout();
            this.SuspendLayout();
            //
            // contextMenu
            //
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemNewCapture,
            this.toolStripSeparator1,
            this.menuItemLibrary,
            this.toolStripSeparator2,
            this.menuItemSettings,
            this.toolStripSeparator3,
            this.menuItemExit});
            this.contextMenu.Name = "contextMenuStrip1";
            this.contextMenu.Size = new System.Drawing.Size(174, 176);
            //
            // menuItemNewCapture
            //
            this.menuItemNewCapture.Name = "menuItemNewCapture";
            this.menuItemNewCapture.Size = new System.Drawing.Size(173, 22);
            this.menuItemNewCapture.Text = "New Capture #";
            this.menuItemNewCapture.Click += new System.EventHandler(this.menuItemNewCapture_Click);
            // toolStripSeparator1
            //
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(170, 6);
            //
            // menuItemLibrary
            //
            this.menuItemLibrary.Name = "menuItemLibrary";
            this.menuItemLibrary.Size = new System.Drawing.Size(173, 22);
            this.menuItemLibrary.Text = "Library #";
            this.menuItemLibrary.Click += new System.EventHandler(this.menuItemLibrary_Click);
            //
            // toolStripSeparator2
            //
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(170, 6);
            //
            // menuItemSettings
            //
            this.menuItemSettings.Name = "menuItemSettings";
            this.menuItemSettings.Size = new System.Drawing.Size(173, 22);
            this.menuItemSettings.Text = "Settings #";
            this.menuItemSettings.Click += new System.EventHandler(this.menuItemSettings_Click);
            //
            // toolStripSeparator3
            //
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(170, 6);
            //
            // menuItemExit
            //
            this.menuItemExit.Name = "menuItemExit";
            this.menuItemExit.Size = new System.Drawing.Size(173, 22);
            this.menuItemExit.Text = "Exit #";
            this.menuItemExit.Click += new System.EventHandler(this.menuItemExit_Click);
            //
            // TrayIconContextMenu
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Name = "TrayIconContextMenu";
            this.Size = new System.Drawing.Size(321, 236);
            this.contextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem menuItemSettings;
        private System.Windows.Forms.ToolStripMenuItem menuItemExit;
        internal System.Windows.Forms.ToolStripMenuItem menuItemNewCapture;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem menuItemLibrary;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
    }
}
