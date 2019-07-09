// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.IO;

namespace ReactiveGit.Library.RunProcess.Helpers
{
    /// <summary>
    /// Helper which assists with file paths.
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// Determine if the directory is empty, ie. no files and no sub-directories.
        /// </summary>
        /// <param name="path">directory to inspect.</param>
        /// <returns>true if directory is empty, false otherwise.</returns>
        public static bool IsDirectoryEmpty(string path)
        {
            return IsDirectoryEmpty(new DirectoryInfo(path));
        }

        /// <summary>
        /// Determine if the directory is empty, ie. no files and no sub-directories.
        /// </summary>
        /// <param name="directory">directory to inspect.</param>
        /// <returns>true if directory is empty, false otherwise.</returns>
        public static bool IsDirectoryEmpty(DirectoryInfo directory)
        {
            if (directory == null)
            {
                throw new System.ArgumentNullException(nameof(directory));
            }

            var files = directory.GetFiles();
            var subDirectories = directory.GetDirectories();

            return (files.Length == 0) && (subDirectories.Length == 0);
        }
    }
}
