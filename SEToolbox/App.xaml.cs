namespace SEToolbox
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;

    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using SEToolbox.Views;
    using WPFLocalizeExtension.Engine;
    using Res = SEToolbox.Properties.Resources;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Fields

        private CoreToolbox _toolboxApplication;

        #endregion

        #region events

        private void OnStartup(Object sender, StartupEventArgs e)
        {
            if ((NativeMethods.GetKeyState(System.Windows.Forms.Keys.ShiftKey) & KeyStates.Down) == KeyStates.Down)
            {
                // Reset User Settings when Shift is held down during start up.
                GlobalSettings.Default.Reset();
                GlobalSettings.Default.PromptUser = true;

                // Clear app bin cache.
                var binCache = ToolboxUpdater.GetBinCachePath();

                if (Directory.Exists(binCache))
                {
                    try
                    {
                        Directory.Delete(binCache, true);
                    }
                    catch
                    {
                        // File is locked and cannot be deleted at this time.
                    }
                }
            }

            LocalizeDictionary.Instance.SetCurrentThreadCulture = false;
            LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfoByIetfLanguageTag(GlobalSettings.Default.LanguageCode);
            Thread.CurrentThread.CurrentUICulture = LocalizeDictionary.Instance.Culture;

            Splasher.Splash = new WindowSplashScreen();
            Splasher.ShowSplash();

            log4net.Config.XmlConfigurator.Configure();

            var update = CodeRepositoryReleases.CheckForUpdates(GlobalSettings.GetAppVersion());

            if (update != null)
            {
                var dialogResult = MessageBox.Show(string.Format(Res.DialogNewVersionMessage, update.Version), Res.DialogNewVersionTitle, MessageBoxButton.YesNo, MessageBoxImage.Information);

                if (dialogResult == MessageBoxResult.Yes)
                {
                    Process.Start(update.Link); // Opens release URL in browser
                    GlobalSettings.Default.Save();
                    Application.Current.Shutdown();
                    return;
                }

                if (dialogResult == MessageBoxResult.No)
                {
                    GlobalSettings.Default.IgnoreUpdateVersion = update.Version.ToString();
                }
            }

            // Configure service locator.
            ServiceLocator.RegisterSingleton<IDialogService, DialogService>();
            ServiceLocator.Register<IOpenFileDialog, OpenFileDialogViewModel>();
            ServiceLocator.Register<ISaveFileDialog, SaveFileDialogViewModel>();
            ServiceLocator.Register<IColorDialog, ColorDialogViewModel>();
            ServiceLocator.Register<IFolderBrowserDialog, FolderBrowserDialogViewModel>();

            System.Windows.FrameworkCompatibilityPreferences.KeepTextBoxDisplaySynchronizedWithTextProperty = false;

            _toolboxApplication = new CoreToolbox();
            if (_toolboxApplication.Init(e.Args))
                _toolboxApplication.Load(e.Args);
            else
                Application.Current.Shutdown();
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            if (_toolboxApplication != null)
                _toolboxApplication.Exit();
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var comException = e.Exception as System.Runtime.InteropServices.COMException;

            if (comException != null && comException.ErrorCode == -2147221040)
            {
                // To fix 'OpenClipboard Failed (Exception from HRESULT: 0x800401D0 (CLIPBRD_E_CANT_OPEN)'
                // http://stackoverflow.com/questions/12769264/openclipboard-failed-when-copy-pasting-data-from-wpf-datagrid

                e.Handled = true;
                return;
            }

            DiagnosticsLogging.LogException(e.Exception);

            string message;

            if (e.Exception is ToolboxException)
            {
                message = e.Exception.Message;
            }
            else
            {
                // Unhandled Exception.
                //if (DiagnosticsLogging.LoggingSourceExists())
                //    message = string.Format(Res.DialogUnhandledExceptionEventMessage, e.Exception.Message);
                //else
                    message = string.Format(Res.DialogUnhandledExceptionMessage, e.Exception.Message);
            }

            MessageBox.Show(message, string.Format(Res.DialogUnhandledExceptionTitle, GlobalSettings.GetAppVersion()), MessageBoxButton.OK, MessageBoxImage.Error);

            TempfileUtil.Dispose();

            e.Handled = true;

            if (Application.Current != null)
            {
                Application.Current.Shutdown();
            }
        }

        #endregion
    }
}
