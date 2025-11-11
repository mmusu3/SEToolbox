namespace SEToolbox
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
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
        private CoreToolbox _toolboxApplication;

        private void OnStartup(object sender, StartupEventArgs e)
        {
            bool appendLog = Enumerable.Contains(e.Args, "/appendlog");

            Log.Init("./log.txt", appendLog);
            Log.Info("Starting.");

            var settings = GlobalSettings.Default;

            Log.Info("Loading settings.");
            settings.Load();

            if ((NativeMethods.GetKeyState(System.Windows.Forms.Keys.ShiftKey) & KeyStates.Down) == KeyStates.Down)
            {
                Log.Info("Restting global settings.");

                // Reset User Settings when Shift is held down during start up.
                settings.Reset();
                settings.PromptUser = true;
            }

            LocalizeDictionary.Instance.SetCurrentThreadCulture = false;
            LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfoByIetfLanguageTag(settings.LanguageCode);
            Thread.CurrentThread.CurrentUICulture = LocalizeDictionary.Instance.Culture;

            Log.Debug("Showing splash screen.");

            Splasher.Splash = new WindowSplashScreen();
            Splasher.ShowSplash();

            Log.Info("Checking for updates.");

            var update = CodeRepositoryReleases.CheckForUpdates(GlobalSettings.GetAppVersion());

            if (update != null)
            {
                Log.Info($"Found update: {update.Version}");

                var message = string.IsNullOrEmpty(update.Notes)
                    ? string.Format(Res.DialogNewVersionMessage, update.Version)
                    : string.Format(Res.DialogNewVersionNotesMessage, update.Version, update.Notes);

                var dialogResult = MessageBox.Show(message, Res.DialogNewVersionTitle, MessageBoxButton.YesNo, MessageBoxImage.Information);

                if (dialogResult == MessageBoxResult.Yes)
                {
                    Log.Debug("Opening update link.");

                    Process.Start(update.Link); // Opens release URL in browser
                    settings.Save();
                    Application.Current.Shutdown();
                    return;
                }
                else if (dialogResult == MessageBoxResult.No)
                {
                    Log.Info($"Ingoring update version: {update.Version}");

                    settings.IgnoreUpdateVersion = update.Version.ToString();
                }
            }

            Log.Debug("Configuring ServiceLocator.");

            // Configure service locator.
            ServiceLocator.RegisterSingleton<IDialogService, DialogService>();
            ServiceLocator.Register<IOpenFileDialog, OpenFileDialogViewModel>();
            ServiceLocator.Register<ISaveFileDialog, SaveFileDialogViewModel>();
            ServiceLocator.Register<IColorDialog, ColorDialogViewModel>();
            ServiceLocator.Register<IFolderBrowserDialog, FolderBrowserDialogViewModel>();

            FrameworkCompatibilityPreferences.KeepTextBoxDisplaySynchronizedWithTextProperty = false;

            Log.Debug("Initializing CoreToolbox.");

            _toolboxApplication = new CoreToolbox();

            if (_toolboxApplication.Init(e.Args))
            {
                Log.Info("Loading.");

                _toolboxApplication.Load(e.Args);

                Log.Info("Closing.");
            }
            else
            {
                Log.Debug("Init returned false, closing early.");

                Application.Current.Shutdown();
            }
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            _toolboxApplication?.Exit();
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

            Log.Exception(e.Exception);

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

            Application.Current?.Shutdown();
        }
    }
}
