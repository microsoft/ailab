// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace SnipInsight.Util
{
    public class KeyCombo : IEquatable<KeyCombo>
    {
        public bool Alt { get; set; }
        public bool Shift { get; set; }
        public bool Ctrl { get; set; }

        private Key _key;

        public Key Key
        {
            get { return _key; }
            set
            {
                if (IsValidPrimaryKey(value))
                {
                    _key = value;
                }
                else
                {
                    _key = Key.None;
                }
            }
        }

        public bool HasKey
        {
            get { return Key != Key.None; }
        }

        public bool IsValid
        {
            get
            {
                return HasKey
                    && (!IsSimpleKey(Key) || (Alt && Ctrl))
                    && (!IsDependentKey(Key) || (Alt || Ctrl || Shift));
            }
        }

        public bool IsEmpty
        {
            get { return !(HasKey || Alt || Ctrl || Shift); }
        }

        public int VirtualKeyCode
        {
            get
            {
                return KeyInterop.VirtualKeyFromKey(Key);
            }
        }

        public int KeyModifier
        {
            get
            {
                int keyModifier = 0;
                if (Alt)
                    keyModifier = keyModifier | (int)NativeMethods.HotKeyModifiers.MOD_ALT;
                if (Ctrl)
                    keyModifier = keyModifier | (int)NativeMethods.HotKeyModifiers.MOD_CONTROL;
                if (Shift)
                    keyModifier = keyModifier | (int)NativeMethods.HotKeyModifiers.MOD_SHIFT;

                return keyModifier | (int)NativeMethods.HotKeyModifiers.MOD_NOREPEAT;
            }
        }

        public void Reset()
        {
            Key = Key.None;
            Ctrl = false;
            Alt = false;
            Shift = false;
        }

        public string ToDescriptiveString()
        {
            StringBuilder sb = new StringBuilder(32);

            if (Ctrl)
            {
                sb.Append("Ctrl");
            }

            if (Alt)
            {
                AppendIfNotZeroLength(sb, " + ");
                sb.Append("Alt");
            }

            if (Shift)
            {
                AppendIfNotZeroLength(sb, " + ");
                sb.Append("Shift");
            }

            if (HasKey)
            {
                AppendIfNotZeroLength(sb, " + ");
                if (Key == Key.Snapshot)
                {
                    sb.Append("PrintScreen");
                }
                else
                {
                    sb.Append(Key.ToString());
                }
            }

            return sb.ToString();
        }

        private static void AppendIfNotZeroLength(StringBuilder sb, string text)
        {
            if (sb.Length > 0)
            {
                sb.Append(text);
            }
        }

        #region Storage String

        public override string ToString()
        {
            if (!IsValid)
            {
                return "";
            }
            else
            {
                StringBuilder sb = new StringBuilder(32);

                if (Ctrl)
                {
                    sb.Append("Ctrl");
                }

                if (Alt)
                {
                    AppendIfNotZeroLength(sb, "+");
                    sb.Append("Alt");
                }

                if (Shift)
                {
                    AppendIfNotZeroLength(sb, "+");
                    sb.Append("Shift");
                }

                if (HasKey)
                {
                    AppendIfNotZeroLength(sb, "+");
                    sb.Append(Key.ToString());
                }

                return sb.ToString();
            }
        }

        public static KeyCombo ParseOrDefault(string s)
        {
            KeyCombo combo = new KeyCombo();

            if (!string.IsNullOrEmpty(s))
            {
                string[] parts = s.ToLowerInvariant().Split('+');

                Key key;

                if (Enum.TryParse<Key>(parts[parts.Length - 1].Trim(), true, out key))
                {
                    combo.Key = key;

                    combo.Ctrl = parts.Contains("ctrl");
                    combo.Alt = parts.Contains("alt");
                    combo.Shift = parts.Contains("shift");

                    if (!combo.IsValid)
                    {
                        combo.Reset();
                    }
                }
            }

            return combo;
        }

        #endregion

        #region Key Categories
        public static bool IsValidPrimaryKey(Key key)
        {
            switch (key)
            {
                case Key.Apps: // Properties
                case Key.System:
                case Key.Sleep:
                case Key.None:
                case Key.NoName:
                case Key.JunjaMode:
                case Key.KanaMode:
                case Key.KanjiMode:
                case Key.ImeAccept:
                case Key.ImeConvert:
                case Key.ImeModeChange:
                case Key.ImeNonConvert:
                case Key.ImeProcessed:
                case Key.DeadCharProcessed:
                case Key.NumLock:
                case Key.OemSemicolon:
                case Key.OemOpenBrackets:
                case Key.CapsLock:
                    return false;
            }

            if (IsDbeKey(key))
                return false;

            return true;
        }

        /// <summary>
        /// Returns a value indicating that they required both CTRL + ALT
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool IsSimpleKey(Key key)
        {
            return key >= Key.A && key <= Key.Z
                || key >= Key.D0 && key <= Key.D9
                || IsSimpleKeyExtended(key);

        }

        public bool IsDependentKey(Key key)
        {
            switch (key)
            {
                case Key.Up:
                case Key.Down:
                case Key.Left:
                case Key.Right:
                case Key.PageUp:
                case Key.PageDown:
                    return true;
                default:
                    return false;
            }
        }

        private bool IsSimpleKeyExtended(Key key)
        {
            switch (key)
            {
                case Key.Enter:
                case Key.Space:
                    return true;
            }

            if (IsOemKey(key))
                return true;

            return false;
        }

        public static bool IsShiftKey(Key key)
        {
            return key == Key.LeftShift || key == Key.RightShift;
        }

        public static bool IsCtrlKey(Key key)
        {
            return key == Key.LeftCtrl || key == Key.RightCtrl;
        }

        public static bool IsAltKey(Key key)
        {
            return key == Key.LeftAlt || key == Key.RightAlt;
        }

        public static bool IsWindowsKey(Key key)
        {
            return key == Key.LWin || key == Key.RWin;
        }

        private static bool IsDbeKey(Key key)
        {
            switch (key)
            {

                case Key.DbeAlphanumeric:
                case Key.DbeCodeInput:
                case Key.DbeDbcsChar:
                case Key.DbeEnterDialogConversionMode:
                case Key.DbeEnterImeConfigureMode:
                case Key.DbeEnterWordRegisterMode:
                case Key.DbeFlushString:
                case Key.DbeHiragana:
                case Key.DbeKatakana:
                case Key.DbeNoCodeInput:
                case Key.DbeNoRoman:
                case Key.DbeRoman:
                case Key.DbeSbcsChar:
                    return true;
            }

            return false;
        }

        private static bool IsOemKey(Key key)
        {
            // Note: OEM Keys appear more than once in the enum,
            // so you won't see all values listed below intentionally.

            switch (key)
            {
                case Key.OemAttn:
                case Key.OemAuto:
                case Key.OemBackslash:
                case Key.OemBackTab:
                case Key.OemClear:
                case Key.OemCloseBrackets:
                case Key.OemComma:
                case Key.OemCopy:
                case Key.OemEnlw:
                case Key.OemFinish:
                case Key.OemMinus:
                case Key.OemOpenBrackets:
                case Key.OemPeriod:
                case Key.OemPipe:
                case Key.OemPlus:
                case Key.OemQuestion:
                case Key.OemQuotes:
                case Key.OemSemicolon:
                case Key.OemTilde:
                    return true;
            }

            return false;
        }

        #endregion

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj is KeyCombo)
            {
                return Equals((KeyCombo)obj);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(KeyCombo other)
        {
            return this.Key == other.Key
                    && this.Alt == other.Alt
                    && this.Ctrl == other.Ctrl
                    && this.Shift == other.Shift;
        }

        public override int GetHashCode()
        {
            // This is good enough. We don't current plan
            // to hash this object.
            return (int)Key;
        }

        #endregion

        #region Clone

        public KeyCombo Clone()
        {
            return new KeyCombo()
            {
                Key = this.Key,
                Ctrl = this.Ctrl,
                Alt = this.Alt,
                Shift = this.Shift
            };
        }

        #endregion
    }
}
