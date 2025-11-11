namespace SEToolbox.Support
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Windows;

    using Microsoft.Win32;

    public class GlobalSettings
    {
        public static GlobalSettings Default = new GlobalSettings();

        /// <summary>
        /// Temporary property to reprompt user to game installation path.
        /// </summary>
        public bool PromptUser;

        private bool _isLoaded;

        private const string BaseKey = @"SOFTWARE\MidSpace\SEToolbox";

        /// <summary>
        /// Temporary store for Game Version.
        /// </summary>
        public Version SEVersion { get; set; }

        /// <summary>
        /// Application binary path.
        /// </summary>
        public string SEBinPath { get; set; }

        /// <summary>
        /// Display language for localized text.
        /// <remarks>This is not for number or date formats, as this is taken directly from the User profile via CurrentCulture.</remarks>
        /// </summary>
        public string LanguageCode { get; set; }

        /// <summary>
        /// Indicates that a SETooolbox resource is to be used first when trying to load localized resources from the game.
        /// </summary>
        public bool? UseCustomResource { get; set; }

        /// <summary>
        /// Delimited ';' list of UNC paths to search for Save World data, with the 'LastLoaded.sbl' at its root.
        /// </summary>
        public string CustomUserSavePaths { get; set; }

        public WindowState? WindowState { get; set; }
        public double? WindowTop { get; set; }
        public double? WindowLeft { get; set; }
        public double? WindowWidth { get; set; }
        public double? WindowHeight { get; set; }

        /// <summary>
        /// Indicates if Toolbox Version check should be ignored.
        /// </summary>
        public bool? AlwaysCheckForUpdates { get; set; }

        /// <summary>
        /// Ignore this specific version during Toolbox version check.
        /// </summary>
        public string IgnoreUpdateVersion { get; set; }

        /// <summary>
        /// Custom user specified path for Asteroids.
        /// </summary>
        public string CustomVoxelPath { get; set; }

        /// <summary>
        /// Counter for the number times successfully started up SEToolbox, total.
        /// </summary>
        public int? TimesStartedTotal { get; set; }

        /// <summary>
        /// Counter for the number times successfully started up SEToolbox, since the last reset.
        /// </summary>
        public int? TimesStartedLastReset { get; set; }

        /// <summary>
        /// Counter for the number times successfully started up SEToolbox, since the last game update.
        /// </summary>
        public int? TimesStartedLastGameUpdate { get; set; }

        public void Save()
        {
            var key = Registry.CurrentUser.OpenSubKey(BaseKey, true) ?? Registry.CurrentUser.CreateSubKey(BaseKey);

            UpdateValue(key, "SEVersion", SEVersion);
            UpdateValue(key, "SEBinPath", SEBinPath);
            UpdateValue(key, "CustomUserSavePaths", CustomUserSavePaths);
            UpdateValue(key, "LanguageCode", LanguageCode);
            UpdateValue(key, "WindowState", WindowState);
            UpdateValue(key, "WindowTop", WindowTop);
            UpdateValue(key, "WindowLeft", WindowLeft);
            UpdateValue(key, "WindowWidth", WindowWidth);
            UpdateValue(key, "WindowHeight", WindowHeight);
            UpdateValue(key, "AlwaysCheckForUpdates", AlwaysCheckForUpdates);
            UpdateValue(key, "UseCustomResource", UseCustomResource);
            UpdateValue(key, "IgnoreUpdateVersion", IgnoreUpdateVersion);
            UpdateValue(key, "CustomVoxelPath", CustomVoxelPath);
            UpdateValue(key, "TimesStartedTotal", TimesStartedTotal);
            UpdateValue(key, "TimesStartedLastReset", TimesStartedLastReset);
            UpdateValue(key, "TimesStartedLastGameUpdate", TimesStartedLastGameUpdate);
        }

        public void Load()
        {
            _isLoaded = true;

            var key = Registry.CurrentUser.OpenSubKey(BaseKey, false);

            if (key == null)
            {
                Reset();
                return;
            }

            SEVersion = ReadValue<Version>(key, "SEVersion", null);
            SEBinPath = ReadValue<string>(key, "SEBinPath", null);
            LanguageCode = ReadValue<string>(key, "LanguageCode", CultureInfo.CurrentUICulture.IetfLanguageTag);
            CustomUserSavePaths = ReadValue<string>(key, "CustomUserSavePaths", null);
            WindowState = ReadValue<WindowState?>(key, "WindowState", null);
            WindowTop = ReadValue<double?>(key, "WindowTop", null);
            WindowLeft = ReadValue<double?>(key, "WindowLeft", null);
            WindowWidth = ReadValue<double?>(key, "WindowWidth", null);
            WindowHeight = ReadValue<double?>(key, "WindowHeight", null);
            AlwaysCheckForUpdates = ReadValue<bool?>(key, "AlwaysCheckForUpdates", true);
            UseCustomResource = ReadValue<bool?>(key, "UseCustomResource", true);
            IgnoreUpdateVersion = ReadValue<string>(key, "IgnoreUpdateVersion", null);
            CustomVoxelPath = ReadValue<string>(key, "CustomVoxelPath", null);
            TimesStartedTotal = ReadValue<int?>(key, "TimesStartedTotal", null);
            TimesStartedLastReset = ReadValue<int?>(key, "TimesStartedLastReset", null);
            TimesStartedLastGameUpdate = ReadValue<int?>(key, "TimesStartedLastGameUpdate", null);

            if (WindowTop.HasValue && (int.MinValue > WindowTop || WindowTop > int.MaxValue))
                WindowTop = null;

            if (WindowLeft.HasValue && (int.MinValue > WindowLeft || WindowLeft > int.MaxValue))
                WindowLeft = null;

            if (WindowWidth.HasValue && (0 > WindowWidth || WindowWidth > int.MaxValue))
                WindowWidth = null;

            if (WindowHeight.HasValue && (0 > WindowHeight || WindowHeight > int.MaxValue))
                WindowHeight = null;
        }

        /// <summary>
        /// set all properties to their default value. Used for new application installs.
        /// </summary>
        public void Reset()
        {
            SEVersion = null;
            SEBinPath = null;
            LanguageCode = CultureInfo.CurrentUICulture.IetfLanguageTag;  // Display language (only applied on multi lingual deployment of Windows OS).
            CustomUserSavePaths = null;
            WindowState = null;
            WindowTop = null;
            WindowLeft = null;
            WindowHeight = null;
            WindowWidth = null;
            AlwaysCheckForUpdates = true;
            IgnoreUpdateVersion = null;
            CustomVoxelPath = null;
            // don't reset TimesStartedTotal.
            TimesStartedLastReset = null;
            // don't reset TimesStartedLastGameUpdate.
        }

        public static Version GetAppVersion(bool ignoreBuildRevision = false)
        {
            var assemblyVersion = Assembly.GetExecutingAssembly()
              .GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false)
              .OfType<AssemblyFileVersionAttribute>()
              .FirstOrDefault();

            var version = assemblyVersion == null ? new Version() : new Version(assemblyVersion.Version);

            if (ignoreBuildRevision)
                return new Version(version.Major, version.Minor, 0, 0);

            return version;
        }

        private static void UpdateValue(RegistryKey key, string subkey, object value)
        {
            if (value == null)
            {
                key.DeleteValue(subkey, false);
            }
            else
            {
                // Registry values need to be non-culture specific when written.
                if (value is bool || value is int)
                {
                    key.SetValue(subkey, value, RegistryValueKind.DWord);
                }
                else if (value is double d)
                {
                    value = d.ToString(CultureInfo.InvariantCulture);
                    key.SetValue(subkey, value);
                }
                else
                {
                    key.SetValue(subkey, value);
                }
            }
        }

        private static T ReadValue<T>(RegistryKey key, string subkey, T defaultValue)
        {
            var item = key.GetValue(subkey, defaultValue);

            try
            {
                if (item == null)
                    return default;

                if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var baseType = typeof(T).GetGenericArguments()[0];

                    if (baseType.BaseType == typeof(Enum))
                        return (T)Enum.Parse(baseType, (string)item);

                    // Registry values need to be non-culture specific when read.
                    return (T)Convert.ChangeType(item, baseType, CultureInfo.InvariantCulture);
                }
                else if (typeof(T) == typeof(Version))
                {
                    item = new Version((string)item);
                }
            }
            catch
            {
                item = defaultValue;
            }

            return (T)item;
        }
    }
}
