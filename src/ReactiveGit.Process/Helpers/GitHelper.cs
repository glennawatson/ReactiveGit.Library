﻿// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace ReactiveGit.RunProcess.Helpers
{
    /// <summary>
    /// Helper class for finding details about the GIT installation.
    /// </summary>
    public static class GitHelper
    {
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
        public static string GetGitInstallationPath()
        {
            var gitPath = GetInstallPathFromEnvironmentVariable();
            if (gitPath != null)
            {
                return gitPath;
            }

            gitPath = GetInstallPathFromRegistry();
            if (gitPath != null)
            {
                return gitPath;
            }

            gitPath = GetInstallPathFromProgramFiles();
            return gitPath;
        }

        /// <summary>
        /// Attempt to get the installation path from the path variable.
        /// </summary>
        /// <returns>The installation path or null if unable to be found.</returns>
        public static string GetInstallPathFromEnvironmentVariable()
        {
            var path = Environment.GetEnvironmentVariable("PATH");
            if (path == null)
            {
                return null;
            }

            var allPaths = path.Split(';');
            var gitPath = allPaths.FirstOrDefault(p => p.ToLowerInvariant().TrimEnd('\\').EndsWith("git\\cmd", StringComparison.OrdinalIgnoreCase));
            if ((gitPath != null) && Directory.Exists(gitPath))
            {
                gitPath = Directory.GetParent(gitPath).FullName.TrimEnd('\\');
            }

            return gitPath;
        }

        /// <summary>
        /// Attempts to get the installation path searching the program files directory.
        /// </summary>
        /// <returns>The installation path or null if unable to be found.</returns>
        public static string GetInstallPathFromProgramFiles()
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
                    var gitPathX64 = Path.Combine(x64ProgramFiles.ToString(), "git");
                    if (Directory.Exists(gitPathX64))
                    {
                        return gitPathX64.TrimEnd('\\');
                    }
                }
            }

            // Else, this is a 64bit or a 32bit machine, and the user installed 32bit git
            var gitPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "git");
            return Directory.Exists(gitPath) ? gitPath.TrimEnd('\\') : null;
        }

        /// <summary>
        /// Attempt to get the installation path from the registry.
        /// </summary>
        /// <returns>The installation path or null if unable to be found.</returns>
        public static string GetInstallPathFromRegistry()
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
