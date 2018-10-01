// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Windows.Forms;
using SnipInsight.Util;

namespace SnipInsight
{
    internal class TrayIcon : IDisposable
    {
        NotifyIcon _icon;
        TrayIconContextMenu _contextMenu;

        internal TrayIcon()
        {
            _contextMenu = new TrayIconContextMenu();

            _icon = new System.Windows.Forms.NotifyIcon();
			_icon.Icon = new System.Drawing.Icon(Properties.Resources.AppIcon, new System.Drawing.Size(16, 16));
			// TODO: Update the tray icon
			_icon.Text = Properties.Resources.TrayIcon_ToolTip;
            _icon.Visible = true;
            _icon.ContextMenuStrip = _contextMenu.contextMenu;
            _icon.Click +=
                delegate(object sender, EventArgs args)
                {
                    if (((MouseEventArgs)args).Button == MouseButtons.Left)
                    {
                        AppManager.TheBoss.ToolWindow.ShowToolWindow(true, true);
                    }
                };
            _icon.BalloonTipClicked +=
                delegate(object sender, EventArgs args)
                {
                    UserSettings.DisableSysTrayBalloonAppStillRunning = true;
                };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~TrayIcon()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_icon != null)
                {
                    _icon.Dispose();
                    _icon = null;
                }

                if (_contextMenu != null)
                {
                    _contextMenu.Dispose();
                    _contextMenu = null;
                }
            }
        }

        internal void Activate()
        {
            ShowBalloonAppIsStillRunning();
        }

        private void ShowBalloonAppIsStillRunning()
        {
            if (!UserSettings.DisableSysTrayBalloonAppStillRunning)
            {
                _icon.BalloonTipTitle = Properties.Resources.TrayIcon_BalloonTip_AppIsStillRunning_Title;
                _icon.BalloonTipText = Properties.Resources.TrayIcon_BalloonTip_AppIsStillRunning_Text;
                _icon.ShowBalloonTip(5000);
            }
        }

    }
}
