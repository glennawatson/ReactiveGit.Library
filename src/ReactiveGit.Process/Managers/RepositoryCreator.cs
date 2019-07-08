﻿// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using ReactiveGit.Core.Exceptions;
using ReactiveGit.Core.Managers;
using ReactiveGit.RunProcess.Helpers;

namespace ReactiveGit.RunProcess.Managers
{
    /// <summary>
    /// Creates a new repository in a directory if the directory is empty.
    /// </summary>
    public class RepositoryCreator : IRepositoryCreator
    {
        private readonly Func<string, IGitProcessManager> _processManagerFunc;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryCreator" /> class.
        /// </summary>
        /// <param name="processManagerFunc">Function to creating a git process.</param>
        public RepositoryCreator(Func<string, IGitProcessManager> processManagerFunc)
        {
            _processManagerFunc = processManagerFunc;
        }

        /// <summary>
        /// Creates a repository.
        /// </summary>
        /// <param name="directoryPath">The path to the new repository.</param>
        /// <param name="scheduler">The scheduler to use when creating the repository.</param>
        /// <returns>An observable monitoring the action.</returns>
        public IObservable<Unit> CreateRepository(string directoryPath, IScheduler scheduler = null)
        {
            return Observable.Create<Unit>(
                observer =>
                    {
                        if (!Directory.Exists(directoryPath))
                        {
                            throw new GitProcessException("Cannot find directory");
                        }

                        if (!FileHelper.IsDirectoryEmpty(directoryPath))
                        {
                            throw new GitProcessException("The directory is not empty.");
                        }

                        var gitProcess = _processManagerFunc(directoryPath);
                        return gitProcess.RunGit(new[] { "init" }, showInOutput: true, scheduler: scheduler).Subscribe(
                            _ => { },
                            observer.OnError,
                            () =>
                                {
                                    observer.OnNext(Unit.Default);
                                    observer.OnCompleted();
                                });
                    });
        }
    }
}