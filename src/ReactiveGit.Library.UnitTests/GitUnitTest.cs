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
using LibGit2Sharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReactiveGit.Library.Core.Exceptions;
using ReactiveGit.Library.Core.Model;
using ReactiveGit.Library.RunProcess.Managers;

namespace ReactiveGit.Library.UnitTests
{
    /// <summary>
    /// Tests the git framework.
    /// </summary>
    [TestClass]
    public class GitUnitTest
    {
        /////// <summary>
        /////// Test creating several branches and making sure that the full history comes back.
        /////// </summary>
        /////// <returns>A task to monitor the progress.</returns>
        ////[TestMethod]
        ////public async Task TestFullHistory()
        ////{
        ////    WriteLine("Executing " + nameof(TestFullHistory));

        ////    var (tempDirectory, local) = await GenerateGitRepository().ConfigureAwait(false);

        ////    var numberCommits = 10;
        ////    await GenerateCommits(numberCommits, tempDirectory, local, "master").ConfigureAwait(false);
        ////    var branchManager = new BranchManager(local);

        ////    var commits = await branchManager.GetCommitsForBranch(
        ////        new GitBranch("master", false, false),
        ////        0,
        ////        0,
        ////        GitLogOptions.BranchOnlyAndParent,
        ////        scheduler: ImmediateScheduler.Instance).ToList();

        ////    commits.Count.Should().Be(numberCommits, $"We have done {numberCommits} commits");

        ////    commits.Should().BeInDescendingOrder(x => x.DateTime);
        ////    await local.RunGit(new[] { "branch test1" }).ObserveOn(ImmediateScheduler.Instance).FirstOrDefaultAsync();
        ////    await local.RunGit(new[] { "checkout test1" }).ObserveOn(ImmediateScheduler.Instance).FirstOrDefaultAsync();

        ////    await GenerateCommits(numberCommits, tempDirectory, local, "master").ConfigureAwait(false);

        ////    commits = await branchManager.GetCommitsForBranch(new GitBranch("test1", false, false), 0, 0, GitLogOptions.None).ObserveOn(ImmediateScheduler.Instance).ToList();

        ////    commits.Count.Should().Be(numberCommits * 2, $"We have done {numberCommits + 1} commits");
        ////}

        /////// <summary>
        /////// Test creating several commits and making sure the get commit message routine
        /////// returns all the commits from the selected parent.
        /////// </summary>
        /////// <returns>A task to monitor the progress.</returns>
        ////[TestMethod]
        ////public async Task TestGetAllCommitMessages()
        ////{
        ////    WriteLine("Executing " + nameof(TestGetAllCommitMessages));

        ////    var (tempDirectory, local) = await GenerateGitRepository().ConfigureAwait(false);

        ////    IList<string> commitNames = new List<string>();
        ////    var numberCommits = 10;
        ////    await GenerateCommits(numberCommits, tempDirectory, local, "master", commitNames).ConfigureAwait(false);

        ////    var branchManager = new BranchManager(local);
        ////    var commits = await branchManager.GetCommitsForBranch(
        ////            new GitBranch("test1", false, false),
        ////            0,
        ////            0,
        ////            GitLogOptions.TopologicalOrder,
        ////            scheduler: ImmediateScheduler.Instance).ToList();

        ////    var commitMessages = await Task.WhenAll(commits.Select(async x => await x.MessageLong)).ConfigureAwait(false);
        ////    commitMessages.Should().BeEquivalentTo(commitNames.Reverse());
        ////}

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
            await GenerateCommits(numberCommits, tempDirectory, local, "master").ConfigureAwait(false);

            var branchManager = new BranchManager(local);

            var commits = await branchManager.GetCommitsForBranch(
                    new GitBranch("master", false, false),
                    0,
                    0,
                    GitLogOptions.BranchOnlyAndParent).ObserveOn(Scheduler.Immediate).ToList();
            commits.Count.Should().Be(numberCommits, $"We have done {numberCommits} commits");

            using (var repository = new Repository(tempDirectory))
            {
                var branch = repository.Branches.FirstOrDefault(x => x.FriendlyName == "master");
                branch.Should().NotBeNull();

                if (branch != null)
                {
                    CheckCommits(branch.Commits.ToList(), commits);
                }
            }

            commits.Should().BeInDescendingOrder(x => x.DateTime);

            await GenerateCommits(numberCommits, tempDirectory, local, "test1").ConfigureAwait(false);

            commits = await branchManager.GetCommitsForBranch(
                    new GitBranch("test1", false, false),
                    0,
                    0,
                    GitLogOptions.BranchOnlyAndParent).ObserveOn(Scheduler.Immediate).ToList();

            commits.Count.Should().Be(numberCommits, $"We have done {numberCommits} commits");

            using (var repository = new Repository(tempDirectory))
            {
                var branch = repository.Branches.FirstOrDefault(x => x.FriendlyName == "test1");
                branch.Should().NotBeNull();

                if (branch != null)
                {
                    CheckCommits(branch.Commits.Take(10).ToList(), commits);
                }
            }
        }

        /// <summary>
        /// Checks to make sure that the selected GIT commits are equal to each other.
        /// </summary>
        /// <param name="repoCommits">The commits coming from LibGit2.</param>
        /// <param name="commits">The commits coming from the GIT command line BranchManager.</param>
        private static void CheckCommits(IList<Commit> repoCommits, IList<GitCommit> commits)
        {
            repoCommits.Select(x => x.Sha).Should().BeEquivalentTo(commits.Select(x => x.Sha));
            repoCommits.Select(x => x.MessageShort).Should().BeEquivalentTo(commits.Select(x => x.MessageShort));
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
            return (tempDirectory, local);
        }

        /// <summary>
        /// Generates a series of commits.
        /// </summary>
        /// <param name="numberCommits">The number of commits to generate.</param>
        /// <param name="directory">The directory of the repository.</param>
        /// <param name="local">The repository manager for the repository.</param>
        /// <param name="branchName">The branch name to add the commits into.</param>
        /// <param name="commitMessages">A optional output list which is populated with the commit messages.</param>
        /// <returns>A task to monitor the progress.</returns>
        private static async Task GenerateCommits(
            int numberCommits,
            string directory,
            IGitProcessManager local,
            string branchName,
            IList<string> commitMessages = null)
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

            for (var i = 0; i < numberCommits; ++i)
            {
                File.WriteAllText(Path.Combine(directory, Path.GetRandomFileName()), @"Hello World" + i);
                await local.RunGit(new[] { "add -A" }).ObserveOn(ImmediateScheduler.Instance).LastOrDefaultAsync();
                commitMessages?.Add($"Commit {branchName}-{i}");
                await local.RunGit(new[] { $"commit -m \"Commit {branchName}-{i}\"" }).ObserveOn(ImmediateScheduler.Instance).LastOrDefaultAsync();
            }
        }

        private static void WriteLine(string format, params string[] args)
        {
            Console.WriteLine(format, args);
            Console.Out.Flush();
        }
    }
}