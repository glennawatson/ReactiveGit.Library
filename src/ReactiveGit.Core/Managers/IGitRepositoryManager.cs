// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Reactive;

namespace ReactiveGit.Core.Managers
{
    /// <summary>
    /// A git repository instance.
    /// </summary>
    public interface IGitRepositoryManager
    {
        /// <summary>
        /// Gets a observable which will monitor the output from GIT.
        /// </summary>
        IObservable<string> GitOutput { get; }

        /// <summary>
        /// Gets a obseravble which will indicate when the repository is updated.
        /// </summary>
        IObservable<Unit> GitUpdated { get; }

        /// <summary>
        /// Gets the path to the repository.
        /// </summary>
        string RepositoryPath { get; }
    }
}
