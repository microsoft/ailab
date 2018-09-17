// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Microsoft.Win32;
using System;
using System.Text;
using System.Security.Cryptography;

namespace SnipInsight.Util
{
    internal static class UserSettings
    {
        #region Properties
        static readonly byte[] SAditionalEntropy = { 0, 1, 11, 111, 2, 22, 222 };

        const string _oldSettingsSubkey = @"Software\Microsoft\SnipInsight";

        const RegistryHive _settingsHive = RegistryHive.CurrentUser;
        const string _settingsSubkey = @"Software\Microsoft\Snip";

        const string _requestIdName = @"RequestId";
        static Guid? _requestId = null;

        const string _appIdName = @"AppId";
        static string _appId = null;

        const string _fallbackEndpointsName = @"FallbackEndpoints";

        // app version that showed a first run window.
        const string _versionName = @"Version";
        static string _version = null;

        // set true if a user explicitly requests.
        const string _disableFirstRunName = @"DisableFirstRun";
        static bool? _disableFirstRun = null;

        // set true if a user explicitly requests.
        const string _disableEditorWindowTourName = @"DisableEditorWindowTour";
        static bool? _disableEditorWindowTour = null;

        static string _setupDownloadPath = null;
        const string _setupDownloadPathName = @"SetupDownloadPath";

        static string _appPath = null;
        const string _appPathName = @"AppPath";

        static bool? _allowProxyForAutoUpdate = null;
        const string _allowProxyForAutoUpdateName = @"AllowProxyForAutoUpdate";

        static string _mainWindowLocation = null;
        const string _mainWindowLocationName = @"MainWindowLocation";

        static string _screenCaptureShortcut = null;
        const string _screenCaptureShortcutName = @"ScreenCaptureShortcut";

        static string _quickCaptureShortcut = null;
        const string _quickCaptureShortcutName = @"QuickCaptureShortcut";

        static string _libraryShortcut = null;
        const string _libraryShortcutName = @"LibraryShortcut";

        static int? _screenCaptureDelay = 0;
        const string _screenCaptureDelayName = @"ScreenCaptureDelay";

        static bool? _disableRunWithWindows = null;
        const string _disableRunWithWindowsName = @"DisableRunWithWindows";

        static bool? _disableSysTrayBalloonAppStillRunning = null;
        const string _disableSysTrayBalloonAppStillRunningName = @"DisableSysTrayBalloonAppStillRunning";

        static bool? _disableToolWindow = null;
        const string _disableToolWindowName = @"DisableToolWindow";

        const string _disableKeyCombo = "None";

        static bool? _autoUpdate = null;
        const string _autoUpdateName = @"AutoUpdate";

        static string _autoUpdateLogPath = null;
        const string _autoUpdateLogPathName = @"AutoUpdateLogPath";

        static bool? _isNotificationToastEnabled = null;
        const string _isNotificationToastEnabledName = @"NotificationToastEnabled";

        static string _customDirectory = null;
        const string _customDirectoryName = @"CustomDirectory";

        static bool? _isOpenEditorPostSnip = null;
        const string _isOpenEditorPostSnipName = @"OpenEditorPostSnip";

        static bool? _copyToClipboardAfterSnip = null;
        const string _copyToClipboardAfterSnipName = @"CopyToClipboard";

        static int? _contentModerationStrength = 0;
        const string _contentModerationStrengthName = @"ContentModeration";

        static bool? _isAIEnabled = null;
        const string _isAIEnabledName = @"IsAIEnabled";

        static bool? _isAutoTaggingEnabled = true;
        const string _isAutoTaggingEnabledName = @"IsAutoTaggingEnabled";

        static string _key = null;
        #endregion

        #region Methods
        static SettingsRegKey _settingsKey = null;

        static SettingsRegKey SettingsKey
        {
            get
            {
                if (_settingsKey == null)
                {
                    try
                    {
                        _settingsKey = new SettingsRegKey(_settingsHive, _settingsSubkey);
                    }
                    catch (Exception e)
                    {
                        Diagnostics.LogTrace(string.Format("Unable to create SettingsRegKey object for {0}\\{1}", _settingsHive, _settingsSubkey));
                        Diagnostics.LogLowPriException(e);
                    }
                }

                return _settingsKey;
            }
        }

        internal static Guid RequestId
        {
            get
            {
                if (_requestId != null)
                {
                    return _requestId.Value;
                }

                try
                {
                    object regValue = SettingsRegKey.GetValue(SettingsKey, _requestIdName, null);

                    Guid requestId;
                    if (regValue is string && Guid.TryParse((string)regValue, out requestId) && requestId != Guid.Empty)
                    {
                        // Request Id was read from registry
                        _requestId = requestId;
                    }
                    else
                    {
                        _requestId = Guid.NewGuid();
                        SettingsRegKey.SetValue(SettingsKey, _requestIdName, _requestId.Value);
                    }
                    return _requestId.Value;
                }
                catch
                {
                    if (_requestId != null)
                    {
                        return _requestId.Value;
                    }
                    else
                    {
                        return Guid.Empty;
                    }
                }
            }
        }

        internal static string FallbackEndpoints
        {
            get
            {
                object regValue = SettingsRegKey.GetValue(SettingsKey, _fallbackEndpointsName, null);
                return regValue as string;
            }
        }

        internal static string AppId
        {
            get
            {
                if (_appId != null)
                {
                    return _appId; // Cache.
                }

                try
                {
                    object regValue = SettingsRegKey.GetValue(SettingsKey, _appIdName, null);

                    var regString = regValue as string;
                    if (regString != null)
                    {
                        _appId = DecryptAppId(regString);
                        return _appId;
                    }
                    return null; // No Registry entry.
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                _appId = value;
                var protectedAppId = EncryptAppId(value);
                SettingsRegKey.SetValue(SettingsKey, _appIdName, protectedAppId);
            }
        }

        internal static string OldAppId
        {
            get
            {
                try
                {
                    var settings = new SettingsRegKey(_settingsHive, _oldSettingsSubkey);
                    object regValue = SettingsRegKey.GetValue(settings, _appIdName, null);

                    var regString = regValue as string;
                    if (regString != null)
                    {
                        return DecryptAppId(regString);
                    }
                    return null; // No Registry entry.
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Convert protected string to clear string. Null if it cannot decrypt successfully.
        /// </summary>
        public static string DecryptAppId(string protectedString)
        {
            try
            {
                var protectedBytes = Convert.FromBase64String(protectedString);
                var clearBytes = Unprotect(protectedBytes);
                if (clearBytes != null)
                {
                    var appId = Encoding.UTF8.GetString(clearBytes);
                    return appId;
                }
            }
            catch (FormatException)
            {
                return null; // Treat as if protected string is not usable.
            }
            return null;
        }

        public static string EncryptAppId(string appId)
        {
            var clearBytes = Encoding.UTF8.GetBytes(appId);
            var protectedBytes = Protect(clearBytes);
            if (protectedBytes != null)
            {
                var encryptedString = Convert.ToBase64String(protectedBytes);
                return encryptedString;
            }
            return null; // Should not happen.
        }

        internal static string Version
        {
            get
            {
                return SettingsRegKey.RetrieveStringValue(SettingsKey, _versionName, ref _version);
            }
            set
            {
                SettingsRegKey.UpdateStringValue(SettingsKey, _versionName, ref _version, value);
            }
        }

        internal static bool DisableFirstRun
        {
            get
            {
                return SettingsRegKey.RetrieveNullableBoolValue(SettingsKey, _disableFirstRunName, ref _disableFirstRun);
            }
            set
            {
                SettingsRegKey.UpdateNullableBoolValue(SettingsKey, _disableFirstRunName, ref _disableFirstRun, value);
            }
        }

        internal static bool DisableEditorWindowTour
        {
            get
            {
                return SettingsRegKey.RetrieveNullableBoolValue(SettingsKey, _disableEditorWindowTourName, ref _disableEditorWindowTour);
            }
            set
            {
                SettingsRegKey.UpdateNullableBoolValue(SettingsKey, _disableEditorWindowTourName, ref _disableEditorWindowTour, value);
            }
        }

        internal static string SetupDownloadPath
        {
            get
            {
                return SettingsRegKey.RetrieveStringValue(SettingsKey, _setupDownloadPathName, ref _setupDownloadPath);
            }
            set
            {
                SettingsRegKey.UpdateStringValue(SettingsKey, _setupDownloadPathName, ref _setupDownloadPath, value);
            }
        }

        internal static string AppPath
        {
            get
            {
                return SettingsRegKey.RetrieveStringValue(SettingsKey, _appPathName, ref _appPath);
            }
            set
            {
                SettingsRegKey.UpdateStringValue(SettingsKey, _appPathName, ref _appPath, value);
            }
        }

        internal static bool AllowProxyForAutoUpdate
        {
            get
            {
                return SettingsRegKey.RetrieveNullableBoolValue(SettingsKey, _allowProxyForAutoUpdateName, ref _allowProxyForAutoUpdate);
            }
        }

        internal static string MainWindowLocation
        {
            get
            {
                return SettingsRegKey.RetrieveStringValue(SettingsKey, _mainWindowLocationName, ref _mainWindowLocation);
            }
            set
            {
                SettingsRegKey.UpdateStringValue(SettingsKey, _mainWindowLocationName, ref _mainWindowLocation, value);
            }
        }

        internal static bool DisableRunWithWindows
        {
            get
            {
                return SettingsRegKey.RetrieveNullableBoolValue(SettingsKey, _disableRunWithWindowsName, ref _disableRunWithWindows);
            }
            set
            {
                SettingsRegKey.UpdateNullableBoolValue(SettingsKey, _disableRunWithWindowsName, ref _disableRunWithWindows, value);
            }
        }

        internal static bool DisableSysTrayBalloonAppStillRunning
        {
            get
            {
                return SettingsRegKey.RetrieveNullableBoolValue(SettingsKey, _disableSysTrayBalloonAppStillRunningName, ref _disableSysTrayBalloonAppStillRunning);
            }
            set
            {
                SettingsRegKey.UpdateNullableBoolValue(SettingsKey, _disableSysTrayBalloonAppStillRunningName, ref _disableSysTrayBalloonAppStillRunning, value);
            }
        }

        internal static bool DisableToolWindow
        {
            get
            {
                return SettingsRegKey.RetrieveNullableBoolValue(SettingsKey, _disableToolWindowName, ref _disableToolWindow);
            }
            set
            {
                SettingsRegKey.UpdateNullableBoolValue(SettingsKey, _disableToolWindowName, ref _disableToolWindow, value);
            }
        }

        internal static int ScreenCaptureDelay
        {
            get
            {
                return SettingsRegKey.RetrieveNullableIntValue(SettingsKey, _screenCaptureDelayName, ref _screenCaptureDelay);
            }
            set
            {
                SettingsRegKey.UpdateNullableIntValue(SettingsKey, _screenCaptureDelayName, ref _screenCaptureDelay, value);
            }
        }

        internal static bool AutoUpdate
        {
            get
            {
                return SettingsRegKey.RetrieveNullableBoolValue(SettingsKey, _autoUpdateName, ref _autoUpdate, true);
            }
        }

        internal static string AutoUpdateLogPath
        {
            get
            {
                return SettingsRegKey.RetrieveStringValue(SettingsKey, _autoUpdateLogPathName, ref _autoUpdateLogPath);
            }
            set
            {
                SettingsRegKey.UpdateStringValue(SettingsKey, _autoUpdateLogPathName, ref _autoUpdateLogPath, value);
            }
        }

        internal static bool IsNotificationToastEnabled
        {
            get
            {
                return SettingsRegKey.RetrieveNullableBoolValue(SettingsKey,
                    _isNotificationToastEnabledName, ref _isNotificationToastEnabled, true);
            }
            set
            {
                SettingsRegKey.UpdateNullableBoolValue(SettingsKey, _isNotificationToastEnabledName, ref _isNotificationToastEnabled, value);
            }
        }

        /// <summary>
        /// Custom directory for the auto-save functionnality
        /// </summary>
        internal static string CustomDirectory
        {
            get
            {
                return SettingsRegKey.RetrieveStringValue(SettingsKey, _customDirectoryName, ref _customDirectory);
            }
            set
            {
                SettingsRegKey.UpdateStringValue(SettingsKey, _customDirectoryName, ref _customDirectory, value);
            }
        }

        /// <summary>
        /// Whether to open the editor after a snip or not
        /// </summary>
        internal static bool IsOpenEditorPostSnip
        {
            get
            {
                return SettingsRegKey.RetrieveNullableBoolValue(SettingsKey,
                    _isOpenEditorPostSnipName, ref _isOpenEditorPostSnip, true);
            }
            set
            {
                SettingsRegKey.UpdateNullableBoolValue(SettingsKey, _isOpenEditorPostSnipName, ref _isOpenEditorPostSnip, value);
            }
        }

        /// <summary>
        /// Gets or sets the nullable bool value whether to copy to clipboad or not
        /// </summary>
        internal static bool CopyToClipboardAfterSnip
        {
            get
            {
                return SettingsRegKey.RetrieveNullableBoolValue(SettingsKey, _copyToClipboardAfterSnipName, ref _copyToClipboardAfterSnip);
            }
            set
            {
                SettingsRegKey.UpdateNullableBoolValue(SettingsKey, _copyToClipboardAfterSnipName, ref _copyToClipboardAfterSnip, value);
            }
        }

        /// <summary>
        /// Strength of moderation applied to prompt warning before sharing
        /// </summary>
        internal static int ContentModerationStrength
        {
            get
            {
                return SettingsRegKey.RetrieveNullableIntValue(SettingsKey, _contentModerationStrengthName, ref _contentModerationStrength);
            }
            set
            {
                SettingsRegKey.UpdateNullableIntValue(SettingsKey, _contentModerationStrengthName, ref _contentModerationStrength, value);
            }
        }

        /// <summary>
        /// Enable/disable intelligent naming and add meta data to file save.
        /// </summary>
        /// <returns>True if enabled, false if disabled</returns>
        internal static bool IsAutoTaggingEnabled
        {
            get
            {
                return SettingsRegKey.RetrieveNullableBoolValue(SettingsKey, _isAutoTaggingEnabledName, ref _isAutoTaggingEnabled);
            }
            set
            {
                SettingsRegKey.UpdateNullableBoolValue(SettingsKey, _isAutoTaggingEnabledName, ref _isAutoTaggingEnabled, value);
            }
        }

        /// <summary>
        /// Enable/disable all the features using AI services services.
        /// </summary>
        /// <returns>True if enabled, false if disabled</returns>
        internal static bool IsAIEnabled
        {
            get
            {
                return SettingsRegKey.RetrieveNullableBoolValue(SettingsKey, _isAIEnabledName, ref _isAIEnabled, false);
            }
            set
            {
                SettingsRegKey.UpdateNullableBoolValue(SettingsKey, _isAIEnabledName, ref _isAIEnabled, value);
            }
        }

        /// <summary>
        /// Fetch API keys/endpoints for cognitive services
        /// </summary>
        /// <param name="keyName">Name of the service, whose value is fetched</param>
        /// <param name="defaultValue">if the key does not exist, use this default</param>
        /// <returns></returns>
        internal static string GetKey(string keyName, string defaultValue, bool createIfNew = false)
        {
            var value = SettingsRegKey.RetrieveStringValue(SettingsKey, keyName, ref _key);
            if (string.IsNullOrWhiteSpace(value))
            {
                value = defaultValue;
                if (createIfNew)
                {
                    SetKey(keyName, value);
                }
            }

            return value;

        }

        /// <summary>
        /// Fetch API keys for cognitive services.
        /// </summary>
        /// <param name="keyName">Name of the service, whose key is fetched</param>
        /// <returns>Key</returns>
        internal static string GetKey(string keyName)
        {
            return SettingsRegKey.RetrieveStringValue(SettingsKey, keyName, ref _key);
        }

        /// <summary>
        /// Store the user entered API key.
        /// </summary>
        /// <param name="keyName">Name of the service, whose key is to be updated</param>
        /// <param name="keyValue">Updated key to be stored and used</param>
        internal static void SetKey(string keyName, string keyValue)
        {
            SettingsRegKey.UpdateStringValue(SettingsKey, keyName, ref _key, keyValue);
        }
        #endregion

        #region Shortcuts

        #region ShortcutsHandler
        public static KeyCombo ScreenCaptureShortcut
        {
            get
            {
                return RetrieveKeyComboValue(SettingsKey, _screenCaptureShortcutName, ref _screenCaptureShortcut);
            }
            set
            {
                UpdateKeyComboValue(SettingsKey, _screenCaptureShortcutName, ref _screenCaptureShortcut, value);
            }
        }

        /// <summary>
        /// Property for the quick capture shortcut
        /// </summary>
        public static KeyCombo QuickCaptureShortcut
        {
            get
            {
                return RetrieveKeyComboValue(SettingsKey, _quickCaptureShortcutName, ref _quickCaptureShortcut);
            }

            set
            {
                UpdateKeyComboValue(SettingsKey, _quickCaptureShortcutName, ref _quickCaptureShortcut, value);
            }
        }

        /// <summary>
        /// Property for accessing the library panel
        /// </summary>
        public static KeyCombo LibraryShortcut
        {
            get
            {
                return RetrieveKeyComboValue(SettingsKey, _libraryShortcutName, ref _libraryShortcut);
            }

            set
            {
                UpdateKeyComboValue(SettingsKey, _libraryShortcutName, ref _libraryShortcut, value);
            }
        }
        #endregion

        private static KeyCombo RetrieveKeyComboValue(SettingsRegKey key, string valueName, ref string member)
        {
            string text = SettingsRegKey.RetrieveStringValue(key, valueName, ref member);
            KeyCombo returnKey = null;

            // set default KeyCombo
            if (string.IsNullOrWhiteSpace(text))
            {
                if (valueName == _screenCaptureShortcutName)
                {
                    returnKey = KeyCombo.ParseOrDefault("Snapshot");
                    ScreenCaptureShortcut = returnKey;
                }
                else if (valueName == _quickCaptureShortcutName)
                {
                    returnKey = KeyCombo.ParseOrDefault("None");
                    QuickCaptureShortcut = returnKey;
                }
                else if (valueName == _libraryShortcutName)
                {
                    returnKey = KeyCombo.ParseOrDefault("None");
                    LibraryShortcut = returnKey;
                }
            }
            else
            {
                returnKey = KeyCombo.ParseOrDefault(text);
            }

            return returnKey;
        }

        private static void UpdateKeyComboValue(SettingsRegKey key, string valueName, ref string member, KeyCombo value)
        {
            string text = _disableKeyCombo;

            if (value != null && value.IsValid)
            {
                text = value.ToString();
            }

            SettingsRegKey.UpdateStringValue(key, valueName, ref member, text);
        }

        #endregion

        /// <summary>
        /// Protect data for user.
        /// </summary>
        /// <see cref="https://msdn.microsoft.com/en-us/library/ms995355.aspx"/>
        public static byte[] Protect(byte[] data)
        {
            try
            {
                // Encrypt the data using DataProtectionScope.CurrentUser. The result can be decrypted
                //  only by the same current user.
                return ProtectedData.Protect(data, SAditionalEntropy, DataProtectionScope.CurrentUser);
            }
            catch (CryptographicException e)
            {
                Diagnostics.LogTrace("Unable to protect appid data");
                Diagnostics.LogLowPriException(e);
                return null;
            }
        }

        /// <summary>
        /// UnProtect data for user.
        /// </summary>
        /// <see cref="https://msdn.microsoft.com/en-us/library/ms995355.aspx"/>
        public static byte[] Unprotect(byte[] data)
        {
            try
            {
                //Decrypt the data using DataProtectionScope.CurrentUser.
                return ProtectedData.Unprotect(data, SAditionalEntropy, DataProtectionScope.CurrentUser);
            }
            catch (CryptographicException e)
            {
                Diagnostics.LogTrace("Unable to unprotect appid data");
                Diagnostics.LogLowPriException(e);
                return null;
            }
        }
    }

    #region SettingReg
    internal class SettingsRegKey
    {
        readonly RegistryHive _settingsHive = RegistryHive.CurrentUser;
        readonly string _settingsSubkey = null;

        internal SettingsRegKey(RegistryHive settingsHive, string settingsSubkey)
        {
            _settingsHive = settingsHive;
            _settingsSubkey = settingsSubkey;
        }

        RegistryHive SettingsHive
        {
            get
            {
                return _settingsHive;
            }
        }

        RegistryKey SettingsKey
        {
            get
            {
                RegistryKey settingsKey = null;

                try
                {
                    // Use the 64-bit view of the registry. This will open the 32-bit view on 32-bit machines
                    using (RegistryKey regRoot = RegistryKey.OpenBaseKey(_settingsHive, RegistryView.Registry64))
                    {
                        if (_settingsHive == RegistryHive.LocalMachine)
                        {
                            settingsKey = regRoot.OpenSubKey(_settingsSubkey);
                        }
                        else // CurrentUser
                        {
                            settingsKey = regRoot.CreateSubKey(_settingsSubkey); // create key or open existing for write
                        }
                    }
                }
                catch (Exception e)
                {
                    Diagnostics.LogTrace(string.Format("Unable to open {0} settings regkey", _settingsHive));
                    Diagnostics.LogLowPriException(e);
                }

                return settingsKey;
            }
        }

        internal static object GetValue(SettingsRegKey key, string valueName, object defaultValue)
        {
            if (key == null)
            {
                return defaultValue;
            }

            using (RegistryKey regKey = key.SettingsKey)
            {
                return regKey == null ? defaultValue : regKey.GetValue(valueName, defaultValue); // HKLM key may not exist
            }
        }

        internal static void SetValue(SettingsRegKey key, string valueName, object value)
        {
            if (key == null)
            {
                return;
            }

            if (key.SettingsHive == RegistryHive.LocalMachine) // prevent writes to HKLM registry
            {
                return;
            }

            using (RegistryKey regKey = key.SettingsKey)
            {
                regKey.SetValue(valueName, value);
            }
        }

        internal static string RetrieveStringValue(SettingsRegKey key, string valueName, ref string member)
        {
            try
            {
                member = GetValue(key, valueName, null) as string;
                if (member == null)
                {
                    member = string.Empty;
                    SetValue(key, valueName, member);
                }
            }
            catch
            {
            }

            if (member != null)
            {
                return member;
            }
            else
            {
                return string.Empty;
            }
        }

        internal static void UpdateStringValue(SettingsRegKey key, string valueName, ref string member, string value)
        {
            try
            {
                if (!string.Equals(member, value))
                {
                    member = value;
                    SetValue(key, valueName, value);
                }
            }
            catch
            {
            }
        }

        internal static bool RetrieveNullableBoolValue(SettingsRegKey key, string valueName, ref bool? member, bool defaultValue = false)
        {
            try
            {
                int? value = GetValue(key, valueName, null) as int?;
                if (value == null)
                {
                    member = defaultValue;
                    SetValue(key, valueName, Convert.ToUInt32(member));
                }
                else
                {
                    member = value.Value != 0;
                }
            }
            catch
            {
            }

            if (member != null)
            {
                return member.Value;
            }
            else
            {
                return false;
            }
        }

        internal static void UpdateNullableBoolValue(SettingsRegKey key, string valueName, ref bool? member, bool value)
        {
            try
            {
                if (member == null || member.Value != value)
                {
                    member = value;
                    SetValue(key, valueName, value ? 1 : 0);
                }
            }
            catch
            {
            }
        }

        internal static int RetrieveNullableIntValue(SettingsRegKey key, string valueName, ref int? member)
        {
            try
            {
                int? value = GetValue(key, valueName, null) as int?;
                if (value == null)
                {
                    member = 0;
                    SetValue(key, valueName, 0);
                }
                else
                {
                    member = value.Value;
                }
            }
            catch
            {
            }

            if (member != null)
            {
                return member.Value;
            }
            else
            {
                return 0;
            }
        }

        internal static void UpdateNullableIntValue(SettingsRegKey key, string valueName, ref int? member, int value)
        {
            try
            {
                if (member == null || member.Value != value)
                {
                    member = value;
                    SetValue(key, valueName, value);
                }
            }
            catch
            {
            }
        }
    }
    #endregion
}
