// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using ReactiveGit.Core.ExtensionMethods;
using ReactiveGit.Core.Managers;
using ReactiveGit.Core.Model;

namespace ReactiveGit.RunProcess.Managers
{
    /// <summary>
    /// Responsible for handling operations in regards to git objects.
    /// </summary>
    public class GitObjectManager : IGitObjectManager
    {
        private readonly IGitProcessManager _gitProcessManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitObjectManager" /> class.
        /// </summary>
        /// <param name="gitProcessManager">The git process to use.</param>
        public GitObjectManager(IGitProcessManager gitProcessManager)
        {
            _gitProcessManager = gitProcessManager ?? throw new ArgumentNullException(nameof(gitProcessManager));
        }

        /// <inheritdoc />
        public IObservable<Unit> Reset(IGitIdObject gitObject, ResetMode resetMode, IScheduler scheduler = null)
        {
            if (gitObject == null)
            {
                throw new ArgumentNullException(nameof(gitObject));
            }

            var arguments = new[] { "reset", $"--{resetMode.ToString().ToLowerInvariant()}", gitObject.Sha };

            return _gitProcessManager.RunGit(arguments, showInOutput: true, scheduler: scheduler).WhenDone();
        }

        /// <inheritdoc />
        public IObservable<Unit> Checkout(IGitIdObject gitObject, bool force, IScheduler scheduler = null)
        {
            if (gitObject == null)
            {
                throw new ArgumentNullException(nameof(gitObject));
            }

            var arguments = new List<string> { "checkout" };

            if (force)
            {
                arguments.Add("--force");
            }

            arguments.Add(gitObject.Sha);

            return _gitProcessManager.RunGit(arguments, showInOutput: true, scheduler: scheduler).WhenDone();
        }
    }
}