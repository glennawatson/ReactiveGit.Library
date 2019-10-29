// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using ReactiveGITLibrary.Core.Managers;
using ReactiveGITLibrary.Core.Model;

namespace ReactiveGit.Library.RunProcess.Managers
{
    /// <summary>
    /// Manager that handles commits.
    /// </summary>
    public class CommitManager : ICommitManager
    {
        private readonly IGitProcessManager _gitProcessManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommitManager" /> class.
        /// </summary>
        /// <param name="gitProcessManager">The git process to use.</param>
        public CommitManager(IGitProcessManager gitProcessManager)
        {
            _gitProcessManager = gitProcessManager;
        }

        /// <inheritdoc/>
        public IObservable<Unit> Create(string message, GitUserDetails author = null, GitUserDetails committer = null)
        {
            var environmentVariables =
                new Dictionary<string, string>();

            if (author != null)
            {
                environmentVariables.Add("GIT_AUTHOR_NAME", author.Name);
                environmentVariables.Add("GIT_AUTHOR_EMAIL", author.Email);
            }

            if (committer != null)
            {
                environmentVariables.Add("GIT_COMMITTER_NAME", committer.Name);
                environmentVariables.Add("GIT_COMMITTER_EMAIL", committer.Email);
            }

            var fileName = Path.GetTempFileName();

            File.WriteAllText(fileName, message);

            return _gitProcessManager.RunGit(new[] { "commit", "-F", fileName }, environmentVariables)
                .Select(_ => Unit.Default)
                .Do(_ => File.Delete(fileName));
        }
    }
}