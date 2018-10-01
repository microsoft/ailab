// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SnipInsight.Util;

namespace SnipInsight.Views
{
    /// <summary>
    /// Interaction logic for KeyCombinationPicker.xaml
    /// </summary>
    public partial class KeyComboPicker : UserControl
    {
        public KeyComboPicker()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetKeyComboText();
        }

        #region KeyCombo

        public event EventHandler KeyComboChanged;

        public KeyCombo KeyCombo
        {
            get { return GetValue(KeyComboProperty) as KeyCombo; }
            set { SetValue(KeyComboProperty, value); }
        }

        public static readonly DependencyProperty KeyComboProperty =
            DependencyProperty.Register("KeyCombo", typeof(KeyCombo), typeof(KeyComboPicker), new PropertyMetadata(null, OnKeyComboChangedStatic));

        protected virtual void OnKeyComboChanged(KeyCombo value)
        {
            SetKeyComboText();

            if (KeyComboChanged != null)
            {
                KeyComboChanged(this, EventArgs.Empty);
            }
        }

        private static void OnKeyComboChangedStatic(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = d as KeyComboPicker;

            if (self != null)
            {
                self.OnKeyComboChanged(e.NewValue as KeyCombo);
            }
        }

        #endregion

        private KeyCombo _workingKeyCombo;

        private static readonly SolidColorBrush WorkingBrush = new SolidColorBrush(Color.FromArgb(255, 153, 153, 153));
        private static readonly SolidColorBrush ActiveBrush = new SolidColorBrush(Color.FromArgb(255, 200, 207, 225));

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            e.Handled = true;
            // If the System tried to capture the key and handle it, find out
            // what the key originally was...
            if (key == Key.System)
            {
                key = e.SystemKey;
            }

            if (IsKeyRequiredForNavigation(key))
            {
                e.Handled = false;
                return;
            }

            if (e.IsRepeat || !e.IsDown)
            {
                return;
            }

#if (DEBUG)
            System.Diagnostics.Debug.WriteLine("PreviewKeyDown: " + e.Key.ToString() + ", " + e.SystemKey.ToString() + ", " + e.KeyStates.ToString());
#endif

            MessageTextBlock.Text = "";

            if (_workingKeyCombo == null)
            {
                _workingKeyCombo = new KeyCombo();
            }

            if (KeyCombo.IsCtrlKey(key))
            {
                _workingKeyCombo.Ctrl = true;
                SetKeyComboText();
            }
            else if (KeyCombo.IsAltKey(key))
            {
                _workingKeyCombo.Alt = true;
                SetKeyComboText();
            }
            else if (KeyCombo.IsShiftKey(key))
            {
                _workingKeyCombo.Shift = true;
                SetKeyComboText();
            }
            else if (KeyCombo.IsWindowsKey(key))
            {

            }
            else if (key == Key.Delete && !_workingKeyCombo.Shift && !_workingKeyCombo.Alt && !_workingKeyCombo.Ctrl)
            {
                // Allow Delete (by itself) to clear the existing combo
                KeyCombo = null;
                return;
            }
            else
            {
                // Try to set it and see if it works!
                _workingKeyCombo.Key = key;
            }

            HandleAfterKeyPressed();
        }

        private void UserControl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            var key = e.Key;

            e.Handled = true;

            // If the System tried to capture the key and handle it, find out
            // what the key originally was...
            if (key == Key.System)
            {
                key = e.SystemKey;
            }

            if (IsKeyRequiredForNavigation(key))
            {
                return;
            }

            if (e.IsRepeat)
            {
                return;
            }

#if (DEBUG)
            System.Diagnostics.Debug.WriteLine("PreviewKeyUp: " + e.Key.ToString() + ", " + e.SystemKey.ToString());
#endif

            MessageTextBlock.Text = "";

            if (_workingKeyCombo == null)
            {
                if (key == Key.PrintScreen)
                {
                    // The Print Key is special. We need to watch
                    // KeyUp instead of just KeyDown
                    _workingKeyCombo = new KeyCombo();
                }
                else
                {
                    return;
                }
            }

            if (KeyCombo.IsCtrlKey(key))
            {
                _workingKeyCombo.Ctrl = false;
                SetKeyComboText();
            }
            else if (KeyCombo.IsAltKey(key))
            {
                _workingKeyCombo.Alt = false;
                SetKeyComboText();
            }
            else if (KeyCombo.IsShiftKey(key))
            {
                _workingKeyCombo.Shift = false;
                SetKeyComboText();
            }
            else if (key == Key.PrintScreen)
            {
                // The KeyDown for PrintScreen seems to be consumed by
                // Windows and we don't see it. Therefore, we need to
                // treat PrintScreen as a special key that we handle on
                // KeyUp.
                _workingKeyCombo.Key = key;
                HandleAfterKeyPressed();
            }
            else if (_workingKeyCombo.Key == key)
            {
                _workingKeyCombo.Key = Key.None;
                SetKeyComboText();
            }
        }

        private void HandleAfterKeyPressed()
        {
            if (_workingKeyCombo.HasKey)
            {
                SetKeyComboText();

                if (_workingKeyCombo.IsValid == false)
                {
                    MessageTextBlock.Text = SnipInsight.Properties.Resources.KeyComboPicker_NotSupportCombination;
                }
                else
                {
                    MessageTextBlock.Text = SnipInsight.Properties.Resources.KeyComboPicker_Updated;
                    KeyCombo = _workingKeyCombo.Clone();
                    _workingKeyCombo.Key = Key.None;
                }
            }
        }

        private bool IsKeyRequiredForNavigation(Key key)
        {
            switch (key)
            {
                case Key.Tab:
                case Key.OemBackTab:
                    return true;
            }

            return false;
        }

        private void SetKeyComboText()
        {
            KeyCombo workingCombo = _workingKeyCombo;
            KeyCombo activeCombo = KeyCombo;

            if (workingCombo != null && !workingCombo.IsEmpty)
            {
                if (activeCombo != null && activeCombo.Equals(workingCombo))
                {
                    KeyComboTextBox.Foreground = ActiveBrush;
                }
                else
                {
                    KeyComboTextBox.Foreground = WorkingBrush;
                }
                KeyComboTextBox.Text = workingCombo.ToDescriptiveString();
            }
            else
            {
                KeyComboTextBox.Foreground = ActiveBrush;

                string text = null;

                if (activeCombo != null)
                {
                    text = activeCombo.ToDescriptiveString();
                }

                if (string.IsNullOrEmpty(text))
                {
                    text = "None";
                }

                KeyComboTextBox.Text = text;
            }
        }

        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            MessageTextBlock.Text = "";
            _workingKeyCombo = null;
            SetKeyComboText();
        }

        private void ClearButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            KeyCombo = null;
            _workingKeyCombo = null;
            SetKeyComboText();
        }
    }
}
