namespace SEToolbox.Support
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Principal;

    using Microsoft.Win32;

    public static class ToolboxUpdater
    {
        /// <summary>
        /// Required dependancies which must be copied for SEToolbox to work.
        /// </summary>
        internal static readonly string[] CoreSpaceEngineersFiles = {
            "HavokWrapper.dll",                 // x64
            "ProtoBuf.Net.dll",                 // 1.192.x requirement.
            "ProtoBuf.Net.Core.dll",            // 1.192.x requirement.
            "Sandbox.Common.dll",               // AnyCPU
            "Sandbox.Game.dll",                 // x64
            "Sandbox.Game.XmlSerializers.dll",  // 1.191.x requirement.
            "Sandbox.Graphics.dll",             // x64
            "Sandbox.RenderDirect.dll",         // x64      1.187.x requirement.
            "SharpDX.dll",                      // AnyCPU
            "SharpDX.Direct3D11.dll",           // AnyCPU   Required to load Planets.
            "SharpDX.DXGI.dll",                 // AnyCPU   Required to load Planets.
            "SpaceEngineers.Game.dll",          // x64
            "SpaceEngineers.ObjectBuilders.dll",                    // x64
            "SpaceEngineers.ObjectBuilders.XmlSerializers.dll",     // x64
            "steam_api64.dll",                  // x64
            "Steamworks.NET.dll",               // x64      1.187.x requirement.
            "VRage.Ansel.dll",                  // x64      1.181.x requirement.
            "VRage.Audio.dll",                  // MSIL     1.147.x requirement.
            "VRage.dll",                        // AnyCPU
            "VRage.XmlSerializers.dll",
            "VRage.Game.dll",                   // x64
            "VRage.Game.XmlSerializers.dll",    // x64
            "VRage.Input.dll",                  // x64
            "VRage.Library.dll",                // AnyCPU
            "VRage.Math.dll",                   // AnyCPU
            "VRage.Math.XmlSerializers.dll",
            "VRage.Native.dll",                 // x64
            "VRage.NativeWrapper.dll",          // 1.191.x requirement.
            "VRage.Network.dll",
            "VRage.Render.dll",                 // AnyCPU
            "VRage.Render11.dll",               // x64
            "VRage.Scripting.dll",              // x64     1.197.x requirement.
            "VRage.Steam.dll",                  // x64     1.188.x requirement.
            "Steamworks.NET.dll",               // x64     1.188.x requirement.
            "System.Buffers.dll",
            "System.ComponentModel.Annotations.dll",
            "System.Collections.Immutable.dll", // AnyCPU  1.194.x requirement
            "System.Memory.dll",                // MSIL    1.191.x requirement for voxels
            "System.Numerics.Vectors.dll",
            "System.Runtime.CompilerServices.Unsafe.dll",  // MSIL     1.191.x requirement for voxels
            "EmptyKeys.UserInterface.dll",
            "EmptyKeys.UserInterface.Core.dll",
            "SixLabors.Core.dll",
            "SixLabors.ImageSharp.dll"
        };

        internal static readonly string[] OptionalSpaceEngineersFiles = {
            "msvcp120.dll",                     // VRage.Native dependancy.  // testing dropping it these. Keen may have made a mistake by removing them from DS deployment.
            "msvcr120.dll",                     // VRage.Native dependancy.
        };

        //internal static readonly string[] CoreMedievalEngineersFiles = {
        //    "Sandbox.Common.dll",
        //    "MedievalEngineers.ObjectBuilders.dll",
        //    "MedievalEngineers.ObjectBuilders.XmlSerializers.dll",
        //    "Sandbox.Game.dll",
        //    "HavokWrapper.dll",
        //    "VRage.dll",
        //    "VRage.Game.dll",
        //    "VRage.Game.XmlSerializers.dll",
        //    "VRage.Library.dll",
        //    "VRage.Math.dll"
        //};

        #region GetApplicationFilePath

        public static string GetApplicationFilePath()
        {
            var gamePath = GlobalSettings.Default.SEBinPath;

            if (string.IsNullOrEmpty(gamePath))
            {
                // We use the Bin64 Path, as these assemblies are marked "AllCPU", and will work regardless of processor architecture.
                gamePath = GetGameRegistryFilePath();

                if (!string.IsNullOrEmpty(gamePath))
                    gamePath = Path.Combine(gamePath, "Bin64");
            }

            return gamePath;
        }

        public static string GetApplicationContentPath()
        {
            return Path.GetFullPath(Path.Combine(GetApplicationFilePath(), @"..\Content"));
        }

        /// <summary>
        /// Looks for the Space Engineers install location in the Registry, which should return the form:
        /// "C:\Program Files (x86)\Steam\steamapps\common\SpaceEngineers"
        /// </summary>
        /// <returns></returns>
        public static string GetGameRegistryFilePath()
        {
            RegistryKey key;

            if (Environment.Is64BitProcess)
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 244850", false);
            else
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 244850", false);

            if (key != null)
                return key.GetValue("InstallLocation") as string;

            // Backup check, but no choice if the above goes to pot.
            // Using the [Software\Valve\Steam\SteamPath] as a base for "\steamapps\common\SpaceEngineers", is unreliable, as the Steam Library is customizable and could be on another drive and directory.
            var steamPath = GetSteamFilePath();

            if (!string.IsNullOrEmpty(steamPath))
                return Path.Combine(steamPath, @"SteamApps\common\SpaceEngineers");

            return null;
        }

        #endregion

        #region GetSteamFilePath

        /// <summary>
        /// Looks for the Steam install location in the Registry, which should return the form:
        /// "C:\Program Files (x86)\Steam"
        /// </summary>
        /// <returns></returns>
        public static string GetSteamFilePath()
        {
            RegistryKey key;

            if (Environment.Is64BitProcess)
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Valve\Steam", false);
            else
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Valve\Steam", false);

            if (key != null)
                return (string)key.GetValue("InstallPath");

            return null;
        }

        #endregion

        #region IsSpaceEngineersInstalled

        /// <summary>
        /// Checks for key directory names from the game bin folder.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool ValidateSpaceEngineersInstall(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            if (!Directory.Exists(filePath))
                return false;

            if (!Directory.Exists(Path.Combine(filePath, @"..\Content")))
                return false;

            // Validate that all core SE assemblies are present, otherwise it's pointless continuing.
            if (CoreSpaceEngineersFiles.Any(filename => !File.Exists(Path.Combine(filePath, filename))))
                return false;

            // Skip checking for the .exe. Not required for the Toolbox currently.
            return true;
        }

        #endregion

        #region IsBaseAssembliesChanged

        public static bool IsBaseAssembliesChanged()
        {
            var baseFilePath = GetApplicationFilePath();
            var appFilePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            foreach (var filename in CoreSpaceEngineersFiles)
            {
                if (DoFilesDiffer(baseFilePath, appFilePath, filename))
                    return true;
            }

            return false;
        }

        public static bool UpdateBaseFiles()
        {
            var baseFilePath = GetApplicationFilePath();
            var appFilePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            foreach (var filename in CoreSpaceEngineersFiles)
            {
                var sourceFile = Path.Combine(baseFilePath, filename);

                if (File.Exists(sourceFile))
                    File.Copy(sourceFile, Path.Combine(appFilePath, filename), true);
            }

            foreach (var filename in OptionalSpaceEngineersFiles)
            {
                var sourceFile = Path.Combine(baseFilePath, filename);

                if (File.Exists(sourceFile))
                    File.Copy(sourceFile, Path.Combine(appFilePath, filename), true);
            }

            return true;
        }

        #endregion

        #region DoFilesDiffer

        public static bool DoFilesDiffer(string directory1, string directory2, string filename)
        {
            return DoFilesDiffer(Path.Combine(directory1, filename), Path.Combine(directory2, filename));
        }

        public static bool DoFilesDiffer(string file1, string file2)
        {
            if (!File.Exists(file2))
                return true;

            if (File.Exists(file1) != File.Exists(file2))
                return false;

            var buffer1 = File.ReadAllBytes(file1);
            var buffer2 = File.ReadAllBytes(file2);

            if (buffer1.Length != buffer2.Length)
                return true;

            return !buffer1.SequenceEqual(buffer2);
        }

        #endregion

        #region IsRuningElevated

        private static bool? _isRuningElevated = null;

        internal static bool IsRuningElevated()
        {
            if (_isRuningElevated.HasValue)
                return _isRuningElevated.Value;

            var identity = WindowsIdentity.GetCurrent();

            if (identity == null)
                return false;

            var pricipal = new WindowsPrincipal(identity);
            _isRuningElevated = pricipal.IsInRole(WindowsBuiltInRole.Administrator);

            return _isRuningElevated.Value;
        }

        #endregion

        #region RunElevated

        internal static int? RunElevated(string fileName, string arguments, bool elevate, bool waitForExit)
        {
            var processInfo = new ProcessStartInfo {
                FileName = fileName,
                Arguments = arguments
            };

            if (elevate)
                processInfo.Verb = "runas";

            try
            {
                var process = Process.Start(processInfo);

                if (waitForExit && process != null)
                {
                    process.WaitForExit();

                    return process.ExitCode;
                }

                return 0;
            }
            catch (Win32Exception)
            {
                // Do nothing. Probably the user canceled the UAC window
                return null;
            }
        }

        #endregion
    }
}
