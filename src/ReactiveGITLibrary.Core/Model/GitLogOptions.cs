// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace ReactiveGITLibrary.Core.Model
{
    /// <summary>
    /// A list of options when we are doing.
    /// </summary>
    [Flags]
    public enum GitLogOptions
    {
        /// <summary>
        /// If there are no additional options for the log.
        /// </summary>
        None = 0,

        /// <summary>
        /// Order the log items in topological ordering.
        /// </summary>
        TopologicalOrder = 1,

        /// <summary>
        /// Include merges in the log entries.
        /// </summary>
        IncludeMerges = 2,

        /// <summary>
        /// Include remote commits in the log entries.
        /// </summary>
        IncludeRemotes = 4,

        /// <summary>
        /// Include branch only commits and the parent.
        /// </summary>
        BranchOnlyAndParent = 8
    }
}
