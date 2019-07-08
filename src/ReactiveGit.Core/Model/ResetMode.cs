// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveGit.Core.Model
{
    /// <summary>
    /// The reset mode when it comes to resetting a git object.
    /// </summary>
    public enum ResetMode
    {
        /// <summary>
        /// Soft commit will not change the index/working directory, only the head.
        /// </summary>
        Soft,

        /// <summary>
        /// Changes the head and index, but not the working directory.
        /// </summary>
        Mixed,

        /// <summary>
        /// Resets the head, index and working directory.
        /// </summary>
        Hard
    }
}