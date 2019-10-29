// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReactiveGit.Library.RunProcess.Managers;
using ReactiveGITLibrary.Core.Exceptions;
using ReactiveGITLibrary.Core.Model;
using Splat;

namespace ReactiveGit.Library.UnitTests
{
    /// <summary>
    /// Tests the git framework.
    /// </summary>
    [TestClass]
    public class GitUnitTest
    {
        /// <summary>
        /// Initializes static members of the <see cref="GitUnitTest"/> class.
        /// </summary>
        static GitUnitTest()
        {
            Splat.Locator.CurrentMutable.RegisterConstant<ILogger>(new ConsoleLogger());
        }

        /// <summary>
        /// Test creating several branches and making sure that the full history comes back.
        /// </summary>
        /// <returns>A task to monitor the progress.</returns>
        [TestMethod]
        public async Task TestFullHistory()
        {
            WriteLine("Executing " + nameof(TestFullHistory));

            var (tempDirectory, local) = await GenerateGitRepository().ConfigureAwait(false);

            var numberCommits = 10;
            await GenerateCommits(numberCommits, tempDirectory, local, "master").ConfigureAwait(false);
            var branchManager = new BranchManager(local);

            var commits = await branchManager.GetCommitsForBranch(
                new GitBranch("master", false, false),
                0,
                0,
                GitLogOptions.BranchOnlyAndParent).ObserveOn(ImmediateScheduler.Instance).ToList();

            commits.Count.Should().Be(numberCommits, $"We have done {numberCommits} commits");

            commits.Should().BeInDescendingOrder(x => x.DateTime);
            await local.RunGit(new[] { "branch test1" }).ObserveOn(ImmediateScheduler.Instance).FirstOrDefaultAsync();
            await local.RunGit(new[] { "checkout test1" }).ObserveOn(ImmediateScheduler.Instance).FirstOrDefaultAsync();

            await GenerateCommits(numberCommits, tempDirectory, local, "master").ConfigureAwait(false);

            commits = await branchManager.GetCommitsForBranch(new GitBranch("test1", false, false), 0, 0, GitLogOptions.None).ObserveOn(ImmediateScheduler.Instance).ToList();

            commits.Count.Should().Be(numberCommits * 2, $"We have done {numberCommits + 1} commits");
        }

        /// <summary>
        /// Test creating several commits and making sure the get commit message routine
        /// returns all the commits from the selected parent.
        /// </summary>
        /// <returns>A task to monitor the progress.</returns>
        [TestMethod]
        public async Task TestGetAllCommitMessages()
        {
            WriteLine("Executing " + nameof(TestGetAllCommitMessages));

            var (tempDirectory, local) = await GenerateGitRepository().ConfigureAwait(false);

            var numberCommits = 10;
            var commitNames = await GenerateCommits(numberCommits, tempDirectory, local, "master").ConfigureAwait(false);

            var branchManager = new BranchManager(local);
            var commits = await branchManager.GetCommitsForBranch(
                    new GitBranch("test1", false, false),
                    0,
                    0,
                    GitLogOptions.TopologicalOrder).ObserveOn(ImmediateScheduler.Instance).ToList();

            var commitMessages = await Task.WhenAll(commits.Select(async x => await x.MessageLong)).ConfigureAwait(false);
            commitMessages.Should().BeEquivalentTo(commitNames.Reverse());
        }

        /// <summary>
        /// Test getting the history from a freshly formed GIT repository and
        /// getting the history for the current branch only.
        /// </summary>
        /// <returns>A task to monitor the progress.</returns>
        [TestMethod]
        public async Task TestGitHistoryBranchOnly()
        {
            WriteLine("Executing " + nameof(TestGitHistoryBranchOnly));
            var (tempDirectory, local) = await GenerateGitRepository().ConfigureAwait(false);

            var numberCommits = 10;
            var commitMessages = await GenerateCommits(numberCommits, tempDirectory, local, "master").ConfigureAwait(false);

            var branchManager = new BranchManager(local);

            var commits = await branchManager.GetCommitsForBranch(
                    new GitBranch("master", false, false),
                    0,
                    0,
                    GitLogOptions.BranchOnlyAndParent).ObserveOn(Scheduler.Immediate).ToList();
            commits.Count.Should().Be(numberCommits, $"We have done {numberCommits} commits");

            CheckCommits(commitMessages, commits);

            commits.Should().BeInDescendingOrder(x => x.DateTime);

            commitMessages = await GenerateCommits(numberCommits, tempDirectory, local, "test1").ConfigureAwait(false);

            commits = await branchManager.GetCommitsForBranch(
                    new GitBranch("test1", false, false),
                    0,
                    0,
                    GitLogOptions.BranchOnlyAndParent).ObserveOn(Scheduler.Immediate).ToList();

            commits.Count.Should().Be(numberCommits, $"We have done {numberCommits} commits");

            CheckCommits(commitMessages, commits);
        }

        /// <summary>
        /// Checks to make sure that the selected GIT commits are equal to each other.
        /// </summary>
        /// <param name="repoCommits">The commits coming from LibGit2.</param>
        /// <param name="commits">The commits coming from the GIT command line BranchManager.</param>
        private static void CheckCommits(IList<string> repoCommits, IList<GitCommit> commits)
        {
            repoCommits.Should().BeEquivalentTo(commits.Select(x => x.MessageShort));
        }

        /// <summary>
        /// Generates a GIT repository with the specified process manager.
        /// </summary>
        /// <returns>The location of the GIT repository.</returns>
        private static async Task<(string tempDirectory, IGitProcessManager processManager)> GenerateGitRepository()
        {
            WriteLine("Executing " + nameof(GenerateGitRepository));
            var tempDirectory = Path.Combine(
                Path.GetTempPath(),
                Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            Directory.CreateDirectory(tempDirectory);

            var local = new GitProcessManager(tempDirectory);

            await local.RunGit(new[] { "init" }).ObserveOn(ImmediateScheduler.Instance).LastOrDefaultAsync();
            await local.RunGit(new[] { "config --local commit.gpgsign false" }).ObserveOn(ImmediateScheduler.Instance).LastOrDefaultAsync();
            await local.RunGit(new[] { "config --local user.email you@example.com" }).ObserveOn(ImmediateScheduler.Instance).LastOrDefaultAsync();
            await local.RunGit(new[] { "config --local user.name Name" }).ObserveOn(ImmediateScheduler.Instance).LastOrDefaultAsync();
            return (tempDirectory, local);
        }

        /// <summary>
        /// Generates a series of commits.
        /// </summary>
        /// <param name="numberCommits">The number of commits to generate.</param>
        /// <param name="directory">The directory of the repository.</param>
        /// <param name="local">The repository manager for the repository.</param>
        /// <param name="branchName">The branch name to add the commits into.</param>
        /// <returns>A task to monitor the progress.</returns>
        private static async Task<IList<string>> GenerateCommits(
            int numberCommits,
            string directory,
            IGitProcessManager local,
            string branchName)
        {
            WriteLine("Executing " + nameof(GenerateCommits));
            if (branchName != "master")
            {
                await local.RunGit(new[] { $"branch {branchName}" }).ObserveOn(ImmediateScheduler.Instance).LastOrDefaultAsync();
            }

            try
            {
                await local.RunGit(new[] { $"checkout {branchName}" }).ObserveOn(ImmediateScheduler.Instance).LastOrDefaultAsync();
            }
            catch (GitProcessException)
            {
                // Ignored
            }

            var commitNames = Enumerable.Range(0, numberCommits).Select(i => branchName + " Hello World " + i).ToList();
            foreach (var commitName in commitNames)
            {
                File.WriteAllText(Path.Combine(directory, Path.GetRandomFileName()), commitName);
                await local.RunGit(new[] { "add -A" }).ObserveOn(ImmediateScheduler.Instance).LastOrDefaultAsync();
                await local.RunGit(new[] { $"commit -m \"{commitName}\"" }).ObserveOn(ImmediateScheduler.Instance).LastOrDefaultAsync();
            }

            return commitNames;
        }

        private static void WriteLine(string format, params string[] args)
        {
            Console.WriteLine(format, args);
            Console.Out.Flush();
        }
    }
}