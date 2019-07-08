// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Reactive.Concurrency;
using ReactiveGit.Core.Model;

namespace ReactiveGit.Core.Managers
{
    /// <summary>
    /// A manager for handling tags.
    /// </summary>
    public interface ITagManager
    {
        /// <summary>
        /// Get the tags inside the git repository.
        /// </summary>
        /// <param name="scheduler">The scheduler to schedule on.</param>
        /// <returns>A observable of the tags.</returns>
        IObservable<GitTag> GetTags(IScheduler scheduler = null);

        /// <summary>
        /// Gets the message for a tag.
        /// </summary>
        /// <param name="gitTag">The tag to get a message for.</param>
        /// <returns>The message for the tag.</returns>
        string GetMessage(GitTag gitTag);
    }
}
