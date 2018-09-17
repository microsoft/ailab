using System;
using System.Windows.Forms;

namespace SnipInsight
{
    public partial class TrayIconContextMenu : UserControl
    {
        SnipInsight.StateMachine.StateMachine AppStateMachine
        {
            get { return AppManager.TheBoss.ViewModel.StateMachine; }
        }

        public TrayIconContextMenu()
        {
            InitializeComponent();

            menuItemNewCapture.Text = Properties.Resources.TrayIcon_ContextMenuItem_NewCapture;
            menuItemLibrary.Text = Properties.Resources.TrayIcon_ContextMenuItem_Library;
            menuItemSettings.Text = Properties.Resources.TrayIcon_ContextMenuItem_Settings;
            menuItemExit.Text = Properties.Resources.TrayIcon_ContextMenuItem_Exit;
        }

        private void menuItemNewCapture_Click(object sender, EventArgs e)
        {
            AppStateMachine.Fire(StateMachine.SnipInsightTrigger.CaptureScreen);
        }

        private void menuItemNewWhiteboard_Click(object sender, EventArgs e)
        {
            AppStateMachine.Fire(StateMachine.SnipInsightTrigger.Whiteboard);
        }

        private void menuItemNewPhoto_Click(object sender, EventArgs e)
        {
            AppStateMachine.Fire(StateMachine.SnipInsightTrigger.CaptureCamera);
        }

        private void menuItemLibrary_Click(object sender, EventArgs e)
        {
            AppStateMachine.Fire(StateMachine.SnipInsightTrigger.ShowLibraryPanel);
        }

        private void menuItemSettings_Click(object sender, EventArgs e)
        {
            AppStateMachine.Fire(StateMachine.SnipInsightTrigger.ShowSettingsPanel);
        }

        private void menuItemExit_Click(object sender, EventArgs e)
        {
            AppStateMachine.Fire(StateMachine.SnipInsightTrigger.Exit);
        }
    }
}
