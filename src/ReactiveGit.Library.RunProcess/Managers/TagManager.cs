// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using ReactiveGit.Library.Core.Exceptions;
using ReactiveGit.Library.Core.Managers;
using ReactiveGit.Library.Core.Model;

namespace ReactiveGit.Library.RunProcess.Managers
{
    /// <summary>
    /// Implementation of the tag manager using command line git processes.
    /// </summary>
    public class TagManager : ITagManager
    {
        private readonly IGitProcessManager _processManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="TagManager"/> class.
        /// </summary>
        /// <param name="processManager">The git process manager which will invoke git commands.</param>
        public TagManager(IGitProcessManager processManager)
        {
            _processManager = processManager;
        }

        /// <inheritdoc />
        public IObservable<GitTag> GetTags(IScheduler scheduler = null)
        {
            string[] arguments = { "tag", "-l", "--format=\"%(refname:short)\u001f%(taggerdate:iso)\u001f%(objectname)\u001f%(objectname:short)\"" };
            return _processManager.RunGit(arguments, scheduler: scheduler).Select(StringToGitTag);
        }

        /// <inheritdoc />
        public string GetMessage(GitTag gitTag)
        {
            if (gitTag == null)
            {
                throw new ArgumentNullException(nameof(gitTag));
            }

            string[] arguments = { "show", "--format=\"%B\"", gitTag.Name };
            var listValue = _processManager.RunGit(arguments).ToList().Wait();

            return string.Join("\r\n", listValue).Trim(' ', '\r', '\n');
        }

        private GitTag StringToGitTag(string line)
        {
            var fields = line.Split('\u001f');

            if (fields.Length != 4)
            {
                throw new GitProcessException($"Cannot process tag entry {line}");
            }

            var name = fields[0];

            if (!DateTime.TryParse(fields[1], out var tagDate))
            {
                throw new GitProcessException("Unable to parse Date Time string" + fields[1]);
            }

            var sha = fields[2];
            var shaShort = fields[3];
            return new GitTag(this, name, shaShort, sha, tagDate);
        }
    }
}
