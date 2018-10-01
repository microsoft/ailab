// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using SnipInsight.Controls.Ariadne;
using SnipInsight.ViewModels;

namespace SnipInsight.Views
{
    /// <summary>
    /// Interaction logic for EditorSideNavigation.xaml
    /// </summary>
    public partial class EditorSideNavigation : UserControl
    {

        private AriInkRadioButton lastCheckedInkButton = null;
        private int lastCheckedPenSize;

        /// <summary>
        /// Defines if the pen was already checked
        /// </summary>
        public EditorSideNavigation()
        {
            InitializeComponent();

            lastCheckedPenSize = (int)AppManager.TheBoss.ViewModel.InkDrawingAttributes.Width;
            HighlightPenSize(lastCheckedPenSize);
        }

        private void ColorButton_Checked(object sender, RoutedEventArgs e)
        {
            AriInkRadioButton button = sender as AriInkRadioButton;

            if (button == null)
            {
                return;
            }

            //switch (button.Ink.ToString())
            //{
            //    case "Black":
            //        Telemetry.ApplicationLogger.Instance.SubmitButtonClickEvent(Telemetry.EventName.BlackColorToggle, Telemetry.ViewName.EditorSideNavigation);
            //        break;
            //    case "Red":
            //        Telemetry.ApplicationLogger.Instance.SubmitButtonClickEvent(Telemetry.EventName.RedColorToggle, Telemetry.ViewName.EditorSideNavigation);
            //        break;
            //    case "Orange":
            //        Telemetry.ApplicationLogger.Instance.SubmitButtonClickEvent(Telemetry.EventName.YellowColorToggle, Telemetry.ViewName.EditorSideNavigation);
            //        break;
            //    case "Green":
            //        Telemetry.ApplicationLogger.Instance.SubmitButtonClickEvent(Telemetry.EventName.GreenColorToggle, Telemetry.ViewName.EditorSideNavigation);
            //        break;
            //    case "Blue":
            //        Telemetry.ApplicationLogger.Instance.SubmitButtonClickEvent(Telemetry.EventName.BlueColorToggle, Telemetry.ViewName.EditorSideNavigation);
            //        break;
            //}

            lastCheckedInkButton = button;

            SetInkColor(button);
        }

        private void SetInkColor(AriInkRadioButton button)
        {
            var brush = button.Ink as SolidColorBrush;
            var model = DataContext as SnipInsightViewModel;

            if (brush == null)
            {
                //Diagnostics.LogTrace("Fail to set ink color. brush is null");
                return;
            }

            if (model == null)
            {
                return;
            }

            model.InkDrawingAttributes.Color = brush.Color;
            model.InkDrawingAttributes.Width = lastCheckedPenSize;
            model.InkDrawingAttributes.Height = lastCheckedPenSize;
            model.InkDrawingAttributes.IsHighlighter = false;
            model.InkDrawingAttributes.StylusTip = System.Windows.Ink.StylusTip.Ellipse;
            model.InkModeRequested = InkCanvasEditingMode.Ink;
        }

        private void PenSizeButton_Click(object sender, RoutedEventArgs e)
        {
            int ptSize = 3;

            SnipInsightViewModel model = DataContext as SnipInsightViewModel;
            if (model == null)
            {
                return;
            }

            if (sender == PenSize1Button)
            {
                Telemetry.ApplicationLogger.Instance.SubmitButtonClickEvent(Telemetry.EventName.PenSize1Button, Telemetry.ViewName.EditorSideNavigation);
                ptSize = 1;
            }
            else if (sender == PenSize3Button)
            {
                Telemetry.ApplicationLogger.Instance.SubmitButtonClickEvent(Telemetry.EventName.PenSize3Button, Telemetry.ViewName.EditorSideNavigation);
                ptSize = 3;
            }
            else if (sender == PenSize5Button)
            {
                Telemetry.ApplicationLogger.Instance.SubmitButtonClickEvent(Telemetry.EventName.PenSize5Button, Telemetry.ViewName.EditorSideNavigation);
                ptSize = 5;
            }
            else if (sender == PenSize9Button)
            {
                Telemetry.ApplicationLogger.Instance.SubmitButtonClickEvent(Telemetry.EventName.PenSize9Button, Telemetry.ViewName.EditorSideNavigation);
                ptSize = 9;
            }

            if (ptSize != 0)
            {
                model.InkDrawingAttributes.Width = ptSize;
                model.InkDrawingAttributes.Height = ptSize;
                model.InkDrawingAttributes.IsHighlighter = false;
                model.InkDrawingAttributes.StylusTip = System.Windows.Ink.StylusTip.Ellipse;
                model.InkModeRequested = InkCanvasEditingMode.Ink;
                lastCheckedPenSize = ptSize;

                HighlightPenSize(ptSize);

                //var currentInkButton = LeftBar.Children.OfType<AriInkRadioButton>().FirstOrDefault(t => t.IsChecked.HasValue && t.IsChecked.Value);
                //if (currentInkButton == null)
                //{
                //    lastCheckedInkButton.IsChecked = true;
                //}
            }
        }

        private void HighlightPenSize(int ptSize)
        {
            HighlightPenSize(PenSize1Shape, ptSize == 1);
            HighlightPenSize(PenSize3Shape, ptSize == 3);
            HighlightPenSize(PenSize5Shape, ptSize == 5);
            HighlightPenSize(PenSize9Shape, ptSize == 9);
        }

        private readonly SolidColorBrush HighlightedPenSizeBrush = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
        private readonly SolidColorBrush NormalPenSizeBrush = new SolidColorBrush(Color.FromArgb(255, 204, 204, 204));

        private void HighlightPenSize(Shape shapeObject, bool isSelected)
        {
            shapeObject.Fill = isSelected ? HighlightedPenSizeBrush : NormalPenSizeBrush;
        }

        private readonly Color[] highlighterColors = { new Color() { R = 255, G = 255, B = 0, A = 127 }, new Color() { R = 128, G = 255, B = 0, A = 127 }, new Color() { R = 255, G = 0, B = 255, A = 127 } };

        private void PenButton_Click(object sender, RoutedEventArgs e)
        {
            if (lastCheckedInkButton == null)
            {
                lastCheckedInkButton = BlackColorButton;
            }

            SnipInsightViewModel model = DataContext as SnipInsightViewModel;
            if (model == null)
            {
                return;
            }

            if (model.penSelected)
            {
                PenPalettePopup.IsOpen = true;
            }
            else
            {
                model.penSelected = true;
                SetInkColor(lastCheckedInkButton);
            }
        }

        private void Eraser_Click(object sender, RoutedEventArgs e)
        {
            AppManager.TheBoss.OnEraser();
        }

        /// <summary>
        /// Event trigger to move the next element to the black color button if pen palette is open
        /// </summary>
        private void PenToggleLostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (PenPalettePopup.IsOpen)
            {
                BlackColorButton.Focus();
            }
        }

        /// <summary>
        /// Event trigger to move to the highlighter once we move past the pen sizes
        /// </summary>
        private void PenSizeLostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            HighlighterButton.Focus();
        }

        private void Highlighter_Checked(object sender, RoutedEventArgs e)
        {
            SnipInsightViewModel model = DataContext as SnipInsightViewModel;
            if (model == null)
            {
                return;
            }

            model.InkDrawingAttributes.Width = 6;
            model.InkDrawingAttributes.Height = 20;
            model.InkDrawingAttributes.IsHighlighter = true;
            model.InkDrawingAttributes.Color = highlighterColors[0];
            model.InkDrawingAttributes.StylusTip = System.Windows.Ink.StylusTip.Rectangle;
            model.InkModeRequested = InkCanvasEditingMode.Ink;

            AppManager.TheBoss.ResetEditorButtons(AppManager.EditorTools.Highlighter);
        }

        private void PenButton_Check(object sender, RoutedEventArgs e)
        {
            AppManager.TheBoss.ResetEditorButtons(AppManager.EditorTools.Pen);
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            AriInkRadioButton button = sender as AriInkRadioButton;

            if (button == null)
            {
                return;
            }

            switch (button.Ink.ToString())
            {
                case "Black":
                    Telemetry.ApplicationLogger.Instance.SubmitButtonClickEvent(Telemetry.EventName.BlackColorToggle, Telemetry.ViewName.EditorSideNavigation);
                    break;
                case "Red":
                    Telemetry.ApplicationLogger.Instance.SubmitButtonClickEvent(Telemetry.EventName.RedColorToggle, Telemetry.ViewName.EditorSideNavigation);
                    break;
                case "Orange":
                    Telemetry.ApplicationLogger.Instance.SubmitButtonClickEvent(Telemetry.EventName.YellowColorToggle, Telemetry.ViewName.EditorSideNavigation);
                    break;
                case "Green":
                    Telemetry.ApplicationLogger.Instance.SubmitButtonClickEvent(Telemetry.EventName.GreenColorToggle, Telemetry.ViewName.EditorSideNavigation);
                    break;
                case "Blue":
                    Telemetry.ApplicationLogger.Instance.SubmitButtonClickEvent(Telemetry.EventName.BlueColorToggle, Telemetry.ViewName.EditorSideNavigation);
                    break;
            }

            lastCheckedInkButton = button;

            SetInkColor(button);
        }
    }
}

