// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using ReactiveGITLibrary.Core.Exceptions;
using ReactiveGITLibrary.Core.ExtensionMethods;
using ReactiveGITLibrary.Core.Managers;
using ReactiveGITLibrary.Core.Model;

namespace ReactiveGit.Library.RunProcess.Managers
{
    /// <summary>
    /// Helper which manages branch history.
    /// </summary>
    public sealed class BranchManager : IBranchManager
    {
        private readonly Subject<GitBranch> _currentBranch = new Subject<GitBranch>();

        private readonly IGitProcessManager _gitProcessManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="BranchManager" /> class.
        /// </summary>
        /// <param name="gitProcessManager">The git process to use.</param>
        public BranchManager(IGitProcessManager gitProcessManager)
        {
            _gitProcessManager = gitProcessManager;
        }

        /// <inheritdoc />
        public IObservable<GitBranch> CurrentBranch
        {
            get
            {
                GetCurrentCheckedOutBranch();
                return _currentBranch;
            }
        }

        /// <inheritdoc />
        public IObservable<Unit> CheckoutBranch(GitBranch branch, bool force = false, IScheduler scheduler = null)
        {
            if (branch == null)
            {
                throw new ArgumentNullException(nameof(branch));
            }

            IList<string> arguments = new List<string> { $"checkout {branch.FriendlyName}" };

            if (force)
            {
                arguments.Add("-f");
            }

            var observable = _gitProcessManager.RunGit(arguments, showInOutput: true, scheduler: scheduler).WhenDone();
            return observable.Finally(() => _currentBranch.OnNext(branch));
        }

        /// <inheritdoc />
        public IObservable<int> GetCommitCount(GitBranch branchName, IScheduler scheduler = null)
        {
            if (branchName == null)
            {
                throw new ArgumentNullException(nameof(branchName));
            }

            return _gitProcessManager.RunGit(new[] { $"rev-list --count {branchName.FriendlyName}" }, scheduler: scheduler)
                .ToList()
                .FirstAsync()
                .Select(x => Convert.ToInt32(x, CultureInfo.InvariantCulture))
                .FirstAsync();
        }

        /// <inheritdoc />
        public IObservable<string> GetCommitMessageLong(GitCommit commit, IScheduler scheduler = null)
        {
            if (commit == null)
            {
                throw new ArgumentNullException(nameof(commit));
            }

            return _gitProcessManager.RunGit(new[] { "log", "--format=%B", "-n 1", commit.Sha }, scheduler: scheduler)
                .Select(x => x.Trim().Trim('\r', '\n'))
                .ToList()
                .Select(result => string.Join("\r\n", result).Trim().Trim('\r', '\n', ' '));
        }

        /// <inheritdoc />
        public IObservable<string> GetCommitMessagesAfterParent(GitCommit parent, IScheduler scheduler = null)
        {
            return CurrentBranch.Select(branch => ExtractLogParameter(branch, 0, 0, GitLogOptions.None, $"{parent.Sha}..HEAD"))
                .Switch()
                .Select(x => _gitProcessManager.RunGit(x, scheduler: scheduler))
                .Switch()
                .Select(x => ConvertStringToGitCommit(x).MessageLong.Select(y => y.Trim('\r', '\n')))
                .Switch();
        }

        /// <inheritdoc />
        public IObservable<GitCommit> GetCommitsForBranch(
            GitBranch branch,
            int skip,
            int limit,
            GitLogOptions logOptions,
            IScheduler scheduler = null)
        {
            return Observable.Return(new[] { "log" })
                .CombineLatest(ExtractLogParameter(branch, skip, limit, logOptions, "HEAD"), (cmd, other) => cmd.Concat(other))
                .SelectMany(x => _gitProcessManager.RunGit(x, scheduler: scheduler).Select(ConvertStringToGitCommit));
        }

        /// <inheritdoc />
        public IObservable<GitBranch> GetLocalAndRemoteBranches(IScheduler scheduler = null)
        {
            return GetLocalBranches(scheduler).Merge(GetRemoteBranches());
        }

        /// <inheritdoc />
        public IObservable<GitBranch> GetLocalBranches(IScheduler scheduler = null)
        {
            return
                _gitProcessManager.RunGit(new[] { "branch" }, scheduler: scheduler).Select(
                    line => new GitBranch(line.Substring(2), false, line[0] == '*'));
        }

        /// <inheritdoc />
        public IObservable<GitBranch> GetRemoteBranch(GitBranch branch, IScheduler scheduler = null)
        {
            return Observable.Return<GitBranch>(null);
        }

        /// <inheritdoc />
        public IObservable<GitBranch> GetRemoteBranches(IScheduler scheduler = null)
        {
            return _gitProcessManager.RunGit(new[] { "branch" }, scheduler: scheduler).Select(
                line =>
                    {
                        var arrowPos = line.IndexOf(" -> ", StringComparison.InvariantCulture);
                        var branch = line;
                        if (arrowPos != -1)
                        {
                            branch = line.Substring(0, arrowPos);
                        }

                        return new GitBranch(branch.Trim(), true, false);
                    });
        }

        /// <inheritdoc />
        public IObservable<bool> IsMergeConflict(IScheduler scheduler = null)
        {
            return _gitProcessManager.RunGit(new[] { "ls-files", "-u" }, scheduler: scheduler).Any();
        }

        /// <inheritdoc />
        public IObservable<bool> IsWorkingDirectoryDirty(IScheduler scheduler = null)
        {
            string[] arguments = { "status", "--porcelain", "--ignore-submodules=dirty", "--untracked-files=all" };

            return _gitProcessManager.RunGit(arguments, scheduler: scheduler).Any();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _currentBranch?.Dispose();
        }

        private static void GenerateFormat(IList<string> arguments)
        {
            var formatString = new StringBuilder("--format=%H\u001f%h\u001f%P\u001f");
            formatString.Append("%ci")
                .Append("\u001f%cn\u001f%ce\u001f%an\u001f%ae\u001f%d\u001f%s\u001f");
            arguments.Add(formatString.ToString());
            arguments.Add("--decorate=full");
            arguments.Add("--date=iso");
        }

        private GitCommit ConvertStringToGitCommit(string line)
        {
            var fields = line.Split('\u001f');

            if (fields.Length != 11)
            {
                return null;
            }

            var changeset = fields[0];
            var changesetShort = fields[1];
            var parents =
                fields[2].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(
                    x => x.Trim('\r', '\n').Trim()).ToArray();

            if (!DateTime.TryParse(fields[3], out var commitDate))
            {
                throw new GitProcessException("Date time was not a valid format " + fields[3]);
            }

            var committer = fields[4];
            var commiterEmail = fields[5];
            var author = fields[6];
            var authorEmail = fields[7];
            var refs = fields[8];
            var messageShort = fields[9];

            return new GitCommit(
                       this,
                       changeset,
                       messageShort,
                       commitDate,
                       author,
                       authorEmail,
                       committer,
                       commiterEmail,
                       changesetShort,
                       parents);
        }

        private IObservable<IEnumerable<string>> ExtractLogParameter(
            GitBranch branch,
            int skip,
            int limit,
            GitLogOptions logOptions,
            string revisionRange)
        {
            IList<string> arguments = new List<string>();

            arguments.Add($"{revisionRange} ");

            if (branch != null)
            {
                arguments.Add($"--branches={branch.FriendlyName} ");
            }

            if (skip > 0)
            {
                arguments.Add($"--skip={skip}");
            }

            if (limit > 0)
            {
                arguments.Add($"--max-count={limit}");
            }

            arguments.Add("--full-history");

            if (logOptions.HasFlag(GitLogOptions.TopologicalOrder))
            {
                arguments.Add("--topo-order");
            }

            if (!logOptions.HasFlag(GitLogOptions.IncludeMerges))
            {
                arguments.Add("--no-merges");
                arguments.Add("--first-parent");
            }

            GenerateFormat(arguments);

            var argumentsObservable = Observable.Return(arguments);

            if (logOptions.HasFlag(GitLogOptions.BranchOnlyAndParent))
            {
                argumentsObservable = argumentsObservable.CombineLatest(
                    GetLocalBranches().Where(x => x != branch).Select(x => x.FriendlyName).ToList().Select(x => $"--not {string.Join(" ", x)} --"),
                    (arg, branches) => arg.Concat(new[] { branches }).ToList());
            }

            return argumentsObservable;
        }

        private void GetCurrentCheckedOutBranch()
        {
            _gitProcessManager.RunGit(new[] { "branch" }).Where(x => x.StartsWith("*", StringComparison.InvariantCulture)).Select(
                line => new GitBranch(line.Substring(2), false, true)).Subscribe(_currentBranch.OnNext);
        }
    }
}
