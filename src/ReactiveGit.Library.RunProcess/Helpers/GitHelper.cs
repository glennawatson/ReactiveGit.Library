// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Win32;
using Splat;

namespace ReactiveGit.Library.RunProcess.Helpers
{
    /// <summary>
    /// Helper class for finding details about the GIT installation.
    /// </summary>
    public static class GitHelper
    {
        private static ILogger _logger;
        private static Lazy<string> _gitInstallationPath;

        /// <summary>
        /// Initializes static members of the <see cref="GitHelper"/> class.
        /// </summary>
        static GitHelper()
        {
            _logger = Locator.Current.GetService<ILogManager>().GetLogger(typeof(GitHelper));
            _gitInstallationPath = new Lazy<string>(GetGitInstallationPathInternal, LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets the binary path to the GIT executable.
        /// </summary>
        /// <returns>The path to the GIT executable.</returns>
        public static string GetGitBinPath()
        {
            var installationPath = GetGitInstallationPath();
            if (installationPath == null)
            {
                return null;
            }

            var binPath = Path.Combine(installationPath, "usr/bin");
            return Directory.Exists(binPath) ? binPath : Path.Combine(installationPath, "bin");
        }

        /// <summary>
        /// Gets the path to the installation path.
        /// </summary>
        /// <returns>The installation path.</returns>
        public static string GetGitInstallationPath() => _gitInstallationPath.Value;

        /// <summary>
        /// Gets the path to the installation path.
        /// </summary>
        /// <returns>The installation path.</returns>
        private static string GetGitInstallationPathInternal()
        {
            var gitPath = GetInstallPathFromEnvironmentVariable();
            if (gitPath != null)
            {
                _logger.Write("Found GIT in directory from the environment path: " + gitPath, LogLevel.Debug);
                return gitPath;
            }

            gitPath = GetInstallPathFromRegistry();
            if (gitPath != null)
            {
                _logger.Write("Found GIT in directory from the registry: " + gitPath, LogLevel.Debug);
                return gitPath;
            }

            gitPath = GetInstallPathFromProgramFiles();

            _logger.Write("Found GIT in directory from program files: " + gitPath, LogLevel.Debug);

            return gitPath;
        }

        /// <summary>
        /// Attempt to get the installation path from the path variable.
        /// </summary>
        /// <returns>The installation path or null if unable to be found.</returns>
        private static string GetInstallPathFromEnvironmentVariable()
        {
            var path = Environment.GetEnvironmentVariable("PATH");

            var allPaths = path?.Split(';');
            return allPaths?.FirstOrDefault(p => p.ToLowerInvariant().TrimEnd('\\').EndsWith("git\\cmd", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Attempts to get the installation path searching the program files directory.
        /// </summary>
        /// <returns>The installation path or null if unable to be found.</returns>
        private static string GetInstallPathFromProgramFiles()
        {
            // If this is a 64bit OS, and the user installed 64bit git, then explictly search that folder.
            if (Environment.Is64BitOperatingSystem)
            {
                var x64ProgramFiles =
                    Registry.GetValue(
                        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion",
                        "ProgramW6432Dir",
                        null);

                if (x64ProgramFiles != null)
                {
                    var gitPathX64 = Path.Combine(x64ProgramFiles.ToString(), "git\\cmd");
                    if (Directory.Exists(gitPathX64))
                    {
                        return gitPathX64.TrimEnd('\\');
                    }
                }
            }

            // Else, this is a 64bit or a 32bit machine, and the user installed 32bit git
            var gitPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "git\\cmd");
            return Directory.Exists(gitPath) ? gitPath.TrimEnd('\\') : null;
        }

        /// <summary>
        /// Attempt to get the installation path from the registry.
        /// </summary>
        /// <returns>The installation path or null if unable to be found.</returns>
        private static string GetInstallPathFromRegistry()
        {
            // Check reg key for msysGit 2.6.1+
            var installLocation = Registry.GetValue(
                @"HKEY_LOCAL_MACHINE\SOFTWARE\GitForWindows",
                "InstallPath",
                null);
            if ((installLocation != null) && Directory.Exists(installLocation.ToString().TrimEnd('\\')))
            {
                return installLocation.ToString().TrimEnd('\\');
            }

            // Check uninstall key for older versions
            installLocation =
                Registry.GetValue(
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Git_is1",
                    "InstallLocation",
                    null);
            if ((installLocation != null) && Directory.Exists(installLocation.ToString().TrimEnd('\\')))
            {
                return installLocation.ToString().TrimEnd('\\');
            }

            // try 32-bit OS
            installLocation =
                Registry.GetValue(
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Git_is1",
                    "InstallLocation",
                    null);
            if ((installLocation != null) && Directory.Exists(installLocation.ToString().TrimEnd('\\')))
            {
                return installLocation.ToString().TrimEnd('\\');
            }

            return null;
        }
    }
}
