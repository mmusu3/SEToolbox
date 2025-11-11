using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Res = SEToolbox.Properties.Resources;

namespace SEToolbox.Support;

partial class Log
{
    public static void Exception(Exception exception)
    {
        var diagReport = new StringBuilder();
        diagReport.AppendLine(Res.ClsErrorUnhandled);

        var appFile = Path.GetFullPath(Assembly.GetEntryAssembly().Location);
        var appFilePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        diagReport.Append(Res.ClsErrorApplication).Append(' ').Append(ObsufacatePathNames(appFile)).AppendLine();
        diagReport.Append(Res.ClsErrorCommandLine).Append(' ').Append(ObsufacatePathNames(Environment.CommandLine)).AppendLine();
        diagReport.Append(Res.ClsErrorCurrentDirectory).Append(' ').Append(ObsufacatePathNames(Environment.CurrentDirectory)).AppendLine();
        diagReport.Append(Res.ClsErrorSEBinPath).Append(' ').Append(GlobalSettings.Default.SEBinPath).AppendLine();
        diagReport.Append(Res.ClsErrorSEBinVersion).Append(' ').Append(GlobalSettings.Default.SEVersion).AppendLine();
        diagReport.Append(Res.ClsErrorProcessorCount).Append(' ').Append(Environment.ProcessorCount).AppendLine();
        diagReport.Append(Res.ClsErrorOSVersion).Append(' ').Append(Environment.OSVersion).AppendLine();
        diagReport.Append(Res.ClsErrorVersion).Append(' ').Append(Environment.Version).AppendLine();
        diagReport.Append(Res.ClsErrorIs64BitOperatingSystem).Append(' ').Append(Environment.Is64BitOperatingSystem).AppendLine();
        diagReport.Append(Res.ClsErrorIntPtrSize).Append(' ').Append(IntPtr.Size).AppendLine();
        diagReport.Append(Res.ClsErrorIsAdmin).Append(' ').Append(ToolboxUpdater.IsRuningElevated()).AppendLine();
        diagReport.Append(Res.ClsErrorCurrentUICulture).Append(' ').Append(CultureInfo.CurrentUICulture.IetfLanguageTag).AppendLine();
        diagReport.Append(Res.ClsErrorCurrentCulture).Append(' ').Append(CultureInfo.CurrentCulture.IetfLanguageTag).AppendLine();
        diagReport.Append(Res.ClsErrorTimesStartedTotal).Append(' ').Append(GlobalSettings.Default.TimesStartedTotal).AppendLine();
        diagReport.Append(Res.ClsErrorTimesStartedLastReset).Append(' ').Append(GlobalSettings.Default.TimesStartedLastReset).AppendLine();
        diagReport.Append(Res.ClsErrorTimesStartedLastGameUpdate).Append(' ').Append(GlobalSettings.Default.TimesStartedLastGameUpdate).AppendLine();
        diagReport.AppendLine();
        diagReport.Append(Res.ClsErrorFiles).AppendLine();

        if (appFilePath != null)
        {
            var files = Directory.GetFiles(appFilePath);

            foreach (var file in files)
            {
                var filename = Path.GetFileName(file);
                var fileInfo = new FileInfo(file);
                var fileVer = FileVersionInfo.GetVersionInfo(file);

                diagReport.AppendFormat("{0:O}\t{1:#,###0}\t{2}\t{3}\r\n", fileInfo.LastWriteTime, fileInfo.Length, fileVer.FileVersion, filename);
            }
        }

        WriteLine(diagReport.ToString(), LogLevel.FATAL, exception);
    }

    static string ObsufacatePathNames(string path)
    {
        return path.Replace($@"\{Environment.UserName}\", @"\%USERNAME%\");
    }
}
