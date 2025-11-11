namespace SEToolbox
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows;

    using SEToolbox.Interop;
    using SEToolbox.Models;
    using SEToolbox.Support;
    using SEToolbox.ViewModels;
    using SEToolbox.Views;
    using Res = SEToolbox.Properties.Resources;

    public class CoreToolbox
    {
        public bool Init(string[] args)
        {
            // Detection and correction of local settings of SE install location.
            var gameBinDir = ToolboxUpdater.GetApplicationFilePath();

            var validApps = new string[] {
                "SpaceEngineers.exe",
                "SpaceEngineersDedicated.exe"
            };

            if (GlobalSettings.Default.PromptUser || !ToolboxUpdater.ValidateSpaceEngineersInstall(gameBinDir))
            {
                if (GlobalSettings.Default.PromptUser)
                    Log.Info("Prompting user for SE bin path.");
                else
                    Log.Info("SE bin path is invalid, prompting user.");

                if (Directory.Exists(gameBinDir))
                {
                    foreach (var validApp in validApps)
                    {
                        var testPath = Path.Combine(gameBinDir, validApp);

                        if (File.Exists(testPath))
                        {
                            gameBinDir = testPath;
                            break;
                        }
                    }
                }

                var faModel = new FindApplicationModel { GameApplicationPath = gameBinDir };
                var faViewModel = new FindApplicationViewModel(faModel);
                var faWindow = new WindowFindApplication(faViewModel);

                if (faWindow.ShowDialog() != true)
                    return false;

                Log.Info("Got new path.");

                gameBinDir = faModel.GameBinPath;
            }

            // Update and save user path.
            GlobalSettings.Default.SEBinPath = gameBinDir;
            GlobalSettings.Default.Save();

            bool ignoreUpdates = args.Any(a => a.ToUpper() == "/X" || a.ToUpper() == "-X");

            // Go looking for any changes in the Dependant Space Engineers assemblies and immediately attempt to update.
            if (!ignoreUpdates && ToolboxUpdater.IsBaseAssembliesChanged() && !Debugger.IsAttached)
            {
                Log.Info("Starting non-elevated updater process.");

                ToolboxUpdater.RunElevated(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SEToolboxUpdate"),
                    "/B " + string.Join(" ", args), elevate: false, waitForExit: false);

                return false;
            }

            var proc = Process.GetCurrentProcess();

            if (Process.GetProcessesByName(proc.ProcessName).Length == 1)
            {
                // Clean up Temp files if this is the only instance running.
                TempfileUtil.DestroyTempFiles();
            }

            // Dot not load any of the Space Engineers assemblies or dependant classes before this point.
            // ============================================

            return true;
        }

        public bool Load(string[] args)
        {
            var settings = GlobalSettings.Default;

            // Fetch the game version and store, so it can be retrieved during crash if the toolbox makes it this far.
            Version gameVersion = SpaceEngineersConsts.GetSEVersion();
            bool newVersion = settings.SEVersion != gameVersion;

            settings.SEVersion = gameVersion;

            // Test the Space Engineers version to make sure users are using an version that is new enough for SEToolbox to run with!
            // This is usually because a user has not updated a manual install of a Dedicated Server, or their Steam did not update properly.
            if (settings.SEVersion < GlobalSettings.GetAppVersion(true))
            {
                MessageBox.Show(
                    string.Format(Res.DialogOldSEVersionMessage, SpaceEngineersConsts.GetSEVersion(), settings.SEBinPath, GlobalSettings.GetAppVersion()),
                    Res.DialogOldSEVersionTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);

                Application.Current.Shutdown();
                return false;
            }

            // the /B argument indicates the SEToolboxUpdate had started SEToolbox after fetching updated game binaries.
            if (newVersion && args.Any(a => a.Equals("/B", StringComparison.OrdinalIgnoreCase) || a.Equals("-B", StringComparison.OrdinalIgnoreCase)))
            {
                // Reset the counter used to indicate if the game binaries have updated.
                settings.TimesStartedLastGameUpdate = null;
            }

            //string loadWorld = null;

            //foreach (var arg in args)
            //{
            //    if (arg.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 && !File.Exists(arg))
            //        continue;

            //    string file = Path.GetFileName(arg);

            //    if (file.Equals("Sandbox.sbc", StringComparison.InvariantCultureIgnoreCase)
            //        || file.Equals("SANDBOX_0_0_0_.sbs", StringComparison.InvariantCultureIgnoreCase))
            //        loadWorld = Path.GetDirectoryName(arg);
            //}

            // Force pre-loading of any Space Engineers resources.
            SpaceEngineersCore.LoadDefinitions();

            // Load the Space Engineers assemblies, or dependant classes after this point.
            var explorerModel = new ExplorerModel();

            if (args.Any(a => a.ToUpper() == "/WR" || a.ToUpper() == "-WR"))
            {
                ResourceReportModel.GenerateOfflineReport(explorerModel, args);
                Application.Current.Shutdown();
                return false;
            }

            var eViewModel = new ExplorerViewModel(explorerModel);
            var eWindow = new WindowExplorer(eViewModel);
            //if (allowClose)
            //{
            eViewModel.CloseRequested += (sender, e) =>
            {
                SaveSettings(eWindow);
                Application.Current.Shutdown();
            };
            //}
            eWindow.Loaded += (sender, e) =>
            {
                Log.Debug("Main window loaded.");

                Splasher.CloseSplash();

                var settings = GlobalSettings.Default;

                double left = settings.WindowLeft ?? eWindow.Left;
                double top = settings.WindowTop ?? eWindow.Top;
                double width = settings.WindowWidth ?? eWindow.Width;
                double height = settings.WindowHeight ?? eWindow.Height;

                var windowRect = new System.Drawing.Rectangle((int)left, (int)top, (int)width, (int)height);
                bool isInsideDesktop = false;

                foreach (var screen in System.Windows.Forms.Screen.AllScreens)
                {
                    try
                    {
                        if (screen.Bounds.IntersectsWith(windowRect))
                        {
                            isInsideDesktop = true;
                            break;
                        }
                    }
                    catch
                    {
                        // some virtual screens have been know to cause issues.
                    }
                }

                if (isInsideDesktop)
                {
                    eWindow.Left = left;
                    eWindow.Top = top;
                    eWindow.Width = width;
                    eWindow.Height = height;

                    if (settings.WindowState.HasValue)
                        eWindow.WindowState = settings.WindowState.Value;
                }
            };

            settings.TimesStartedTotal = (settings.TimesStartedTotal ?? 0) + 1;
            settings.TimesStartedLastReset = (settings.TimesStartedLastReset ?? 0) + 1;
            settings.TimesStartedLastGameUpdate = (settings.TimesStartedLastGameUpdate ?? 0) + 1;
            settings.Save();

            Log.Debug("Showing main window.");

            eWindow.ShowDialog();

            return true;
        }

        public void Exit()
        {
            //if (VRage.Plugins.MyPlugins.Loaded)
            //    VRage.Plugins.MyPlugins.Unload();

            TempfileUtil.Dispose();
        }

        private static void SaveSettings(WindowExplorer eWindow)
        {
            var settings = GlobalSettings.Default;
            settings.WindowState = eWindow.WindowState;
            eWindow.WindowState = WindowState.Normal; // Reset the State before getting the window size.
            settings.WindowHeight = eWindow.Height;
            settings.WindowWidth = eWindow.Width;
            settings.WindowTop = eWindow.Top;
            settings.WindowLeft = eWindow.Left;
            settings.Save();
        }
    }
}
