// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using ReactiveGITLibrary.Core.Exceptions;
using ReactiveGITLibrary.Core.ExtensionMethods;
using ReactiveGITLibrary.Core.Managers;
using ReactiveGITLibrary.Core.Model;

namespace ReactiveGit.Library.RunProcess.Managers
{
    /// <summary>
    /// Class responsible for handling GIT rebases.
    /// </summary>
    public class RebaseManager : IRebaseManager
    {
        private readonly IBranchManager _branchManager;

        private readonly IGitProcessManager _gitProcess;

        /// <summary>
        /// Initializes a new instance of the <see cref="RebaseManager" /> class.
        /// </summary>
        /// <param name="processManager">The process manager which invokes GIT commands.</param>
        /// <param name="branchManager">The branch manager which will get's GIT branch information.</param>
        public RebaseManager(IGitProcessManager processManager, IBranchManager branchManager)
        {
            _gitProcess = processManager;
            _branchManager = branchManager;
        }

        /// <summary>
        /// Gets the writers names.
        /// </summary>
        /// <param name="rebaseWriter">The rebase name.</param>
        /// <param name="commentWriter">The comment name.</param>
        /// <returns>The commit.</returns>
        public static bool GetWritersName(out string rebaseWriter, out string commentWriter)
        {
            rebaseWriter = null;
            commentWriter = null;

            try
            {
                var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase);
                var location = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);

                if (!File.Exists(location))
                {
                    location = Assembly.GetExecutingAssembly().Location;

                    if (string.IsNullOrWhiteSpace(location))
                    {
                        throw new GitProcessException("Cannot find location of writers");
                    }

                    location = Uri.UnescapeDataString(location);
                }

                if (string.IsNullOrWhiteSpace(location))
                {
                    return false;
                }

                var directoryName = Path.GetDirectoryName(location);

                if (string.IsNullOrWhiteSpace(directoryName))
                {
                    return false;
                }

                rebaseWriter = Path.Combine(directoryName, "rebasewriter.exe").Replace(@"\", "/");
                commentWriter = Path.Combine(directoryName, "commentWriter.exe").Replace(@"\", "/");
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (PathTooLongException)
            {
                return false;
            }
        }

        /// <inheritdoc />
        public IObservable<Unit> Abort(IScheduler scheduler = null)
        {
            return _gitProcess.RunGit(new[] { "rebase --abort" }, showInOutput: true, scheduler: scheduler).WhenDone();
        }

        /// <inheritdoc />
        public IObservable<Unit> ContinueRebase(string commitMessage, IScheduler scheduler = null)
        {
            return Observable.Create<Unit>(
                observer =>
                    {
                        if (!GetWritersName(out var rewriterName, out var commentWriterName))
                        {
                            observer.OnError(new GitProcessException("Cannot get valid paths to GIT parameters"));
                        }

                        var fileName = Path.GetTempFileName();
                        File.WriteAllText(fileName, commitMessage);

                        IList<string> gitArguments = new List<string>
                                                         {
                                                             $"-c \"core.editor=\'{commentWriterName}\'\"",
                                                             "rebase --continue"
                                                         };

                        var environmentVariables = new Dictionary<string, string> { { "COMMENT_FILE_NAME", fileName } };

                        var running = _gitProcess.RunGit(gitArguments, environmentVariables, showInOutput: true, scheduler: scheduler).Subscribe(
                            _ => { },
                            observer.OnError,
                            () =>
                            {
                                observer.OnNext(Unit.Default);
                                observer.OnCompleted();
                            });

                        return Disposable.Create(() => running?.Dispose());
                    });
        }

        /// <inheritdoc />
        public IObservable<bool> HasConflicts(IScheduler scheduler = null)
        {
            return _branchManager.IsMergeConflict(scheduler);
        }

        /// <inheritdoc />
        public bool IsRebaseHappening()
        {
            var isFile = Directory.Exists(Path.Combine(_gitProcess.RepositoryPath, ".git/rebase-apply"));

            return isFile || Directory.Exists(Path.Combine(_gitProcess.RepositoryPath, ".git/rebase-merge"));
        }

        /// <inheritdoc />
        public IObservable<Unit> Rebase(GitBranch parentBranch, IScheduler scheduler = null)
        {
            return Observable.Create<Unit>(
                async (observer, token) =>
                    {
                        if (await _branchManager.IsWorkingDirectoryDirty())
                        {
                            observer.OnError(
                                new GitProcessException("The working directory is dirty. There are un-committed files."));
                        }

                        IList<string> gitArguments = new List<string> { $"rebase -i  {parentBranch.FriendlyName}" };

                        _gitProcess.RunGit(gitArguments, showInOutput: true, scheduler: scheduler).Subscribe(
                            _ => { },
                            observer.OnError,
                            () =>
                                {
                                    observer.OnNext(Unit.Default);
                                    observer.OnCompleted();
                                }, token);
                    });
        }

        /// <inheritdoc />
        public IObservable<Unit> Skip(IScheduler scheduler = null)
        {
            return _gitProcess.RunGit(new[] { "rebase --skip" }, showInOutput: true, scheduler: scheduler).WhenDone();
        }

        /// <inheritdoc />
        public IObservable<Unit> Squash(string newCommitMessage, GitCommit startCommit, IScheduler scheduler = null)
        {
            return Observable.Create<Unit>(
                async (observer, token) =>
                    {
                        if (await _branchManager.IsWorkingDirectoryDirty())
                        {
                            observer.OnError(
                                new GitProcessException("The working directory is dirty. There are un-committed files."));
                        }

                        if (!GetWritersName(out var rewriterName, out var commentWriterName))
                        {
                            observer.OnError(new GitProcessException("Cannot get valid paths to GIT parameters"));
                        }

                        var fileName = Path.GetTempFileName();
                        File.WriteAllText(fileName, newCommitMessage);

                        var environmentVariables = new Dictionary<string, string> { { "COMMENT_FILE_NAME", fileName } };

                        IList<string> gitArguments = new List<string>
                                                         {
                                                             $"-c \"sequence.editor=\'{rewriterName}\'\"",
                                                             $"-c \"core.editor=\'{commentWriterName}\'\"",
                                                             $"rebase -i  {startCommit.Sha}"
                                                         };

                        _gitProcess.RunGit(gitArguments, environmentVariables, showInOutput: true, scheduler: scheduler).Subscribe(
                            _ => { },
                            observer.OnError,
                            () =>
                            {
                                observer.OnNext(Unit.Default);
                                observer.OnCompleted();
                            }, token);
                    });
        }
    }
}
