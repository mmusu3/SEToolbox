namespace SEToolboxUpdate
{
    using SEToolbox.Support;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Windows;
    using Res = SEToolboxUpdate.Properties.Resources;

    class Program
    {
        private const int NoError = 0;
        private const int UpdateBinariesFailed = 1;
        private const int UacDenied = 2;

        private const string logFilePath = "updater-log.txt";

        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfoByIetfLanguageTag(GlobalSettings.Default.LanguageCode);

            if (File.Exists(logFilePath))
                File.Delete(logFilePath);

            // Install.
            if (args.Any(a => a.Equals("/I", StringComparison.OrdinalIgnoreCase) || a.Equals("-I", StringComparison.OrdinalIgnoreCase)))
            {
                InstallConfigurationSettings();
                return;
            }

            // Uninstall.
            if (args.Any(a => a.Equals("/U", StringComparison.OrdinalIgnoreCase) || a.Equals("-U", StringComparison.OrdinalIgnoreCase)))
            {
                UninstallConfigurationSettings();
                return;
            }

            // Binaries.
            if (args.Any(a => a.Equals("/B", StringComparison.OrdinalIgnoreCase) || a.Equals("-B", StringComparison.OrdinalIgnoreCase)))
            {
                UpdateBaseLibrariesFromSpaceEngineers(args);
                return;
            }

            var appFile = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
            MessageBox.Show(string.Format(Res.AppParameterHelpMessage, appFile), Res.AppParameterHelpTitle, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK);
        }

        private static void InstallConfigurationSettings()
        {
            DiagnosticsLogging.CreateLog();
            CleanBinCache();
        }

        private static void UninstallConfigurationSettings()
        {
            DiagnosticsLogging.RemoveLog();
            CleanBinCache();
        }

        private static void UpdateBaseLibrariesFromSpaceEngineers(string[] args)
        {
            bool attemptedAlready = args.Any(a => a.Equals("/A", StringComparison.OrdinalIgnoreCase));

            var updaterExePath = Assembly.GetExecutingAssembly().Location;
            var appDirectory = Path.GetDirectoryName(updaterExePath);
            var toolboxExePath = Path.Combine(appDirectory, "SEToolbox.exe");

            if (!ToolboxUpdater.IsRuningElevated())
            {
                // Does not have elevated permission to run.
                if (!attemptedAlready)
                {
                    MessageBox.Show(Res.UpdateRequiredUACMessage, Res.UpdateRequiredTitle, MessageBoxButton.OK, MessageBoxImage.Information);

                    var ret = ToolboxUpdater.RunElevated(updaterExePath, string.Join(" ", args) + " /A", elevate: true, waitForExit: true);

                    // Don't run toolbox from the elevated process, do it here.
                    if (ret.HasValue)
                        LaunchToolbox(ret.Value);
                    else
                        LaunchToolbox(UacDenied);
                }
            }
            else
            {
                if (!attemptedAlready)
                    MessageBox.Show(Res.UpdateRequiredMessage, Res.UpdateRequiredTitle, MessageBoxButton.OK, MessageBoxImage.Information);

                // Is running elevated permission, update the files.
                bool wasUpdated = UpdateBaseFiles(appDirectory, out var ex);

                if (!wasUpdated && ex != null)
                {
                    string errorMsg;

                    if (ex is IOException ioEx)
                        errorMsg = $"Failed to copy one or more game files. Error:\n{ioEx.Message}";
                    else
                        errorMsg = $"Failed to copy one or more game files. Error:\n{ex}";

                    File.WriteAllText(logFilePath, errorMsg);
                }

                int errorCode = wasUpdated ? NoError : UpdateBinariesFailed;

                if (!attemptedAlready)
                    LaunchToolbox(errorCode);
                else // Don't run toolbox from the elevated process, return to the original updater process.
                    Environment.Exit(errorCode);
            }

            void LaunchToolbox(int errorCode)
            {
                if (errorCode == 0)
                {
                    // B = Binaries were updated.
                    ToolboxUpdater.RunElevated(toolboxExePath, "/B " + string.Join(" ", args), elevate: false, waitForExit: false);
                }
                else
                {
                    string message, caption;

                    if (errorCode == UacDenied)
                    {
                        message = Res.CancelUACMessage;
                        caption = Res.CancelUACTitle;
                    }
                    else
                    {
                        // Update failed? Files are readonly. Files are locked. Source Files missing (or renamed).
                        message = Res.UpdateErrorMessage;
                        caption = Res.UpdateErrorTitle;
                    }

                    var dialogResult = MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (dialogResult == MessageBoxResult.Yes)
                    {
                        // X = Ignore updates.
                        ToolboxUpdater.RunElevated(toolboxExePath, "/X " + string.Join(" ", args), elevate: false, waitForExit: false);
                    }
                }

                Environment.Exit(errorCode);
            }
        }

        /// <summary>
        /// Updates the base library files from the Space Engineers application path.
        /// </summary>
        /// <param name="appFilePath"></param>
        /// <returns>True if it succeeded, False if there was an issue that blocked it.</returns>
        private static bool UpdateBaseFiles(string appFilePath, out Exception exception)
        {
            exception = null;

            var liveProcesses = Process.GetProcessesByName("SEToolbox");

            // Wait until SEToolbox is shut down.
            foreach (var item in liveProcesses)
            {
                if (!item.WaitForExit(10_000))
                {
                    exception = new Exception("Timed out waiting for SEToolbox to close.");
                    return false; // 10 seconds is too long. Abort.
                }
            }

            var baseFilePath = ToolboxUpdater.GetApplicationFilePath();

            foreach (var fileName in ToolboxUpdater.CoreSpaceEngineersFiles)
            {
                var sourceFile = Path.Combine(baseFilePath, fileName);

                try
                {
                    File.Copy(sourceFile, Path.Combine(appFilePath, fileName), overwrite: true);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    return false;
                }
            }

            foreach (var fileName in ToolboxUpdater.OptionalSpaceEngineersFiles)
            {
                var sourceFile = Path.Combine(baseFilePath, fileName);

                try
                {
                    File.Copy(sourceFile, Path.Combine(appFilePath, fileName), overwrite: true);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Clear app bin cache.
        /// </summary>
        private static void CleanBinCache()
        {
            var binCache = ToolboxUpdater.GetBinCachePath();

            if (Directory.Exists(binCache))
            {
                try
                {
                    Directory.Delete(binCache, true);
                }
                catch { }
            }
        }
    }
}
