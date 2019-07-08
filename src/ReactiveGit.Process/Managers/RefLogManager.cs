// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using ReactiveGit.Core.Exceptions;
using ReactiveGit.Core.Managers;
using ReactiveGit.Core.Model;

namespace ReactiveGit.RunProcess.Managers
{
    /// <summary>
    /// Manages handling ref log instances.
    /// </summary>
    public class RefLogManager : IRefLogManager
    {
        private readonly IGitProcessManager _gitProcessManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefLogManager" /> class.
        /// </summary>
        /// <param name="gitProcessManager">The git process to use.</param>
        public RefLogManager(IGitProcessManager gitProcessManager)
        {
            _gitProcessManager = gitProcessManager;
        }

        /// <inheritdoc />
        public IObservable<GitRefLog> GetRefLog(GitBranch branch, IScheduler scheduler = null)
        {
            if (branch == null)
            {
                throw new ArgumentNullException(nameof(branch));
            }

            string[] arguments = { "reflog", "--format=\"%H\u001f%h\u001f%gd\u001f%gs\u001f%ci\"", branch.FriendlyName };

            return _gitProcessManager.RunGit(arguments, scheduler: scheduler).Select(StringToRefLog);
        }

        private static GitRefLog StringToRefLog(string line)
        {
            var fields = line.Split('\u001f');

            if (fields.Length != 5)
            {
                throw new GitProcessException($"Cannot process ref log entry {line}");
            }

            var sha = fields[0];
            var shaShort = fields[1];
            var refLogSubject = fields[3].Split(new[] { ':' }, 2);
            var operation = refLogSubject[0];
            var condenseText = refLogSubject[1];

            if (!DateTime.TryParse(fields[4], out var commitDate))
            {
                throw new GitProcessException("Could not convert the input into valid date time " + fields[4]);
            }

            return new GitRefLog(sha, shaShort, operation, condenseText, commitDate);
        }
    }
}