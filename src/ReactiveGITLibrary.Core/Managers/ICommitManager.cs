// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Reactive;
using ReactiveGITLibrary.Core.Model;

namespace ReactiveGITLibrary.Core.Managers
{
    /// <summary>
    /// A manager which will manage commits.
    /// </summary>
    public interface ICommitManager
    {
        /// <summary>
        /// Create a new commit.
        /// </summary>
        /// <param name="message">The message about the commit.</param>
        /// <param name="author">Optional author of the commit.</param>
        /// <param name="committer">Optional committer of the commit.</param>
        /// <returns>An observable which signals when the commit has been committed.</returns>
        IObservable<Unit> Create(string message, GitUserDetails author = null, GitUserDetails committer = null);
    }
}
