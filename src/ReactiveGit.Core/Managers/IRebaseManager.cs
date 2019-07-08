// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Reactive;
using System.Reactive.Concurrency;
using ReactiveGit.Core.Model;

namespace ReactiveGit.Core.Managers
{
    /// <summary>
    /// Manager relating to rebasing activities.
    /// </summary>
    public interface IRebaseManager
    {
        /// <summary>
        /// Aborts a attempt to squash/rebase.
        /// </summary>
        /// <param name="scheduler">The scheduler to schedule the task on.</param>
        /// <returns>Details about the operation.</returns>
        IObservable<Unit> Abort(IScheduler scheduler = null);

        /// <summary>
        /// Indicates we want to continue after a conflict was found.
        /// </summary>
        /// <param name="commitMessage">The commit message to use.</param>
        /// <param name="scheduler">The scheduler to schedule the task on.</param>
        /// <returns>Details about the operation.</returns>
        IObservable<Unit> ContinueRebase(string commitMessage, IScheduler scheduler = null);

        /// <summary>
        /// Determines if there are any conflicts.
        /// </summary>
        /// <param name="scheduler">The scheduler to schedule the task on.</param>
        /// <returns>True if there is a conflict, false otherwise.</returns>
        IObservable<bool> HasConflicts(IScheduler scheduler = null);

        /// <summary>
        /// Determines if there is currently a rebase in operation.
        /// </summary>
        /// <returns>True if rebase in operation, false otherwise.</returns>
        bool IsRebaseHappening();

        /// <summary>
        /// Performs the squash action.
        /// </summary>
        /// <param name="parentBranch">The branch parent to parent the rebase/squash from.</param>
        /// <param name="scheduler">The scheduler to schedule the task on.</param>
        /// <returns>Details about the commit.</returns>
        IObservable<Unit> Rebase(GitBranch parentBranch, IScheduler scheduler = null);

        /// <summary>
        /// Skips the specified commit in a rebase.
        /// </summary>
        /// <param name="scheduler">The scheduler to schedule the task on.</param>
        /// <returns>Details about the operation.</returns>
        IObservable<Unit> Skip(IScheduler scheduler = null);

        /// <summary>
        /// Performs the squash action.
        /// </summary>
        /// <param name="newCommitMessage">The new commit message for the squashed commit.</param>
        /// <param name="startCommit">The commit to start the rebase/squash from.</param>
        /// <param name="scheduler">The scheduler to schedule the task on.</param>
        /// <returns>Details about the commit.</returns>
        IObservable<Unit> Squash(string newCommitMessage, GitCommit startCommit, IScheduler scheduler = null);
    }
}
