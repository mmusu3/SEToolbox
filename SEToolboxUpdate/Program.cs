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

        static void Main(string[] args)
        {
            var logFileName = ToolboxUpdater.IsRuningElevated()
                ? "./updater-elevated-log.txt"
                : "./updater-log.txt";

            Log.Init(logFileName, appendFile: false);
            Log.Info("Updater started.");

            Log.Debug("Loading settings");
            GlobalSettings.Default.Load();

            Log.Info($"Current language code is: {GlobalSettings.Default.LanguageCode}");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfoByIetfLanguageTag(GlobalSettings.Default.LanguageCode);

            // Binaries.
            if (args.Any(a => a.Equals("/B", StringComparison.OrdinalIgnoreCase) || a.Equals("-B", StringComparison.OrdinalIgnoreCase)))
            {
                UpdateBaseLibrariesFromSpaceEngineers(args);
                return;
            }

            Log.Info("Process was started by user, closing.");

            var appFile = Path.GetFileName(Assembly.GetExecutingAssembly().Location);

            MessageBox.Show(string.Format(Res.AppParameterHelpMessage, appFile), Res.AppParameterHelpTitle, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK);
        }

        private static void UpdateBaseLibrariesFromSpaceEngineers(string[] args)
        {
            Log.Info("Updater task is update game files.");

            bool attemptedAlready = args.Any(a => a.Equals("/A", StringComparison.OrdinalIgnoreCase));

            var updaterExePath = Assembly.GetExecutingAssembly().Location;
            var appDirectory = Path.GetDirectoryName(updaterExePath);
            var toolboxExePath = Path.Combine(appDirectory, "SEToolbox.exe");

            if (!ToolboxUpdater.IsRuningElevated())
            {
                Log.Debug("Not running elevated.");

                // Does not have elevated permission to run.
                if (!attemptedAlready)
                {
                    Log.Debug("Is first updater process.");

                    MessageBox.Show(Res.UpdateRequiredUACMessage, Res.UpdateRequiredTitle, MessageBoxButton.OK, MessageBoxImage.Information);

                    Log.Info("Starting elevated updater process, waiting for exit.");

                    var ret = ToolboxUpdater.RunElevated(updaterExePath, string.Join(" ", args) + " /A", elevate: true, waitForExit: true);

                    if (ret is { } r)
                        Log.Info($"Elevated updater process closed with exit code {r}.");
                    else
                        Log.Info("Failed to start updater process.");

                    // Don't run toolbox from the elevated process, do it here.
                    if (ret.HasValue)
                        LaunchToolbox(ret.Value);
                    else
                        LaunchToolbox(UacDenied);
                }
            }
            else
            {
                Log.Debug("Running elevated.");

                if (!attemptedAlready)
                {
                    Log.Debug("Is first updater process.");

                    MessageBox.Show(Res.UpdateRequiredMessage, Res.UpdateRequiredTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Is running elevated permission, update the files.
                bool wasUpdated = UpdateBaseFiles(appDirectory, out var ex);

                if (!wasUpdated && ex != null)
                {
                    Log.Info("Game file updating failed.");

                    string errorMsg;

                    if (ex is IOException ioEx)
                        errorMsg = $"Failed to copy one or more game files. Error:\n{ioEx.Message}";
                    else
                        errorMsg = $"Failed to copy one or more game files. Error:\n{ex}";

                    Log.Fatal(errorMsg, ex);
                }

                int errorCode = wasUpdated ? NoError : UpdateBinariesFailed;

                if (!attemptedAlready)
                {
                    LaunchToolbox(errorCode);
                }
                else // Don't run toolbox from the elevated process, return to the original updater process.
                {
                    Log.Info($"Closing with exit code {errorCode}.");

                    Environment.Exit(errorCode);
                }
            }

            void LaunchToolbox(int errorCode)
            {
                if (errorCode == 0)
                {
                    Log.Info("Starting toolbox process.");

                    // B = Binaries were updated.
                    ToolboxUpdater.RunElevated(toolboxExePath, "/appendlog /B " + string.Join(" ", args), elevate: false, waitForExit: false);
                }
                else
                {
                    Log.Info($"Got error code {errorCode}");

                    string message, caption;

                    if (errorCode == UacDenied)
                    {
                        Log.Info("UAC denied.");

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
                        Log.Info("Starting toolbox process with ignore updates.");

                        // X = Ignore updates.
                        ToolboxUpdater.RunElevated(toolboxExePath, "/appendlog /X " + string.Join(" ", args), elevate: false, waitForExit: false);
                    }
                }

                Log.Info($"Closing with exit code {errorCode}.");

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
            Log.Info("Updating game files.");

            exception = null;

            var liveProcesses = Process.GetProcessesByName("SEToolbox");

            if (liveProcesses.Length > 0)
                Log.Info("Waiting for one or more Toolbox processes to exit.");

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
    }
}
