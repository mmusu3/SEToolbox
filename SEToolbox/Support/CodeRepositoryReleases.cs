namespace SEToolbox.Support
{
    using Octokit;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Extracts the GitHub website information to determine the version of the current release.
    /// </summary>
    public class CodeRepositoryReleases
    {
        public static ApplicationRelease CheckForUpdates(Version currentVersion, bool dontIgnore = false)
        {
            if (!dontIgnore)
            {
                if (GlobalSettings.Default.AlwaysCheckForUpdates.HasValue && !GlobalSettings.Default.AlwaysCheckForUpdates.Value)
                    return null;

#if DEBUG
                // Skip the load check, as it may take a few seconds during development.
                if (Debugger.IsAttached)
                    return null;
#endif
            }

            // Accessing GitHub API directly for updates.
            GitHubClient client = new GitHubClient(new ProductHeaderValue("SEToolbox-Updater"));

            Release latest;

            try
            {
                latest = client.Repository.Release.GetLatest("mmusu3", "SEToolbox").Result;
            }
            catch (Exception ex)
            {
                // Network connection error.
                if (ex?.InnerException is HttpRequestException ||
                    ex?.InnerException?.InnerException is WebException)
                {
                    return null;
                }

                throw;
            }

            var item = new ApplicationRelease {
                Name = latest.Name,
                Link = latest.HtmlUrl,
                Version = GetVersion(latest.TagName),
                Notes = latest.Body
            };

            Version ignoreVersion;
            Version.TryParse(GlobalSettings.Default.IgnoreUpdateVersion, out ignoreVersion);

            if (item.Version > currentVersion && item.Version != ignoreVersion || dontIgnore)
                return item;

            return null;
        }


        private static Version GetVersion(string version)
        {
            var match = Regex.Match(version, @"v?(?<v1>\d+)\.(?<v2>\d+)\.(?<v3>\d+)\sRelease\s(?<v4>\d+)");

            if (match.Success)
            {
                return new Version(match.Groups["v1"].Value + "." + match.Groups["v2"].Value + "." + match.Groups["v3"].Value + "." + match.Groups["v4"].Value);
            }

            match = Regex.Match(version, @"v?(?<v1>\d+)\.(?<v2>\d+)\.(?<v3>\d+).(?<v4>\d+)");

            if (match.Success)
            {
                return new Version(match.Groups["v1"].Value + "." + match.Groups["v2"].Value + "." + match.Groups["v3"].Value + "." + match.Groups["v4"].Value);
            }

            return new Version(0, 0, 0, 0);
        }
    }

    public class ApplicationRelease
    {
        public string Name { get; set; }
        public string Link { get; set; }
        public Version Version { get; set; }
        public string Notes { get; set; }
    }
}
