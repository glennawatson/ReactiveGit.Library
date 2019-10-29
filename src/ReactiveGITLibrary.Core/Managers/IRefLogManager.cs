// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Reactive.Concurrency;
using ReactiveGITLibrary.Core.Model;

namespace ReactiveGITLibrary.Core.Managers
{
    /// <summary>
    /// Represents getting items out of the ref log.
    /// </summary>
    public interface IRefLogManager
    {
        /// <summary>
        /// Gets the ref log for the desired branch. If none is specified then it's all branches.
        /// </summary>
        /// <param name="branch">The branch to get the ref log for.</param>
        /// <param name="scheduler">The scheduler to schedule the task on.</param>
        /// <returns>The ref log items.</returns>
        IObservable<GitRefLog> GetRefLog(GitBranch branch, IScheduler scheduler = null);
    }
}
